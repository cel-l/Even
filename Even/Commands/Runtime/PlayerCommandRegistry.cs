using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Even.Utils;
using Photon.Pun;
using Photon.Realtime;

namespace Even.Commands.Runtime;

/// <summary>
/// Maintains per-player (Photon actor) commands that follow the pattern:
///   "{action} (player name)"
///
/// - Action templates can be registered at any time (even before the registry instance exists).
/// - When a player joins, the registry instantiates all templates for that player.
/// - When a player leaves, the registry unregisters all commands created for that actor.
/// </summary>
public sealed class PlayerCommandRegistry
{
    public static PlayerCommandRegistry Instance { get; private set; }

    private sealed class ActionTemplate
    {
        public string ActionVerb { get; set; }
        public string Category { get; set; }
        public Func<Player, string> DescriptionFactory { get; set; }
        public Action<Player> Action { get; set; }
    }

    private static readonly List<ActionTemplate> PendingTemplates = new();

    private readonly Dictionary<int, List<string>> _registeredByActor = new();
    private readonly Dictionary<string, int> _phraseKeyToActor = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<ActionTemplate> _templates = new();

    public PlayerCommandRegistry()
    {
        Instance = this;

        if (PendingTemplates.Count <= 0) return;
        _templates.AddRange(PendingTemplates);
        PendingTemplates.Clear();
    }

    /// <summary>
    /// Register a per-player action template (e.g., actionVerb: "mute").
    /// </summary>
    public static void RegisterPlayerActionTemplate(
        string actionVerb,
        string category,
        Func<Player, string> descriptionFactory,
        Action<Player> action)
    {
        if (string.IsNullOrWhiteSpace(actionVerb))
            throw new ArgumentException("Action verb cannot be null/empty.", nameof(actionVerb));
        if (string.IsNullOrWhiteSpace(category))
            throw new ArgumentException("Category cannot be null/empty.", nameof(category));
        descriptionFactory ??= _ => "";
        action ??= _ => { };

        var template = new ActionTemplate
        {
            ActionVerb = actionVerb.Trim().ToLowerInvariant(),
            Category = category.Trim(),
            DescriptionFactory = descriptionFactory,
            Action = action
        };
        
        if (Instance == null)
        {
            PendingTemplates.Add(template);
            return;
        }

        Instance._templates.Add(template);

        if (!PhotonNetwork.InRoom) return;
        var players = PhotonNetwork.PlayerList;
        if (players == null) return;
        foreach (var p in players)
            Instance.RegisterTemplateCommandsForPlayer(p, template);
    }

    public void OnJoinedRoom()
    {
        var players = PhotonNetwork.PlayerList;
        if (players == null) return;

        foreach (var p in players)
            OnPlayerEnteredRoom(p);
    }

    public void OnLeftRoom()
    {
        foreach (var kvp in _registeredByActor)
        {
            foreach (var commandName in kvp.Value)
                CommandAPI.UnregisterByName(commandName);
        }

        _registeredByActor.Clear();
        _phraseKeyToActor.Clear();
    }

    public void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (newPlayer == null)
            return;

        foreach (var template in _templates)
            RegisterTemplateCommandsForPlayer(newPlayer, template);
    }

    public void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (otherPlayer == null)
            return;

        UnregisterAllForActor(otherPlayer.ActorNumber);
    }

    private void RegisterTemplateCommandsForPlayer(Player player, ActionTemplate template)
    {
        if (player == null)
            return;

        var actor = player.ActorNumber;

        var normalizedNick = NormalizeNickname(player.NickName);
        if (string.IsNullOrWhiteSpace(normalizedNick))
            normalizedNick = $"player {actor}";
        
        var phraseKey = normalizedNick;
        var suffix = 2;
        while (_phraseKeyToActor.TryGetValue(phraseKey, out var existingActor) && existingActor != actor)
        {
            phraseKey = $"{normalizedNick} {suffix}";
            suffix++;
        }

        _phraseKeyToActor[phraseKey] = actor;

        var phrase = $"{template.ActionVerb} {phraseKey}";
        var commandName = phrase;

        if (!_registeredByActor.TryGetValue(actor, out var list))
        {
            list = new List<string>();
            _registeredByActor[actor] = list;
        }

        if (list.Contains(commandName, StringComparer.OrdinalIgnoreCase))
            return;

        var cmd = new Command(
            name: commandName,
            category: template.Category,
            description: template.DescriptionFactory(player),
            action: () =>
            {
                var current = PhotonNetwork.CurrentRoom?.GetPlayer(actor) ?? player;
                template.Action(current);
            },
            keywords: [phrase]
        );

        CommandAPI.Register(cmd);
        list.Add(commandName);

        Logger.Info($"Registered player command: '{phrase}' (actor {actor}, nick '{player.NickName}')");
    }

    private void UnregisterAllForActor(int actorNumber)
    {
        if (!_registeredByActor.TryGetValue(actorNumber, out var commands))
            return;

        foreach (var commandName in commands)
            CommandAPI.UnregisterByName(commandName);

        _registeredByActor.Remove(actorNumber);

        var toRemove = new List<string>();
        foreach (var kvp in _phraseKeyToActor)
        {
            if (kvp.Value == actorNumber)
                toRemove.Add(kvp.Key);
        }

        foreach (var key in toRemove)
            _phraseKeyToActor.Remove(key);
    }

    private static string NormalizeNickname(string nickname)
    {
        nickname ??= "";
        nickname = nickname.Trim();
        nickname = Regex.Replace(nickname, @"\s+", " ");
        return nickname.ToLowerInvariant();
    }
}