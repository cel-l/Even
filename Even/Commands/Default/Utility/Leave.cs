using System;
using Even.Utils;
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
                    if (!NetworkSystem.Instance.InRoom) return;
                    
                    await NetworkSystem.Instance.ReturnToSinglePlayer();
                    
                    Notification.Show("Disconnected from room successfully", 0.6f, false, true);
                    Audio.PlaySound("success.wav", 1.3f);
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