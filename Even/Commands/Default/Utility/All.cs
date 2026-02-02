using System;
using System.Collections;
using System.Collections.Generic;
using Even.Commands.Helpers;
using Even.Utils;
using UnityEngine;
using Logger = Even.Utils.Logger;
namespace Even.Commands.Default.Utility;

public sealed class MuteAll : IEvenCommand
{
    public static IEnumerator DelayedUpdateRoutine()
    {
        yield return new WaitForSeconds(0.5f);
        GorillaScoreboardTotalUpdater.instance.UpdateActiveScoreboards();
    }

    public Command Create()
    {
        return new Command(
            name: "mute all",
            category: "Utility",
            description: "Mutes every player in the room",
            action: () =>
            {
                try
                {
                    if (!NetworkSystem.Instance.InRoom) return;
                    
                    foreach (var line in GorillaScoreboardTotalUpdater.allScoreboardLines)
                    {
                        line.PressButton(true, GorillaPlayerLineButton.ButtonType.Mute);
                        break;
                    }
                    
                    _ = DelayedUpdateRoutine();
                    
                    Notification.Show($"Muted all players in the room");
                    Audio.PlaySound("success.wav", 0.74f);
                }
                catch (Exception e)
                {
                    Logger.Error($"Failed to run command 'mute all': {e}");
                }
            },
            keywords: ["mute everyone", "mute others"]
        );
    }
}

public sealed class UnmuteAll : IEvenCommand
{
    public static IEnumerator DelayedUpdateRoutine()
    {
        yield return new WaitForSeconds(0.5f);
        GorillaScoreboardTotalUpdater.instance.UpdateActiveScoreboards();
    }

    public Command Create()
    {
        return new Command(
            name: "unmute all",
            category: "Utility",
            description: "Unmute every player in the room",
            action: () =>
            {
                try
                {
                    if (!NetworkSystem.Instance.InRoom) return;
                    
                    foreach (var line in GorillaScoreboardTotalUpdater.allScoreboardLines)
                    {
                        line.PressButton(false, GorillaPlayerLineButton.ButtonType.Mute);
                        break;
                    }
                    
                    _ = DelayedUpdateRoutine();
                    
                    Notification.Show($"Unmuted all players in the room");
                    Audio.PlaySound("success.wav", 0.74f);
                }
                catch (Exception e)
                {
                    Logger.Error($"Failed to run command 'unmute all': {e}");
                }
            },
            keywords: ["unmute everyone", "unmute others"]
        );
    }
}