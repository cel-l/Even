using System;
using System.Collections.Generic;
using System.Linq;
using Even.Commands.Default.Utility;
using Even.Commands.Helpers;
using Even.Utils;
using GorillaNetworking;
using UnityEngine;
using Logger = Even.Utils.Logger;
namespace Even.Commands.Default.Fun;

public sealed class OwnedCosmetics : IEvenCommand
{
    private static bool _initialized;
    private static readonly HashSet<string> RegisteredCosmetics =
        new(StringComparer.OrdinalIgnoreCase);

    private static readonly Dictionary<string, string[]> CosmeticList = new()
    {
        { "LBAAD.", ["Admin Badge", "4A1B82", "FF69B4"] },
        { "LBAAK.", ["Dev Stick", "FF0000", "6E2323"] },
        { "LMAPY.", ["Forest Guide Stick", "206DB0", "284ABD"] },
        { "LBAGS.", ["Illustrator Badge", "E100FF", "BF7F0F"] },
        { "LBADE.", ["Finger Painter Badge", "FF0000", "00FF00", "0000FF"] },
        { "LBANI.", ["Another Axiom Creator Badge", "4A1B82", "1C8A53"] }
    };

    public Command Create()
    {
        RegisterOwnedCosmeticCommands();

        return new Command(
            name: "owned cosmetics",
            category: "Utility",
            description: "Registers toggle commands for all owned cosmetics",
            action: () => { },
            keywords: ["owned cosmetics"]
        );
    }

    private static void RegisterOwnedCosmeticCommands()
    {
        if (_initialized)
            return;

        _initialized = true;

        try
        {
            var concat = CosmeticsController.instance.concatStringCosmeticsAllowed;
            if (string.IsNullOrWhiteSpace(concat))
                return;

            foreach (var token in concat.Split('.'))
            {
                if (string.IsNullOrWhiteSpace(token))
                    continue;

                if (token == "ITEMS" || token == "Slingshot")
                    continue;

                var cosmeticId = token.EndsWith(".") ? token : $"{token}.";

                string displayName;
                try
                {
                    displayName = Cosmetic.GetDisplayNameFromCosmetic(cosmeticId);
                }
                catch
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(displayName) ||
                    displayName.Equals("NOTHING", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!RegisteredCosmetics.Add(displayName))
                    continue;

                CommandAPI.Register(CreateToggleCommand(displayName, cosmeticId));
            }
        }
        catch (Exception e)
        {
            Logger.Error($"Owned cosmetic command registration failed: {e}");
        }
    }

    private static Command CreateToggleCommand(string displayName, string cosmeticId)
    {
        var lower = displayName.ToLowerInvariant();

        return new Command(
            name: $"toggle {lower}",
            category: "Cosmetic",
            description: $"Toggle the {displayName} cosmetic on or off",
            action: () =>
            {
                Audio.PlayPopSound();

                Cosmetic.EquipCosmeticFromCosmeticDisplayName(displayName);

                var cosmeticSet = GorillaTagger.Instance.offlineVRRig.cosmeticSet;
                var isWearing = cosmeticSet.items.Any(c =>
                    !c.isNullItem && c.itemName == cosmeticId
                );

                var nameForMessage = lower;

                if (isWearing && CosmeticList.TryGetValue(cosmeticId, out var data))
                {
                    var badgeName = data[0];
                    var colors = data.Skip(1).ToArray();
                    nameForMessage = ApplyGradient(badgeName, colors);
                }

                var message = isWearing
                    ? $"You are now wearing the {nameForMessage}"
                    : $"You are no longer wearing the {lower}";

                Notification.Show(message, 0.36f, false, true);
            },
            keywords: [
                $"toggle {lower}",
                $"equip {lower}",
                $"put on {lower}",
                $"take off {lower}"
            ]
        );
    }

    private static string ApplyGradient(string text, params string[] hexColors)
    {
        if (string.IsNullOrEmpty(text) || hexColors.Length == 0)
            return text;

        var result = "";
        var len = text.Length;

        for (var i = 0; i < len; i++)
        {
            var t = (float)i / (len - 1);
            Color color = Color.white;

            if (hexColors.Length == 2)
            {
                ColorUtility.TryParseHtmlString("#" + hexColors[0], out var start);
                ColorUtility.TryParseHtmlString("#" + hexColors[1], out var end);
                color = Color.Lerp(start, end, t);
            }
            else if (hexColors.Length == 3)
            {
                ColorUtility.TryParseHtmlString("#" + hexColors[0], out var c1);
                ColorUtility.TryParseHtmlString("#" + hexColors[1], out var c2);
                ColorUtility.TryParseHtmlString("#" + hexColors[2], out var c3);

                var section = t * 2;
                color = section <= 1
                    ? Color.Lerp(c1, c2, section)
                    : Color.Lerp(c2, c3, section - 1);
            }

            result += $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{text[i]}</color>";
        }

        return result;
    }
}

public sealed class TakeOffCosmetics : IEvenCommand
{
    public Command Create()
    {
        return new Command(
            name: "take off cosmetics",
            category: "Fun",
            description: "Takes off all cosmetics worn by the rig",
            action: () =>
            {
                Audio.PlayPopSound();
                MicMode.Set("OPEN MIC");
            },
            keywords: ["take off cosmetics", "get naked"]
        );
    }
}

public sealed class RandomizeCosmetics : IEvenCommand
{
    public Command Create()
    {
        return new Command(
            name: "randomize cosmetics",
            category: "Fun",
            description: "Randomizes the cosmetics worn by the rig",
            action: () =>
            {
                var categories = new[]
                {
                    CosmeticsController.CosmeticCategory.Hat,
                    CosmeticsController.CosmeticCategory.Face,
                    CosmeticsController.CosmeticCategory.Badge,
                    CosmeticsController.CosmeticCategory.Shirt,
                    CosmeticsController.CosmeticCategory.Pants,
                    CosmeticsController.CosmeticCategory.Paw,
                    CosmeticsController.CosmeticCategory.Chest,
                    CosmeticsController.CosmeticCategory.Fur
                };

                foreach (var category in categories)
                    Cosmetic.PickAndWearRandomCosmeticFromType(category);

                Audio.PlayPopSound();
            },
            keywords: ["randomize outfit", "random fit", "new fit"]
        );
    }
}

internal static class LoadOutfitCommandFactory
{
    internal static Command CreateForSlot(int slot)
    {
        return new Command(
            name: $"load outfit {slot}",
            category: "Fun",
            description: $"Loads the saved outfit slot {slot}",
            action: () =>
            {
                if (slot is < 1 or > 5)
                    return;

                var internalIndex = slot - 1;
                Audio.PlayPopSound();
                
                CosmeticsController.instance.LoadSavedOutfit(internalIndex);
                Notification.Show($"You are now wearing outfit #{slot}", 0.36f, false, true);
            },
            keywords: [$"load preset {slot}", $"preset {slot}", $"outfit {slot}"]
        );
    }
}

public sealed class LoadOutfit1 : IEvenCommand { public Command Create() => LoadOutfitCommandFactory.CreateForSlot(1); }
public sealed class LoadOutfit2 : IEvenCommand { public Command Create() => LoadOutfitCommandFactory.CreateForSlot(2); }
public sealed class LoadOutfit3 : IEvenCommand { public Command Create() => LoadOutfitCommandFactory.CreateForSlot(3); }
public sealed class LoadOutfit4 : IEvenCommand { public Command Create() => LoadOutfitCommandFactory.CreateForSlot(4); }
public sealed class LoadOutfit5 : IEvenCommand { public Command Create() => LoadOutfitCommandFactory.CreateForSlot(5); }