using System;
using System.Threading.Tasks;
using Even.Models;
using MonkeNotificationLib;
using Newtonsoft.Json;
using Photon.Pun;
using UnityEngine.Networking;
using Hashtable = ExitGames.Client.Photon.Hashtable;
namespace Even.Utils;

public sealed class Network
{
    public static Network Instance { get; } = new();
    private Network() { }

    public async Task<int> FetchServerDataAsync(string version, string modName)
    {
        if (string.IsNullOrWhiteSpace(version) || string.IsNullOrWhiteSpace(modName))
        {
            Logger.Error("Version or modName is empty");
            return 0;
        }

        try
        {
            var url = $"{Config.DataUrl}?version={UnityWebRequest.EscapeURL(version)}&mod={UnityWebRequest.EscapeURL(modName)}&user={NetworkSystem.Instance.LocalPlayer.UserId}";
            var json = await GetTextAsync(url);
            if (string.IsNullOrWhiteSpace(json))
            {
                Logger.Error("Empty server response");
                return 0;
            }

            var data = JsonConvert.DeserializeObject<ServerResponse>(json);
            if (data == null)
            {
                Logger.Error("Failed to deserialize server response");
                return 0;
            }

            if (data.VersionCheck != null)
            {
                var check = data.VersionCheck;
                if (check.Outdated)
                    NotificationController.AppendMessage(Plugin.Alias, $"Mod is outdated! Latest: {check.LatestVersion}", false, 20f);

                Logger.Info(check.Outdated ? $"Version outdated: {check.Message}" : $"Version check: {check.Message}");
            }

            if (string.IsNullOrWhiteSpace(data.CustomProperty) || PhotonNetwork.LocalPlayer == null) return 0;
            PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable
            {
                { data.CustomProperty.Trim(), true }
            });

            return 0;
        }
        catch (Exception ex)
        {
            Logger.Error($"FetchServerDataAsync failed: {ex}");
            return 0;
        }
    }

    private static async Task<string> GetTextAsync(string url)
    {
        using var req = UnityWebRequest.Get(url);
        req.downloadHandler = new DownloadHandlerBuffer();
        await req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
            throw new Exception($"Request failed ({req.responseCode}): {req.error}");

        return req.downloadHandler.text;
    }

    public class ServerResponse
    {
        [JsonProperty("custom_property")] public string CustomProperty { get; private set; } = "";
        [JsonProperty("version_check")] public VersionCheckInfo VersionCheck { get; private set; }
    }

    public class VersionCheckInfo
    {
        [JsonProperty("outdated")] public bool Outdated { get; private set; }
        [JsonProperty("latest_version")] public string LatestVersion { get; private set; } = "";
        [JsonProperty("current_version")] public string CurrentVersion { get; private set; } = "";
        [JsonProperty("message")] public string Message { get; private set; } = "";
    }
}