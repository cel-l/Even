using System;
using Even.Commands.Helpers;
using Even.Utils;
using GorillaNetworking;
using UnityEngine;
using Logger = Even.Utils.Logger;
namespace Even.Commands.Default.Utility;

internal static class QueueMode
{
    internal static void Set(string queue)
    {
        Audio.PlaySound("success", 0.74f);
        Notification.Show($"Changed queue to {char.ToUpper(queue[0]) + queue[1..].ToLower()}");

        GorillaComputer.instance.currentQueue = queue;
        PlayerPrefs.Save();
    }
}

internal static class WeatherType
{
    internal static void Set(BetterDayNightManager.WeatherType weatherType)
    {
        BetterDayNightManager.instance.SetFixedWeather(weatherType);
        Audio.PlaySound("success", 0.74f);
    }
}

internal static class TimeOfDay
{
    internal static void Set(int timeIndex)
    {
        BetterDayNightManager.instance.SetTimeOfDay(timeIndex);
        Audio.PlaySound("success", 0.74f);
    }
}

internal static class MicMode
{
    internal static void Set(string mode)
    {
        Audio.PlaySound("success.wav", 0.74f);
        Notification.Show($"Changed mic mode to {char.ToUpper(mode[0]) + mode[1..].ToLower()}");

        GorillaComputer.instance.pttType = mode;
        PlayerPrefs.SetString("pttType", GorillaComputer.instance.pttType);
        PlayerPrefs.Save();
    }
}

public sealed class ChangeQueueToDefault : IEvenCommand
{
    public Command Create() => new Command(
        name: "change queue to default",
        category: "Utility",
        description: "Sets the matchmaking queue to Default",
        action: () => {
            try { QueueMode.Set("DEFAULT"); }
            catch (Exception e) { Logger.Error($"Failed to run command 'change queue to default': {e}"); }
        },
        keywords: ["default queue"]
    );
}

public sealed class ChangeQueueToMinigames : IEvenCommand
{
    public Command Create() => new Command(
        name: "change queue to minigames",
        category: "Utility",
        description: "Sets the matchmaking queue to Minigames",
        action: () => {
            try { QueueMode.Set("MINIGAMES"); }
            catch (Exception e) { Logger.Error($"Failed to run command 'change queue to minigames': {e}"); }
        },
        keywords: ["minigames queue"]
    );
}

public sealed class ChangeQueueToCompetitive : IEvenCommand
{
    public Command Create() => new Command(
        name: "change queue to competitive",
        category: "Utility",
        description: "Sets the matchmaking queue to Competitive",
        action: () => {
            try { QueueMode.Set("COMPETITIVE"); }
            catch (Exception e) { Logger.Error($"Failed to run command 'change queue to competitive': {e}"); }
        },
        keywords: ["competitive queue"]
    );
}

public sealed class ChangeWeatherToRain : IEvenCommand
{
    public Command Create() => new Command(
        name: "change weather to rain",
        category: "Utility",
        description: "Sets the weather to rain",
        action: () => {
            try { WeatherType.Set(BetterDayNightManager.WeatherType.Raining); }
            catch (Exception e) { Logger.Error($"Failed to run command 'change weather to rain': {e}"); }
        },
        keywords: ["change weather to rain", "set weather to rain", "make it rain"]
    );
}

public sealed class ClearWeather : IEvenCommand
{
    public Command Create() => new Command(
        name: "clear weather",
        category: "Utility",
        description: "Sets the weather to clear",
        action: () => {
            try { WeatherType.Set(BetterDayNightManager.WeatherType.None); }
            catch (Exception e) { Logger.Error($"Failed to run command 'clear weather': {e}"); }
        },
        keywords: ["clear weather", "change weather to clear", "set weather clear", "make it clear"]
    );
}

public sealed class SetTimeToDawn : IEvenCommand
{
    public Command Create() => new Command(
        name: "set time to dawn",
        category: "Utility",
        description: "Sets the time of day to dawn",
        action: () => {
            try { TimeOfDay.Set(1); }
            catch (Exception e) { Logger.Error($"Failed to run command 'set time to dawn': {e}"); }
        },
        keywords: ["set dawn", "time dawn", "make it dawn"]
    );
}

public sealed class SetTimeToDay : IEvenCommand
{
    public Command Create() => new Command(
        name: "set time to day",
        category: "Utility",
        description: "Sets the time of day to day",
        action: () => {
            try { TimeOfDay.Set(2); }
            catch (Exception e) { Logger.Error($"Failed to run command 'set time to day': {e}"); }
        },
        keywords: ["set day", "time day", "make it day", "daytime"]
    );
}

public sealed class SetTimeToNoon : IEvenCommand
{
    public Command Create() => new Command(
        name: "set time to noon",
        category: "Utility",
        description: "Sets the time of day to noon",
        action: () => {
            try { TimeOfDay.Set(3); }
            catch (Exception e) { Logger.Error($"Failed to run command 'set time to noon': {e}"); }
        },
        keywords: ["set noon", "time noon", "make it noon", "midday"]
    );
}

public sealed class SetTimeToAfternoon : IEvenCommand
{
    public Command Create() => new Command(
        name: "set time to afternoon",
        category: "Utility",
        description: "Sets the time of day to afternoon",
        action: () => {
            try { TimeOfDay.Set(4); }
            catch (Exception e) { Logger.Error($"Failed to run command 'set time to afternoon': {e}"); }
        },
        keywords: ["set afternoon", "time afternoon", "make it afternoon"]
    );
}

public sealed class SetTimeToEvening : IEvenCommand
{
    public Command Create() => new Command(
        name: "set time to evening",
        category: "Utility",
        description: "Sets the time of day to evening",
        action: () => {
            try { TimeOfDay.Set(5); }
            catch (Exception e) { Logger.Error($"Failed to run command 'set time to evening': {e}"); }
        },
        keywords: ["set evening", "time evening", "make it evening"]
    );
}

public sealed class SetTimeToNight : IEvenCommand
{
    public Command Create() => new Command(
        name: "set time to night",
        category: "Utility",
        description: "Sets the time of day to night",
        action: () => {
            try { TimeOfDay.Set(6); }
            catch (Exception e) { Logger.Error($"Failed to run command 'set time to night': {e}"); }
        },
        keywords: ["set night", "time night", "make it night", "nighttime"]
    );
}

public sealed class SetTimeToMidnight : IEvenCommand
{
    public Command Create() => new Command(
        name: "set time to midnight",
        category: "Utility",
        description: "Sets the time of day to midnight",
        action: () => {
            try { TimeOfDay.Set(7); }
            catch (Exception e) { Logger.Error($"Failed to run command 'set time to midnight': {e}"); }
        },
        keywords: ["set midnight", "time midnight", "make it midnight"]
    );
}

public sealed class ChangeMicOpenMic : IEvenCommand
{
    public Command Create() => new Command(
        name: "change mic to open mic",
        category: "Utility",
        description: "Sets the voice mode to Open Mic",
        action: () => {
            try { MicMode.Set("OPEN MIC"); }
            catch (Exception e) { Logger.Error($"Failed to run command 'change mic to open mic': {e}"); }
        },
        keywords: ["open mic"]
    );
}

public sealed class ChangeMicPushToTalk : IEvenCommand
{
    public Command Create() => new Command(
        name: "change mic to push to talk",
        category: "Utility",
        description: "Sets the voice mode to Push To Talk",
        action: () => {
            try { MicMode.Set("PUSH TO TALK"); }
            catch (Exception e) { Logger.Error($"Failed to run command 'change mic to push to talk': {e}"); }
        },
        keywords: ["push to talk", "ptt"]
    );
}

public sealed class ChangeMicPushToMute : IEvenCommand
{
    public Command Create() => new Command(
        name: "change mic to push to mute",
        category: "Utility",
        description: "Sets the voice mode to Push To Mute",
        action: () => {
            try { MicMode.Set("PUSH TO MUTE"); }
            catch (Exception e) { Logger.Error($"Failed to run command 'change mic to push to mute': {e}"); }
        },
        keywords: ["push to mute", "ptm"]
    );
}

public sealed class Notifications : IEvenCommand
{
    public Command Create()
    {
        return new Command(
            name: "toggle notifications",
            category: "Utility",
            description: "Toggles Even notifications",
            action: void () =>
            {
                try
                {
                    var enabled = Settings.Toggle(Settings.Keys.Notifications, saveImmediately: true);
                    
                    Notification.Show($"Notifications are now {StatusText.FromBool(enabled)}");
                }
                catch (Exception e)
                {
                    Logger.Error($"Failed to run command 'toggle notifications': {e}");
                }
            },
            keywords: ["notifications", "alerts"]
        );
    }
}