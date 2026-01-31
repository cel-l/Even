using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using Newtonsoft.Json;
using UnityEngine;

namespace Even.Utils;

public static class Settings
{
    public static Data Current { get; private set; } = new();
    public static event Action<Data> Changed;

    public static bool IsInitialized { get; private set; }
    public static string FilePath { get; private set; } = "";

    private static bool _dirty;
    private static float _saveAt;
    private const float SaveDebounceSeconds = 0.5f;

    private static readonly Dictionary<string, bool> BuiltInDefaults = new(StringComparer.OrdinalIgnoreCase)
    {
        [Keys.Notifications] = true,
        [Keys.QuickCommands] = true
    };

    public static void Initialize(string folderName = "Even", string fileName = "settings.json")
    {
        if (IsInitialized) return;
        IsInitialized = true;

        var dir = Path.Combine(Paths.ConfigPath, folderName);
        Directory.CreateDirectory(dir);

        FilePath = Path.Combine(dir, fileName);
        LoadOrCreate();
    }

    public static void Tick()
    {
        if (!_dirty) return;
        if (Time.time < _saveAt) return;
        SaveNow();
    }

    /// <summary>
    /// Safely update settings and queue save
    /// </summary>
    public static void Update(Action<Data> mutator, bool saveImmediately = false)
    {
        mutator?.Invoke(Current);
        MarkDirty();
        Changed?.Invoke(Current);

        if (saveImmediately)
            SaveNow();
    }

    /// <summary>
    /// Load settings from disk or create defaults if missing
    /// </summary>
    public static void LoadOrCreate()
    {
        EnsurePath();

        if (!File.Exists(FilePath))
        {
            CreateDefaults();
            return;
        }

        try
        {
            var json = File.ReadAllText(FilePath);
            Current = JsonConvert.DeserializeObject<Data>(json) ?? new Data();
            MergeDefaults();
            Current.Normalize();
            Changed?.Invoke(Current);
        }
        catch (Exception e)
        {
            Logger.Error($"Settings load failed: {e}");
            CreateDefaults();
        }
    }

    /// <summary>
    /// Save settings immediately
    /// </summary>
    public static void SaveNow()
    {
        EnsurePath();

        try
        {
            Current.Normalize();
            var json = JsonConvert.SerializeObject(Current, Formatting.Indented);
            File.WriteAllText(FilePath, json);
            _dirty = false;
        }
        catch (Exception e)
        {
            Logger.Error($"Settings save failed: {e}");
            MarkDirty();
        }
    }

    /// <summary>
    /// Check if a feature flag is enabled
    /// </summary>
    public static bool IsEnabled(string key, bool defaultValue = false)
    {
        if (string.IsNullOrWhiteSpace(key))
            return defaultValue;

        key = NormalizeKey(key);
        if (BuiltInDefaults.TryGetValue(key, out var builtInDefault))
            defaultValue = builtInDefault;

        return Current.Flags.GetValueOrDefault(key, defaultValue);
    }

    /// <summary>
    /// Enable or disable a feature flag
    /// </summary>
    public static void SetEnabled(string key, bool enabled, bool saveImmediately = false)
    {
        if (string.IsNullOrWhiteSpace(key))
            return;

        key = NormalizeKey(key);
        Update(s => s.Flags[key] = enabled, saveImmediately);
    }

    /// <summary>
    /// Toggle a feature flag
    /// </summary>
    public static bool Toggle(string key, bool defaultValue = false, bool saveImmediately = false)
    {
        var next = !IsEnabled(key, defaultValue);
        SetEnabled(key, next, saveImmediately);
        return next;
    }

    private static void CreateDefaults()
    {
        Current = new Data();
        MergeDefaults();
        MarkDirty();
        Changed?.Invoke(Current);
        SaveNow();
    }

    private static void MergeDefaults()
    {
        foreach (var kv in BuiltInDefaults)
        {
            if (!Current.Flags.ContainsKey(kv.Key))
                Current.Flags[kv.Key] = kv.Value;
        }
    }

    private static void MarkDirty()
    {
        _dirty = true;
        _saveAt = Time.time + SaveDebounceSeconds;
    }

    private static void EnsurePath()
    {
        if (!string.IsNullOrWhiteSpace(FilePath)) return;

        var dir = Path.Combine(Paths.ConfigPath, "Even");
        Directory.CreateDirectory(dir);
        FilePath = Path.Combine(dir, "settings.json");
    }

    private static string NormalizeKey(string key)
    {
        return key.Trim().Replace(" ", "_").ToLowerInvariant();
    }

    /// <summary>
    /// Serializable settings payload
    /// </summary>
    public sealed class Data
    {
        public int SchemaVersion { get; set; } = 1;

        /// <summary>
        /// Generic feature flags
        /// </summary>
        public Dictionary<string, bool> Flags { get; set; } = new(StringComparer.OrdinalIgnoreCase);

        public void Normalize()
        {
            Flags ??= new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            Flags = Flags
                .Where(kv => !string.IsNullOrWhiteSpace(kv.Key))
                .ToDictionary(
                    kv => kv.Key.Trim().Replace(" ", "_").ToLowerInvariant(),
                    kv => kv.Value,
                    StringComparer.OrdinalIgnoreCase
                );
        }
    }

    /// <summary>
    /// Built-in keys (core Even flags)
    /// </summary>
    public static class Keys
    {
        public const string Notifications = "even_notifications";
        public const string QuickCommands = "even_quick_commands";
    }
}