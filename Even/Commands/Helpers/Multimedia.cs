using System;
using System.Runtime.InteropServices;
using Logger = Even.Utils.Logger;

namespace Even.Commands.Helpers;

public static class Multimedia
{
    private const uint KEYEVENTF_KEYUP = 0x0002;

    [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
    private static extern void keybd_event(uint bVk, uint bScan, uint dwFlags, uint dwExtraInfo);

    public static void PlayPause() => Send(MediaKeys.PlayPause);

    public static void NextTrack() => Send(MediaKeys.NextTrack);

    public static void PreviousTrack() => Send(MediaKeys.PreviousTrack);

    private static void Send(MediaKeys key)
    {
        try
        {
            keybd_event((uint)key, 0, 0, 0);
            keybd_event((uint)key, 0, KEYEVENTF_KEYUP, 0);
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed sending multimedia key '{key}': {ex}");
        }
    }

    private enum MediaKeys : uint
    {
        NextTrack = 0xB0,
        PreviousTrack = 0xB1,
        PlayPause = 0xB3
    }
}