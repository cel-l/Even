using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Even.Commands;
using Even.Models;
using Logger = Even.Utils.Logger;
namespace Even;

public static class CommandAPI
{
    private static readonly object Sync = new();

    private static readonly List<Command> Registered = new();
    private static bool _sealed;

    /// <summary>
    /// Fired whenever the registry changes (add/remove/clear)
    /// Plugin can subscribe and rebuild KeywordRecognizer by recreating the assistant
    /// </summary>
    public static event Action RegistryChanged;

    /// <summary>
    /// Register a custom command. This is allowed even after Even's initial build;
    /// Even can rebuild its KeywordRecognizer when RegistryChanged fires
    /// </summary>
    public static bool Register(Command command)
    {
        if (command == null) throw new ArgumentNullException(nameof(command));

        lock (Sync)
        {
            // NOTE: Do NOT block late registration. We support dynamic rebuilds.
            if (_sealed)
                Logger.Info($"CommandAPI.Register called after initial seal. Accepting and requesting rebuild. Command: '{command.Name}'");

            Registered.Add(command);
            Logger.Info($"CommandAPI.Registered custom command: '{command.Name}'");
        }

        SafeRaiseRegistryChanged();
        return true;
    }

    /// <summary>
    /// Async-friendly registration helper (doesn't block caller thread)
    /// </summary>
    public static Task<bool> RegisterAsync(Command command, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        // Registration itself is fast; returning Task enables async pipelines/callers
        return Task.FromResult(Register(command));
    }

    /// <summary>
    /// Remove commands by name (case-insensitive). Returns number removed
    /// </summary>
    public static int UnregisterByName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return 0;

        int removed;
        lock (Sync)
        {
            removed = Registered.RemoveAll(c => c?.Name != null &&
                                               string.Equals(c.Name.Trim(), name.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        if (removed > 0)
        {
            Logger.Info($"CommandAPI.UnregisterByName removed {removed} command(s) named '{name}'.");
            SafeRaiseRegistryChanged();
        }

        return removed;
    }

    /// <summary>
    /// Clears all registered custom commands. Returns number cleared
    /// </summary>
    public static int Clear()
    {
        int cleared;
        lock (Sync)
        {
            cleared = Registered.Count;
            Registered.Clear();
        }

        if (cleared > 0)
        {
            Logger.Info($"CommandAPI.Clear removed {cleared} custom command(s).");
            SafeRaiseRegistryChanged();
        }

        return cleared;
    }

    /// <summary>
    /// Point-in-time snapshot
    /// </summary>
    public static List<Command> GetRegisteredSnapshot()
    {
        lock (Sync)
        {
            return [..Registered];
        }
    }

    /// <summary>
    /// Used by Even itself: waits briefly to allow other plugins to register commands, then seals and returns snapshot
    /// </summary>
    internal static async Task<List<Command>> SealAndGetRegisteredCommandsAsync(
        TimeSpan registrationWindow,
        CancellationToken cancellationToken = default)
    {
        if (registrationWindow < TimeSpan.Zero)
            registrationWindow = TimeSpan.Zero;

        if (registrationWindow > TimeSpan.Zero)
        {
            try
            {
                await Task.Delay(registrationWindow, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // proceed
            }
        }

        lock (Sync)
        {
            _sealed = true;
            return [..Registered];
        }
    }

    internal static void ResetForDebugOnly()
    {
        lock (Sync)
        {
            Registered.Clear();
            _sealed = false;
        }

        SafeRaiseRegistryChanged();
    }

    private static void SafeRaiseRegistryChanged()
    {
        try
        {
            RegistryChanged?.Invoke();
        }
        catch (Exception e)
        {
            Logger.Warning($"CommandAPI.RegistryChanged handler threw: {e}");
        }
    }
}