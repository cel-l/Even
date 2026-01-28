using System.Threading.Tasks;
using Even.Commands.Helpers;
using Even.Commands.Runtime;
using Even.Utils;
using GorillaNetworking;
namespace Even.Commands.Default.Player;

public sealed class Copy : IEvenCommand
{
    public static async Task CopyOutfitFromVRRig(VRRig rig)
    {
        if (rig == null || rig.cosmeticSet == null || GorillaTagger.Instance.offlineVRRig == null)
            return;

        Cosmetic.RemoveCosmeticsFromSet();

        await Task.Delay(300);

        string colorCode = Rig.GetColorCode(rig);
        if (!string.IsNullOrEmpty(colorCode))
        {
            string[] colorParts = colorCode.Split(' ');
            if (colorParts.Length == 3 &&
                int.TryParse(colorParts[0], out int red) &&
                int.TryParse(colorParts[1], out int green) &&
                int.TryParse(colorParts[2], out int blue))
            {
                Rig.SetColorCode(red, green, blue);
            }
        }

        var wornCosmetics = rig.cosmeticSet.ToDisplayNameArray();
        var concat = CosmeticsController.instance.concatStringCosmeticsAllowed;

        foreach (var displayName in wornCosmetics)
        {
            if (string.IsNullOrEmpty(displayName) || displayName == "null" || displayName == "NOTHING")
                continue;

            var cosmeticId = Cosmetic.FromDisplayName(displayName);

            if (!string.IsNullOrEmpty(cosmeticId) && concat.Contains(cosmeticId))
            {
                Cosmetic.EquipFromDisplayName(displayName);
            }
        }
    }
    
    public Command Create()
    {
        PlayerCommandRegistry.RegisterPlayerActionTemplate(
            actionVerb: "copy",
            category: "Player",
            descriptionFactory: p => $"Copies the color and outfit of {p?.NickName ?? "player"}",
            action: CopyPlayer
        );

        return new Command(
            name: "copy player",
            category: "Player",
            description: "Copy player commands like: 'copy (player name) to copy their color and outfit'",
            action: () => Notification.Show("Use the copy command by saying copy (player)"),
            keywords: []
        );
    }

    private static void CopyPlayer(Photon.Realtime.Player player)
    {
        if (player == null)
            return;

        foreach (var line in GorillaScoreboardTotalUpdater.allScoreboardLines)
        {
            if (line.linePlayer != (NetPlayer)player) continue;
            
            line.PressButton(true, GorillaPlayerLineButton.ButtonType.Mute);
            break;
        }
        
        Notification.Show($"Copied {Rig.ColoredName(player)}");
    }
}