using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
namespace Even.Commands;

public static class Loader
{
    public static List<Command> LoadBuiltIn()
    {
        var result = new List<Command>();

        var asm = Assembly.GetExecutingAssembly();
        var sources = asm
            .GetTypes()
            .Where(t =>
                !t.IsAbstract &&
                typeof(IEvenCommand).IsAssignableFrom(t) &&
                t.GetConstructor(Type.EmptyTypes) != null)
            .OrderBy(t => t.FullName, StringComparer.Ordinal);

        foreach (var type in sources)
        {
            try
            {
                var src = (IEvenCommand)Activator.CreateInstance(type)!;

                var cmd = src.Create();
                if (cmd != null)
                    result.Add(cmd);
            }
            catch
            {
                // Don't break the whole command system if one command fails to load
            }
        }

        return result;
    }
}