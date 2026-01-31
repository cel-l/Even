using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Even.Utils;
using UnityEngine;
using Logger = Even.Utils.Logger;

namespace Even.Commands.Default.Miscellaneous;

public sealed class ExportCommands : IEvenCommand
{
    public Command Create()
    {
        return new Command(
            name: "export commands",
            category: "Utility",
            description: "Exports all registered commands to a JSON file",
            action: void () =>
            {
                try
                {
                    var allCommands = Command.CreateAll();

                    var jsonData = allCommands.Select(c => new
                    {
                        Name = c.Name,
                        Category = c.Category,
                        Description = c.Description,
                        Keywords = c.Keywords
                    }).ToList<object>();

                    var json = JsonUtility.ToJson(new Wrapper { Commands = jsonData }, true);

                    var filePath = Path.Combine(Application.persistentDataPath, "commands_export.json");
                    File.WriteAllText(filePath, json);
                    
                    Audio.PlaySound("success.wav", 0.74f);
                    Notification.Show($"Exported {allCommands.Count} commands to {filePath}", 0.6f, false, true);
                }
                catch (Exception e)
                {
                    Logger.Error($"Failed to export commands: {e}");
                }
            },
            keywords: ["dump commands", "save commands", "commands json"]
        );
    }
}

[Serializable]
internal class Wrapper
{
    public List<object> Commands;
}