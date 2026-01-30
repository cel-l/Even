using System;
using Even.Utils;
using UnityEngine;
using Logger = Even.Utils.Logger;
namespace Even.Commands.Default.Utility;

public sealed class Quit : IEvenCommand
{
    public Command Create()
    {
        return new Command(
            name: "quit game",
            category: "Utility",
            description: "Leave the current room",
            action: void () =>
            {
                try
                {
                    Audio.PlaySound("success.wav", 0.74f);
                    Application.Quit();
                }
                catch (Exception e)
                {
                    Logger.Error($"Failed to run command 'quit game': {e}");
                }
            },
            keywords: ["close game", "exit game"]
        );
    }
}