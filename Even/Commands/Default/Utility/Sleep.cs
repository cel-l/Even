using System;
using Even.Utils;
using Logger = Even.Utils.Logger;

namespace Even.Commands.Default.Utility;

public sealed class Sleep : IEvenCommand
{
    public Command Create()
    {
        return new Command(
            name: "sleep",
            category: "Utility",
            description: "Makes Even stop listening",
            action: async void () =>
            {
                try
                {
                    Notification.Show("Going to sleep...", 0.6f, false, true);
                    Audio.PlaySound("success.wav", 1.3f);

                    if (!Plugin.AssistantInstance)
                    {
                        return;
                    }

                    Plugin.AssistantInstance.GoToSleep();
                }
                catch (Exception e)
                {
                    Logger.Error($"Failed to run command 'sleep': {e}");
                }
            },
            keywords: ["go to sleep", "stop listening"]
        );
    }
}