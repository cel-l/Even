using System;
using Even.Commands.Helpers;
using Even.Utils;
using Logger = Even.Utils.Logger;

namespace Even.Commands.Default.Utility;

public sealed class QuickCommands : IEvenCommand
{
    public Command Create()
    {
        return new Command(
            name: "toggle quick commands",
            category: "Utility",
            description: "Toggles 'even {command}' quick commands on/off",
            action: void () =>
            {
                try
                {
                    var enabled = Settings.Toggle(Settings.Keys.QuickCommands, defaultValue: true, saveImmediately: true);
                    Audio.PlayPopSound();
                    Notification.Show($"Quick commands are now {StatusText.FromBool(enabled)}");
                }
                catch (Exception e)
                {
                    Logger.Error($"Failed to run command 'toggle quick commands': {e}");
                }
            },
            keywords: ["toggle quick commands", "quick commands", "quick command", "toggle quick command"]
        );
    }
}