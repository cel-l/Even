using System;
using Even.Utils;
using GorillaNetworking;
using UnityEngine;
using Logger = Even.Utils.Logger;
namespace Even.Commands.Default.Utility;

internal static class MicMode
{
    internal static void Set(string mode)
    {
        GorillaComputer.instance.pttType = mode;
        PlayerPrefs.SetString("pttType", GorillaComputer.instance.pttType);
        PlayerPrefs.Save();
        
        Audio.PlaySound("success", 0.74f);
    }
}

public sealed class ChangeMicOpenMic : IEvenCommand
{
    public Command Create()
    {
        return new Command(
            name: "change mic to open mic",
            category: "Utility",
            description: "Sets the voice mode to Open Mic",
            action: () =>
            {
                try
                {
                    MicMode.Set("OPEN MIC");
                }
                catch (Exception e)
                {
                    Logger.Error($"Failed to run command 'change mic to open mic': {e}");
                }
            },
            keywords: ["open mic"]
        );
    }
}

public sealed class ChangeMicPushToTalk : IEvenCommand
{
    public Command Create()
    {
        return new Command(
            name: "change mic to push to talk",
            category: "Utility",
            description: "Sets the voice mode to Push To Talk",
            action: () =>
            {
                try
                {
                    MicMode.Set("PUSH TO TALK");
                }
                catch (Exception e)
                {
                    Logger.Error($"Failed to run command 'change mic to push to talk': {e}");
                }
            },
            keywords: ["push to talk", "ptt"]
        );
    }
}

public sealed class ChangeMicPushToMute : IEvenCommand
{
    public Command Create()
    {
        return new Command(
            name: "change mic to push to mute",
            category: "Utility",
            description: "Sets the voice mode to Push To Mute",
            action: () =>
            {
                try
                {
                    MicMode.Set("PUSH TO MUTE");
                }
                catch (Exception e)
                {
                    Logger.Error($"Failed to run command 'change mic to push to mute': {e}");
                }
            },
            keywords: ["push to mute", "ptm"]
        );
    }
}