namespace Even.Models;

public static class Config
{
    public const string BaseUrl = "https://even.rest/api";
    public static string DataUrl => $"{BaseUrl}/server-data";
}