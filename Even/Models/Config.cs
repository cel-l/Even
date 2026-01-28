namespace Even.Models;

public static class Config
{
    public const string BaseUrl = "https://api.aeris.now";
    public static string DataUrl => $"{BaseUrl}/data";
    public static string SoundsUrl => $"{BaseUrl}/sounds";
}