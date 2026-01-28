using System.Collections;
using Even.Commands.Helpers;
using Even.Commands.Runtime;
using Even.Utils;
using UnityEngine;
namespace Even.Commands.Default.Player;

public sealed class Mute : IEvenCommand
{
    private static IEnumerator DelayedUpdateRoutine()
    {
        yield return new WaitForSeconds(0.5f);
        GorillaScoreboardTotalUpdater.instance.UpdateActiveScoreboards();
    }
    
    public Command Create()
    {
        PlayerCommandRegistry.RegisterPlayerActionTemplate(
            actionVerb: "mute",
            category: "Player",
            descriptionFactory: p => $"Toggles local mute for {p?.NickName ?? "player"}",
            action: ToggleMute
        );

        return new Command(
            name: "mute player",
            category: "Player",
            description: "Enables per-player mute commands like: 'mute (player name)'",
            action: () => Notification.Show("Use the mute command by saying mute (player)"),
            keywords: []
        );
    }

    private static void ToggleMute(Photon.Realtime.Player player)
    {
        if (player == null)
            return;

        foreach (var line in GorillaScoreboardTotalUpdater.allScoreboardLines)
        {
            if (line.linePlayer != (NetPlayer)player) continue;
            
            line.PressButton(true, GorillaPlayerLineButton.ButtonType.Mute);
            break;
        }
        
        Notification.Show($"Muted {Rig.ColoredName(player)}");
        _ = DelayedUpdateRoutine();
    }
}