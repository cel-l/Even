using System;
using System.Collections.Generic;
using System.Linq;
using Even.Commands;
using Even.Utils;
using MonkeNotificationLib;
using Logger = Even.Utils.Logger;
using UnityEngine;

namespace Even.Interaction;

public class Assistant : MonoBehaviour
{
    public float AwakeWindow { get; } = 20f;

    private Voice _voice;
    private List<Command> _commands;

    private float _awakeUntilTime;

    private enum AssistantState
    {
        Sleeping,
        Awake,
        Listening,
        ExecutingCommand,
        Cooldown
    }

    private AssistantState _state = AssistantState.Sleeping;

    private const float CommandCooldownSeconds = 1f;
    private float _cooldownUntilTime;

    private string[] _wakeKeywords = [];
    private readonly HashSet<string> _wakeKeywordSet = new(StringComparer.OrdinalIgnoreCase);

    private readonly string[] _wakeMessages =
    [
        "How can I help?",
        "I'm here!",
        "What's up?"
    ];

    public void Initialize(Voice voice, List<Command> commands, IEnumerable<string> wakeKeywords)
    {
        _voice = voice;
        _commands = commands;

        if (!_voice)
        {
            Logger.Error("Assistant.Initialize failed: Voice is null");
            return;
        }

        if (_commands == null)
        {
            Logger.Error("Assistant.Initialize failed: commands list is null");
            return;
        }

        SetWakeKeywords(wakeKeywords);

        Audio.AttachTo(gameObject);

        _voice.PhraseRecognized -= OnPhraseRecognized;
        _voice.PhraseRecognizedWithStartTime -= OnPhraseRecognizedWithStartTime;
        _voice.PhraseRecognizedWithStartTime += OnPhraseRecognizedWithStartTime;

        GoToSleep();
    }

    private void SetWakeKeywords(IEnumerable<string> wakeKeywords)
    {
        _wakeKeywordSet.Clear();

        var list = (wakeKeywords ?? [])
            .Select(s => (s ?? string.Empty).Trim())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (list.Length == 0)
            list = ["hey even"];

        _wakeKeywords = list;

        foreach (var k in _wakeKeywords)
            _wakeKeywordSet.Add(k);
    }

    private void Update()
    {
        if (_state == AssistantState.Sleeping)
            return;

        if (_state == AssistantState.Cooldown && Time.time >= _cooldownUntilTime)
        {
            _state = AssistantState.Listening;
            Logger.Info("Cooldown finished (back to listening)");
        }

        if (Time.time >= _awakeUntilTime)
            GoToSleep();
    }

    private void OnDestroy()
    {
        if (_voice != null)
        {
            _voice.PhraseRecognized -= OnPhraseRecognized;
            _voice.PhraseRecognizedWithStartTime -= OnPhraseRecognizedWithStartTime;
        }
    }

    private void OnPhraseRecognized(string text) { }

    private void OnPhraseRecognizedWithStartTime(string text, DateTime phraseStartTime)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        if (_state == AssistantState.Sleeping)
        {
            if (IsWakeWord(text))
                WakeUp();

            return;
        }

        if (_state == AssistantState.ExecutingCommand)
        {
            Logger.Info($"Ignoring phrase while executing: '{text}'");
            return;
        }

        if (_state == AssistantState.Cooldown)
        {
            Logger.Info($"Ignoring phrase during cooldown: '{text}'");
            return;
        }

        if (_state != AssistantState.Listening && _state != AssistantState.Awake)
        {
            Logger.Info($"Ignoring phrase in state {_state}: '{text}'");
            return;
        }

        if (Command.TryFindByRecognizedText(_commands, text, out var command))
        {
            _state = AssistantState.ExecutingCommand;
            Logger.Info($"Executing command: {command.Name}");

            try
            {
                command.Action?.Invoke();
            }
            catch (Exception ex)
            {
                Logger.Error($"Command '{command.Name}' threw: {ex}");
            }

            var nowUtc = DateTime.UtcNow;
            var startUtc = phraseStartTime.Kind == DateTimeKind.Utc
                ? phraseStartTime
                : phraseStartTime.ToUniversalTime();

            var latencySeconds = (float)(nowUtc - startUtc).TotalSeconds;
            if (latencySeconds < 0f) latencySeconds = 0f;

            var phraseTimeInUnity = Time.time - latencySeconds;
            _awakeUntilTime = phraseTimeInUnity + AwakeWindow;

            _cooldownUntilTime = Time.time + CommandCooldownSeconds;
            _state = AssistantState.Cooldown;

            Logger.Info("Wake window reset after command execution (entering cooldown)");
            return;
        }

        Logger.Info($"Awake but no command matched: '{text}'");
    }

    private bool IsWakeWord(string text)
    {
        var normalized = text.Trim();
        return _wakeKeywordSet.Contains(normalized);
    }

    private void WakeUp()
    {
        _state = AssistantState.Awake;
        _awakeUntilTime = Time.time + AwakeWindow;

        Audio.PlaySound("wake.wav", 1.3f);

        var index = UnityEngine.Random.Range(0, _wakeMessages.Length);
        var message = _wakeMessages[index];

        NotificationController.AppendMessage(Plugin.Alias, message, false, 0.6f);

        var commandKeywords = Command.GetAllKeywords(_commands);
        _voice.StartListening(commandKeywords);

        _state = AssistantState.Listening;
        Logger.Info("Even awakened (listening for commands)");
    }

    private void GoToSleep()
    {
        _state = AssistantState.Sleeping;
        _voice.StartListening(_wakeKeywords);

        Audio.PlaySound("sleep.wav", 2f);
        Logger.Info("Even asleep (listening for wake word)");
    }
}