using System;
using System.Linq;
using GorillaNetworking;
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
    
    public static string ColoredName(NetPlayer player)
    {
        var playerRig = FromPlayer(player);
        if (playerRig == null) return player.NickName;
        
        var playerColor = playerRig.playerColor;
        var hexColor = ColorUtility.ToHtmlStringRGB(playerColor);
        return $"<color=#{hexColor}>{player.SanitizedNickName}</color>";
    }
    
    public static string ConvertColor(float color)
    {
        return ((int)(color * 9f)).ToString();
    }
    
    public static string GetColorCode(VRRig rig)
    {
        if (rig == null) return null;
        
        var playerColor = rig.playerColor;
        var str = "";
        str = ConvertColor(playerColor.r) + " " + ConvertColor(playerColor.g) + " " + ConvertColor(playerColor.b);
        return str;
    }
    
    public static void SetColorCode(int red, int green, int blue)
    {
        var redValue = Mathf.Clamp01(red / 9f);
        var greenValue = Mathf.Clamp01(green / 9f);
        var blueValue = Mathf.Clamp01(blue / 9f);

        PlayerPrefs.SetFloat("redValue", redValue);
        PlayerPrefs.SetFloat("greenValue", greenValue);
        PlayerPrefs.SetFloat("blueValue", blueValue);

        GorillaTagger.Instance.UpdateColor(redValue, greenValue, blueValue);
        GorillaComputer.instance.UpdateColor(redValue, greenValue, blueValue);

        PlayerPrefs.Save();

        if (NetworkSystem.Instance.InRoom)
        {
            GorillaTagger.Instance.myVRRig.SendRPC("RPC_InitializeNoobMaterial", RpcTarget.All, redValue, greenValue, blueValue);
        }
    }
    
    public static VRRig Random(bool includeSelf)
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

    public static VRRig Closest()
    {
        VRRig closest = null;
        var minDistance = float.MaxValue;
        
        try
        {
            var self = GorillaTagger.Instance?.offlineVRRig;
            var selfTransform = GorillaTagger.Instance?.bodyCollider?.transform;

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
}