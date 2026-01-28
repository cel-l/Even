using System;
using System.Linq;
using HarmonyLib;
using Photon.Pun;
using UnityEngine;
using Logger = Even.Utils.Logger;
namespace Even.Commands.Helpers;

public static class Rig
{
    public static bool IsTagged(VRRig rig)
    {
        var materialName = rig?.mainSkin?.material?.name;
        if (string.IsNullOrEmpty(materialName))
            return false;

        var name = materialName.ToLowerInvariant();
        return name.Contains("fected") || name.Contains("it") || name.Contains("stealth");
    }

    public static VRRig FromPlayer(NetPlayer player)
    {
        if (player == null)
            return null;

        try
        {
            return GorillaGameManager.instance.FindPlayerVRRig(player);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Even] Error getting VRRig from player: {ex.Message}");
            return null;
        }
    }

    public static NetPlayer ToPlayer(VRRig rig) => rig?.Creator;

    public static NetPlayer FindPlayerByUserId(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return null;

        try
        {
            foreach (NetPlayer p in NetworkSystem.Instance.PlayerListOthers)
            {
                if (p != null && string.Equals(p.UserId, userId, StringComparison.Ordinal))
                    return p;
            }

            return null;
        }
        catch (Exception ex)
        {
            Logger.Info($"Error getting player from userId '{userId}': {ex.Message}");
            return null;
        }
    }

    public static NetPlayer FindPlayerByName(string playerName)
    {
        if (string.IsNullOrWhiteSpace(playerName))
            return null;

        var target = playerName.ToLowerInvariant();

        try
        {
            return NetworkSystem.Instance.PlayerListOthers.Where(p => p?.NickName != null).FirstOrDefault(p => p.NickName.ToLowerInvariant() == target);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Even] Error getting player by name '{playerName}': {ex.Message}");
            return null;
        }
    }

    public static VRRig RandomRig(bool includeSelf)
    {
        try
        {
            var list = includeSelf ? PhotonNetwork.PlayerList : PhotonNetwork.PlayerListOthers;
            if (list == null || list.Length == 0)
                return null;

            var chosen = list[UnityEngine.Random.Range(0, list.Length)];
            return FromPlayer(chosen);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Even] Error getting random VRRig: {ex.Message}");
            return null;
        }
    }

    public static VRRig ClosestToSelf()
    {
        VRRig closest = null;
        var minDistance = float.MaxValue;
        
        try
        {
            var self = GorillaTagger.Instance?.offlineVRRig;
            Transform selfTransform = GorillaTagger.Instance?.bodyCollider?.transform;

            if (self == null || selfTransform == null || GorillaParent.instance == null)
                return null;

            foreach (var rig in GorillaParent.instance.vrrigs)
            {
                if (rig == null || rig == self)
                    continue;

                var distance = Vector3.Distance(selfTransform.position, rig.transform.position);
                if (!(distance < minDistance)) continue;
                
                minDistance = distance;
                closest = rig;
            }

            return closest;
        }
        catch (Exception ex)
        {
            Logger.Error($"Error getting closest VRRig: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// I will NOT be using an assembly publicizer
    /// </summary>
    public static string GetRawCosmeticString(VRRig rig)
    {
        if (rig == null)
            return null;

        try
        {
            return Traverse.Create(rig).Field("rawCosmeticString").GetValue<string>();
        }
        catch
        {
            return null;
        }
    }
}