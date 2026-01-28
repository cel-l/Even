using System;
using Logger = Even.Utils.Logger;
namespace Even.Commands.Default.Utility;

public sealed class Leave : IEvenCommand
{
    public Command Create()
    {
        return new Command(
            name: "leave room",
            category: "Utility",
            description: "Leave the current room",
            action: async void () =>
            {
                try
                {
                    await NetworkSystem.Instance.ReturnToSinglePlayer();
                }
                catch (Exception e)
                {
                    Logger.Error($"Failed to run command 'leave': {e}");
                }
            },
            keywords: ["leave", "leave room", "disconnect"]
        );
    }
}