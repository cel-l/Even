using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using BepInEx;
using Even.Commands;
using Even.Commands.Runtime;
using Even.Interaction;
using Even.Utils;
using UnityEngine;
using Input = Even.Interaction.Input;
using DiscordRPC;
using DiscordRPC.Logging;
using Photon.Pun;

// ReSharper disable UseCollectionExpression
// ReSharper disable ArrangeObjectCreationWhenTypeEvident
// ReSharper disable RedundantExplicitArrayCreation

namespace Even;

[BepInPlugin("cel.even", Mod, Version)]
public class Plugin : BaseUnityPlugin
{
    public static Plugin Instance { get; private set; }

    public const string Mod = "Even";
    public const string Version = "1.0.1";
    public const string Alias = "<color=#8aadf4>E</color><color=#7aa2f7>v</color><color=#7289da>e</color><color=#5b6ee0>n</color>";

    private static bool s_embeddedResolverInstalled;

    private Voice _voice;
    private Input _input;
    private Assistant _wakeWordAssistant;
    public static Assistant AssistantInstance { get; private set; }
    private Models.Callbacks.Photon _photon;

    private PlayerCommandRegistry _playerCommands;

    private bool _hasInitialized;

    public List<Command> _commands;

    private bool _rebuildQueued;
    private float _rebuildAt;
    private const float RebuildDebounceSeconds = 0.25f;

    private static readonly string[] WakeAliases =
    [
        "hey even",
        "hey jarvis",
        "hey siri"
    ];

    private DiscordRpcClient _discordClient;
    private bool _discordInitialized;
    private bool _lastInRoomState;

    private void Awake()
    {
        Instance = this;
        InstallEmbeddedAssemblyResolver();

        CommandAPI.RegistryChanged += OnCommandRegistryChanged;
        Settings.Changed += OnSettingsChanged;

        Settings.Initialize();
    }

    private void CheckHasUsed()
    {
        if (Settings.IsEnabled("has_used"))
        {
            Notification.Show($"Loaded {_commands?.Count ?? 0} commands successfully", 0.8f, true, true);
            return;
        }
        
        try
        {
            Audio.PlaySound("wake.wav", 0.74f);
            _ = Audio.PlayVoiceSound("welcome", 0.74f);
            
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://discord.gg/2mDhbzTpdT",
                UseShellExecute = true
            });
                
            Settings.SetEnabled("has_used", true, true);
        }
        catch (Exception ex)
        {
            Utils.Logger.Error($"Failed to open URL: {ex}");
        }
    }
    
    private void OnDestroy()
    {
        CommandAPI.RegistryChanged -= OnCommandRegistryChanged;
        Settings.Changed -= OnSettingsChanged;

        _discordClient?.Dispose();
    }
    
    private void OnApplicationFocus(bool isFocused)
    {
        if (!isFocused)
        {
            Notification.Show("Application lost focus, voice recognition will not work until focused", 3f);
        }
    }
    
    private void OnSettingsChanged(Settings.Data data)
    {
        if (AssistantInstance)
            AssistantInstance.ApplySettings();
    }

    private void OnCommandRegistryChanged()
    {
        _rebuildQueued = true;
        _rebuildAt = Time.time + RebuildDebounceSeconds;
    }

    private static void InstallEmbeddedAssemblyResolver()
    {
        if (s_embeddedResolverInstalled) return;
        s_embeddedResolverInstalled = true;

        AppDomain.CurrentDomain.AssemblyResolve += (_, args) =>
        {
            try
            {
                var requested = new AssemblyName(args.Name).Name + ".dll";

                string[] embeddedDlls =
                [
                    "MonkeNotificationLib.dll"
                ];

                if (!embeddedDlls.Any(dll => string.Equals(requested, dll, StringComparison.OrdinalIgnoreCase)))
                    return null;

                var self = Assembly.GetExecutingAssembly();
                var resourceName = self
                    .GetManifestResourceNames()
                    .FirstOrDefault(n => n.EndsWith(requested, StringComparison.OrdinalIgnoreCase));

                if (resourceName == null)
                    return null;

                using var stream = self.GetManifestResourceStream(resourceName);
                if (stream == null)
                    return null;

                using var ms = new MemoryStream();
                stream.CopyTo(ms);
                return Assembly.Load(ms.ToArray());
            }
            catch
            {
                return null;
            }
        };
    }
    
    private void Start()
    {
        Notification.Show($"Loading...", 6f);
        if (CosmeticsV2Spawner_Dirty.OnPostInstantiateAllPrefabs2 == null)
            CosmeticsV2Spawner_Dirty.OnPostInstantiateAllPrefabs2 += Initialize;
        else
            Initialize();

        InitializeDiscordRPC();
    }

    private async void Initialize()
    {
        try
        {
            _commands = Command.CreateAll();

            _voice = gameObject.AddComponent<Voice>();
            _input = gameObject.AddComponent<Input>();

            _playerCommands = new PlayerCommandRegistry();

            _photon = gameObject.AddComponent<Models.Callbacks.Photon>();
            _photon.Initialize(_playerCommands);

            _voice.Initialize(WakeAliases);

            _wakeWordAssistant = gameObject.AddComponent<Assistant>();
            _wakeWordAssistant.Initialize(_voice, _commands, WakeAliases);
            _wakeWordAssistant.ApplySettings();
            AssistantInstance = _wakeWordAssistant;

            var versionCheck = await Network.Instance.FetchServerDataAsync(Version, NetworkSystem.Instance.LocalPlayer.UserId);

            if (versionCheck?.Outdated == true)
            {
                Notification.Show($"Mod is outdated! Latest: {versionCheck.LatestVersion}", 0.6f, true, true);
                enabled = false;

                CommandAPI.RegistryChanged -= OnCommandRegistryChanged;
                Settings.Changed -= OnSettingsChanged;

                Destroy(_voice);
                Destroy(_input);
                Destroy(_wakeWordAssistant);

                Destroy(this);
                return;
            }

            _hasInitialized = true;
            
            CheckHasUsed();
        }
        catch (Exception ex)
        {
            Utils.Logger.Error($"Failed to initialize: {ex}");
        }
    }
    
    private void InitializeDiscordRPC()
    {
        try
        {
            _discordClient = new DiscordRpcClient("1467006876230094878");
            _discordClient.Logger = new ConsoleLogger() { Level = LogLevel.Warning };
            _discordClient.Initialize();

            _discordInitialized = true;
            UpdateDiscordPresence(force: true);
        }
        catch (Exception ex)
        {
            Utils.Logger.Error($"Failed to initialize DiscordRPC: {ex}");
        }
    }
    
    private void UpdateDiscordPresence(bool force = false)
    {
        if (!_discordInitialized) return;

        var inRoom = NetworkSystem.Instance && NetworkSystem.Instance.InRoom;

        if (!force && inRoom == _lastInRoomState) return;

        _lastInRoomState = inRoom;

        var stateText = inRoom ? "In room" : "Not in room";

        var partySize = 1;
        const int partyMax = 10;

        if (inRoom && PhotonNetwork.CurrentRoom != null)
        {
            partySize = PhotonNetwork.CurrentRoom.PlayerCount;
        }

        _discordClient.SetPresence(new RichPresence
        {
            Details = "Gorilla Tag with Even",
            State = stateText,
            Assets = new DiscordRPC.Assets
            {
                LargeImageKey = "even_logo",
                LargeImageText = "Even",
            },
            Party = inRoom ? new Party
            {
                ID = NetworkSystem.Instance.RoomName,
                Size = partySize,
                Max = partyMax
            } : null,
            Buttons = new Button[]
            {
                new Button
                {
                    Label = "Get Even",
                    Url = "https://even.rest"
                }
            }
        });
    }

    private void Update()
    {
        if (!_hasInitialized || !_voice.IsReady) return;

        if (_rebuildQueued && Time.time >= _rebuildAt)
        {
            _rebuildQueued = false;
            RebuildCommandsAndRecognizer();
        }

        if (_input.LeftJoystickClick.WasPressed())
        {
            Utils.Logger.Info("left joystick clicked");
        }

        UpdateDiscordPresence();

        Settings.Tick();
    }

    private void RebuildCommandsAndRecognizer()
    {
        try
        {
            _commands = Command.CreateAll();

            if (_wakeWordAssistant)
            {
                _wakeWordAssistant.RefreshCommands(_commands);
                _wakeWordAssistant.ApplySettings();
            }
            else
            {
                _wakeWordAssistant = gameObject.AddComponent<Assistant>();
                _wakeWordAssistant.Initialize(_voice, _commands, WakeAliases);
                _wakeWordAssistant.ApplySettings();
            }

            Utils.Logger.Info($"Rebuilt commands. Command count: {_commands?.Count ?? 0}");
        }
        catch (Exception ex)
        {
            Utils.Logger.Error($"Failed to rebuild commands/recognizer: {ex}");
        }
    }
}