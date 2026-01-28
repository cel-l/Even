using System;
using System.IO;
using UnityEngine;
// ReSharper disable UnusedVariable
// ReSharper disable UnusedMember.Local
namespace Even.Utils;

/// <summary>
/// This is a modified version of https://github.com/BepInEx/BepInEx/blob/master/BepInEx/Unity/WavUtility.cs
/// I did not make this class, I'm just using it as a reference
/// </summary>
public static class WavUtility
{
    const int HEADER_SIZE = 44;

    public static AudioClip ToAudioClip(byte[] fileBytes, string name = "wav")
    {
        using var stream = new MemoryStream(fileBytes);
        using var reader = new BinaryReader(stream);

        // RIFF header
        reader.ReadBytes(4); // "RIFF"
        int fileSize = reader.ReadInt32();
        reader.ReadBytes(4); // "WAVE"

        // fmt chunk
        reader.ReadBytes(4); // "fmt "
        int fmtSize = reader.ReadInt32();
        int audioFormat = reader.ReadInt16();
        int channels = reader.ReadInt16();
        int sampleRate = reader.ReadInt32();
        int byteRate = reader.ReadInt32();
        int blockAlign = reader.ReadInt16();
        int bitsPerSample = reader.ReadInt16();

        // skip any extra fmt bytes
        if (fmtSize > 16)
            reader.ReadBytes(fmtSize - 16);

        // data chunk header
        string dataID = new string(reader.ReadChars(4));
        while (dataID != "data")
        {
            int chunkSize = reader.ReadInt32();
            reader.ReadBytes(chunkSize);
            dataID = new string(reader.ReadChars(4));
        }

        int dataSize = reader.ReadInt32();
        byte[] data = reader.ReadBytes(dataSize);

        int sampleCount = dataSize / (bitsPerSample / 8);
        float[] samples = new float[sampleCount];

        if (bitsPerSample == 16)
        {
            int i = 0;
            for (int s = 0; s < sampleCount; s++)
            {
                short val = BitConverter.ToInt16(data, i);
                samples[s] = val / 32768f;
                i += 2;
            }
        }
        else if (bitsPerSample == 8)
        {
            for (int s = 0; s < sampleCount; s++)
                samples[s] = (data[s] - 128) / 128f;
        }
        else
        {
            Debug.LogWarning("Unsupported WAV format: " + bitsPerSample);
            return null;
        }

        var clip = AudioClip.Create(name, sampleCount / channels, channels, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}