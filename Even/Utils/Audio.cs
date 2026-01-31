using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Photon.Pun;
using UnityEngine;

namespace Even.Utils;

public static class Audio
{
    private static readonly Dictionary<string, AudioClip> Cache = new(StringComparer.OrdinalIgnoreCase);
    private static AudioSource _source;

    public static void AttachTo(GameObject host)
    {
        if (!host) throw new ArgumentNullException(nameof(host));
        if (_source) return;

        _source = host.GetComponent<AudioSource>() ?? host.AddComponent<AudioSource>();
        _source.playOnAwake = false;
        _source.loop = false;
    }

    public static void PlaySound(string name, float volume = 1f)
    {
        if (!_source)
        {
            Logger.Error("Audio.AttachTo(host) must be called before playing audio.");
            return;
        }

        _ = PlaySoundAsync(name, volume);
    }

    private static async Task PlaySoundAsync(string name, float volume)
    {
        var clip = await LoadFromResourcesAsync(name);
        if (!clip) return;

        _source.PlayOneShot(clip, Mathf.Clamp01(volume));
    }

    public static Task<AudioClip> LoadFromResourcesAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return Task.FromResult<AudioClip>(null);

        var key = Path.GetFileNameWithoutExtension(name);

        if (Cache.TryGetValue(key, out var cached) && cached)
            return Task.FromResult(cached);

        var clip = Resources.Load<AudioClip>($"Sounds/{key}");
        if (clip)
        {
            Cache[key] = clip;
            return Task.FromResult(clip);
        }

        return LoadWavFromEmbeddedResourceAsync(key);
    }

    private static async Task<AudioClip> LoadWavFromEmbeddedResourceAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;

        var key = Path.GetFileNameWithoutExtension(name);

        if (Cache.TryGetValue(key, out var cached) && cached)
            return cached;

        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"Even.Assets.Resources.Sounds.{key}.wav";

        await using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            Logger.Warning($"Embedded audio resource not found: {resourceName}");
            return null;
        }

        var data = new byte[stream.Length];
        var readAsync = await stream.ReadAsync(data, 0, data.Length);

        var clip = WavUtility.ToAudioClip(data, key);
        if (!clip)
        {
            Logger.Warning($"Failed to convert WAV to AudioClip: {resourceName}");
            return null;
        }

        Cache[key] = clip;
        return clip;
    }

    public static void PlayPopSound()
    {
        var soundId = 84;

        if (NetworkSystem.Instance.InRoom)
        {
            GorillaTagger.Instance.myVRRig.SendRPC(
                "RPC_PlayHandTap",
                RpcTarget.All,
                soundId,
                false,
                0.8f
            );
        }
        else
        {
            VRRig.LocalRig.PlayHandTapLocal(soundId, false, 0.8f);
        }
    }
    
    public static Task PlayVoiceSound(string message, float volume = 1f)
    {
        if (string.IsNullOrWhiteSpace(message)) return Task.CompletedTask;
        
        var fileName = Regex.Replace(message.ToLower(), @"[^\w]+", "_");
        
        PlaySound(fileName, volume);
        return Task.CompletedTask;
    }
}