using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace Even.Commands;

public sealed class Command(string name, string category, string description, Action action, params string[] keywords)
{
    public string Name { get; } = name;
    public string Category { get; } = category ?? "Uncategorized";
    public string Description { get; } = description ?? string.Empty;
    public Action Action { get; } = action ?? throw new ArgumentNullException(nameof(action));
    public List<string> Keywords { get; } = keywords == null ? [] : [.. keywords];
    
    public Command(string name, Action action, params string[] keywords)
        : this(name, "Custom", string.Empty, action, keywords)
    {
    }

    /// <summary>
    /// Creates the full command list: built-in (discovered via IBuiltInCommandSource) + custom (CommandAPI registry)
    /// </summary>
    public static List<Command> CreateAll()
    {
        var builtIn = Loader.LoadBuiltIn();
        var custom = CommandAPI.GetRegisteredSnapshot();
        return Combine(builtIn, custom);
    }

    public static List<Command> Combine(List<Command> builtIn, List<Command> custom)
    {
        builtIn ??= [];
        custom ??= [];

        var all = new List<Command>(builtIn.Count + custom.Count);
        all.AddRange(builtIn);

        var seenNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var c in builtIn)
        {
            if (!string.IsNullOrWhiteSpace(c?.Name))
                seenNames.Add(c.Name.Trim());
        }

        foreach (var c in custom)
        {
            if (c == null) continue;

            var name = c.Name?.Trim();
            if (string.IsNullOrWhiteSpace(name) || seenNames.Add(name))
                all.Add(c);
        }

        return all;
    }

    public static string[] GetAllKeywords(List<Command> commands)
    {
        var keywords = new List<string>();
        foreach (var cmd in commands.Where(cmd => cmd != null))
        {
            if (!string.IsNullOrWhiteSpace(cmd.Name))
                keywords.Add(cmd.Name);

            if (cmd.Keywords == null) continue;
            keywords.AddRange(cmd.Keywords.Where(k => !string.IsNullOrWhiteSpace(k)));
        }

        return keywords.ToArray();
    }

    public static bool TryFindByRecognizedText(List<Command> commands, string recognizedText, out Command command)
    {
        command = null;

        if (string.IsNullOrWhiteSpace(recognizedText))
            return false;

        var recognized = Normalize(recognizedText);

        foreach (var cmd in commands)
        {
            if (cmd == null) continue;

            if (!string.IsNullOrWhiteSpace(cmd.Name))
            {
                var nameKey = Normalize(cmd.Name);
                if (IsMatch(recognized, nameKey))
                {
                    command = cmd;
                    return true;
                }
            }

            foreach (var keyword in cmd.Keywords)
            {
                if (string.IsNullOrWhiteSpace(keyword)) continue;

                var key = Normalize(keyword);
                if (IsMatch(recognized, key))
                {
                    command = cmd;
                    return true;
                }
            }
        }

        return false;
    }

    private static bool IsMatch(string recognizedNormalized, string keywordNormalized)
    {
        if (string.IsNullOrWhiteSpace(keywordNormalized))
            return false;

        if (string.Equals(recognizedNormalized, keywordNormalized, StringComparison.OrdinalIgnoreCase))
            return true;

        return recognizedNormalized.IndexOf(keywordNormalized, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static string Normalize(string s)
    {
        s = s.Trim().ToLowerInvariant();

        var sb = new StringBuilder(s.Length);
        var lastWasSpace = false;

        foreach (var ch in s)
        {
            if (char.IsLetterOrDigit(ch))
            {
                sb.Append(ch);
                lastWasSpace = false;
                continue;
            }

            if (char.IsWhiteSpace(ch))
            {
                if (!lastWasSpace)
                {
                    sb.Append(' ');
                    lastWasSpace = true;
                }
                continue;
            }

            if (!lastWasSpace)
            {
                sb.Append(' ');
                lastWasSpace = true;
            }
        }

        return sb.ToString().Trim();
    }
}