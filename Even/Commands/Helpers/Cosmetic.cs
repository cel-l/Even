using System;
using System.Collections.Generic;
using GorillaNetworking;
using static GorillaNetworking.CosmeticsController;
using Logger = Even.Utils.Logger;
// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace Even.Commands.Helpers;

public static class Cosmetic
{
    private static readonly Random Random = new();
    public static List<string> cosmeticList { get; private set; }
    
    public static string FromDisplayName(string cosmeticDisplayName)
    {
        if (string.IsNullOrWhiteSpace(cosmeticDisplayName))
            return null;

        try
        {
            return instance.GetItemNameFromDisplayName(cosmeticDisplayName);
        }
        catch (Exception ex)
        {
            Logger.Error($"Error getting cosmetic from display name '{cosmeticDisplayName}': {ex.Message}");
            return null;
        }
    }
    
    public static void EquipFromDisplayName(string cosmeticDisplayName)
    {
        if (string.IsNullOrWhiteSpace(cosmeticDisplayName))
            return;

        var cosmeticCodeName = instance.GetItemNameFromDisplayName(cosmeticDisplayName);
        instance.PressWardrobeItemButton(
            instance.GetItemFromDict(cosmeticCodeName),
            false,
            false
        );
    }

    public static List<string> DisplayNames()
    {
        var displayNames = new List<string>();

        try
        {
            var concat = instance.concatStringCosmeticsAllowed;
            if (string.IsNullOrEmpty(concat))
                return displayNames;

            var ids = concat.Split('.');
            foreach (var id in ids)
            {
                if (!IsValidOwnedCosmeticToken(id))
                    continue;

                var cosmeticId = EnsureTrailingDot(id);
                var displayName = GetDisplayNameFromCosmetic(cosmeticId);

                if (!string.Equals(displayName, "NOTHING", StringComparison.Ordinal))
                    displayNames.Add(displayName);
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"Error getting cosmetic display names: {ex.Message}");
        }

        return displayNames;
    }

    public static void PopulateCosmeticList()
    {
        var concat = instance.concatStringCosmeticsAllowed ?? string.Empty;
        cosmeticList = new List<string>(concat.Split('.'));
    }

    public static void PickAndWearRandomCosmeticFromType(CosmeticCategory itemType)
    {
        try
        {
            var concat = instance.concatStringCosmeticsAllowed;
            if (string.IsNullOrEmpty(concat))
            {
                Logger.Warning($"No owned cosmetics found, cannot randomize {itemType}");
                return;
            }

            var available = new List<string>();

            foreach (var token in concat.Split('.'))
            {
                if (!IsValidOwnedCosmeticToken(token))
                    continue;
                
                var cosmeticId = EnsureTrailingDot(token);

                CosmeticItem item;
                try
                {
                    item = instance.GetItemFromDict(cosmeticId);
                }
                catch
                {
                    continue;
                }

                if (item.itemCategory == itemType)
                    available.Add(cosmeticId);
            }

            if (available.Count == 0)
            {
                Logger.Warning($"No cosmetics owned for category {itemType}");
                return;
            }

            var chosenCosmeticId = available[Random.Next(available.Count)];
            EquipFromDisplayName(GetDisplayNameFromCosmetic(chosenCosmeticId));
        }
        catch (Exception ex)
        {
            Logger.Info($"Error randomizing cosmetic for {itemType}: {ex.Message}");
        }
    }

    public static string GetDisplayNameFromCosmetic(string cosmeticId)
    {
        if (string.IsNullOrWhiteSpace(cosmeticId))
            return "NOTHING";

        var cosmetic = CosmeticsController.instance.GetItemFromDict(cosmeticId);
        return CosmeticsController.instance.GetItemDisplayName(cosmetic);
    }

    public static void RemoveCosmeticsFromSet()
    {
        var cosmeticsController = CosmeticsController.instance;

        for (var i = 0; i < cosmeticsController.currentWornSet.items.Length; i++)
        {
            cosmeticsController.currentWornSet.items[i] = cosmeticsController.nullItem;
        }

        cosmeticsController.SaveCurrentItemPreferences();
        cosmeticsController.UpdateShoppingCart();
        cosmeticsController.UpdateWornCosmetics(true);
        cosmeticsController.OnCosmeticsUpdated?.Invoke();
    }

    private static bool IsValidOwnedCosmeticToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return false;
        
        if (string.Equals(token, "ITEMS", StringComparison.Ordinal) ||
            string.Equals(token, "ITEMS.", StringComparison.Ordinal) ||
            string.Equals(token, "Slingshot", StringComparison.Ordinal) ||
            string.Equals(token, "Slingshot.", StringComparison.Ordinal))
        {
            return false;
        }

        return true;
    }

    private static string EnsureTrailingDot(string token)
    {
        return token.EndsWith(".", StringComparison.Ordinal) ? token : $"{token}.";
    }
}