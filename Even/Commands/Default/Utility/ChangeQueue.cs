using System;
using Even.Utils;
using GorillaNetworking;
using UnityEngine;
using Logger = Even.Utils.Logger;
namespace Even.Commands.Default.Utility;

internal static class QueueMode
{
    internal static void Set(string queue)
    {
        GorillaComputer.instance.currentQueue = queue;
        PlayerPrefs.Save();
        
        Audio.PlaySound("success", 0.74f);
    }
}

public sealed class ChangeQueueToDefault : IEvenCommand
{
    public Command Create()
    {
        return new Command(
            name: "change queue to default",
            category: "Utility",
            description: "Sets the matchmaking queue to Minigames",
            action: () =>
            {
                try
                {
                    QueueMode.Set("DEFAULT");
                }
                catch (Exception e)
                {
                    Logger.Error($"Failed to run command 'change queue to default': {e}");
                }
            },
            keywords: ["default queue"]
        );
    }
}

public sealed class ChangeQueueToMinigames : IEvenCommand
{
    public Command Create()
    {
        return new Command(
            name: "change queue to minigames",
            category: "Utility",
            description: "Sets the matchmaking queue to Minigames",
            action: () =>
            {
                try
                {
                    QueueMode.Set("MINIGAMES");
                }
                catch (Exception e)
                {
                    Logger.Error($"Failed to run command 'change queue to minigames': {e}");
                }
            },
            keywords: ["minigames queue"]
        );
    }
}

public sealed class ChangeQueueToCompetitive : IEvenCommand
{
    public Command Create()
    {
        return new Command(
            name: "change queue to competitive",
            category: "Utility",
            description: "Sets the matchmaking queue to Competitive",
            action: () =>
            {
                try
                {
                    QueueMode.Set("COMPETITIVE");
                }
                catch (Exception e)
                {
                    Logger.Error($"Failed to run command 'change queue to competitive': {e}");
                }
            },
            keywords: ["default queue"]
        );
    }
}
