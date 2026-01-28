using System;
using Even.Utils;
using Logger = Even.Utils.Logger;
namespace Even.Commands.Default.Miscellaneous;

public sealed class TimeCommand : IEvenCommand
{
    public Command Create()
    {
        return new Command(
            name: "what time is it",
            category: "Utility",
            description: "Tells the current time",
            action: () =>
            {
                try
                {
                    var timeMessage = $"It's{DateTime.Now: h:mm tt}";
                    Notification.Show(timeMessage);
                }
                catch (Exception e)
                {
                    Logger.Error($"Failed to run command 'what time is it': {e}");
                }
            },
            keywords: ["time", "what's the time"]
        );
    }
}

public sealed class DateCommand : IEvenCommand
{
    public Command Create()
    {
        return new Command(
            name: "what's the date",
            category: "Utility",
            description: "Tells today's date",
            action: () =>
            {
                try
                {
                    var dateMessage = $"Today's date is {DateTime.Now:dddd}, {DateTime.Now:MMMM dd, yyyy}";
                    Notification.Show(dateMessage);
                }
                catch (Exception e)
                {
                    Logger.Error($"Failed to run command 'what's the date': {e}");
                }
            },
            keywords: ["what's today"]
        );
    }
}