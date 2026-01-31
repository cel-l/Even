using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Even.Utils;
using UnityEngine;
using Logger = Even.Utils.Logger;
using Newtonsoft.Json;
// ReSharper disable UnusedAutoPropertyAccessor.Local
namespace Even.Commands.Default.Miscellaneous;

public sealed class ExportCommands : IEvenCommand
{
    private class CommandExport
    {
        public string Name { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string Keywords { get; set; }
    }

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
                    var exportList = new List<CommandExport>();

                    foreach (var cmd in allCommands)
                    {
                        try
                        {
                            var keywords = cmd.Keywords != null 
                                ? $"[{string.Join(", ", cmd.Keywords.Select(k => $"\"{k}\""))}]" 
                                : "[]";

                            exportList.Add(new CommandExport
                            {
                                Name = cmd.Name ?? "Unnamed",
                                Category = cmd.Category ?? "Uncategorized",
                                Description = cmd.Description ?? "",
                                Keywords = keywords
                            });
                        }
                        catch (Exception exCmd)
                        {
                            Logger.Warning($"Failed to process command '{cmd.Name ?? "Unknown"}': {exCmd}");
                        }
                    }

                    var json = JsonConvert.SerializeObject(exportList, Formatting.Indented);
                    var filePath = Path.Combine(Application.persistentDataPath, "commands_export.json");
                    File.WriteAllText(filePath, json);

                    Audio.PlaySound("success.wav", 0.74f);
                    Notification.Show($"Exported {exportList.Count} commands to {filePath}", 0.6f, false, true);
                }
                catch (Exception e)
                {
                    Logger.Error($"Failed to export commands: {e}");
                }
            },
            keywords: ["dump commands", "save commands"]
        );
    }
}