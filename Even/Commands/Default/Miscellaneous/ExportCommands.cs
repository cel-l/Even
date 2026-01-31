using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Even.Utils;
using UnityEngine;
using Logger = Even.Utils.Logger;

namespace Even.Commands.Default.Miscellaneous
{
    public sealed class ExportCommands : IEvenCommand
    {
        public Command Create()
        {
            return new Command(
                name: "export commands",
                category: "Utility",
                description: "Exports all registered commands to a JSON file",
                action: () =>
                {
                    try
                    {
                        var allCommands = Plugin.Instance._commands;

                        var jsonData = allCommands.Select(c => new SerializableCommand
                        {
                            Name = c.Name,
                            Category = c.Category,
                            Description = c.Description,
                            Keywords = c.Keywords?.ToArray() ?? new string[0]
                        }).ToList();

                        var wrapper = new Wrapper { Commands = jsonData };
                        var json = JsonUtility.ToJson(wrapper, true);

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
                keywords: new[] { "dump commands", "save commands" }
            );
        }
    }

    [Serializable]
    public class SerializableCommand
    {
        public string Name;
        public string Category;
        public string Description;
        public string[] Keywords;
    }

    [Serializable]
    public class Wrapper
    {
        public List<SerializableCommand> Commands;
    }
}