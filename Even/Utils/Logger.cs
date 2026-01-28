using System;
using System.Runtime.InteropServices;
namespace Even.Utils;

public static class Logger
{
    [DllImport("kernel32.dll")]
    private static extern bool AllocConsole();

    private static bool _consoleInitialized;

    public static void Init()
    {
        if (_consoleInitialized) return;
        AllocConsole();
        _consoleInitialized = true;
        Info("Logger initialized");
    }

    public static void Info(string message)
    {
        WriteLine("[INFO]", message, ConsoleColor.Cyan);
    }

    public static void Warning(string message)
    {
        WriteLine("[WARN]", message, ConsoleColor.Yellow);
    }

    public static void Error(string message)
    {
        WriteLine("[ERROR]", message, ConsoleColor.Red);
    }

    private static void WriteLine(string label, string message, ConsoleColor color)
    {
        if (!_consoleInitialized) Init();

        var originalColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine($"{DateTime.Now:HH:mm:ss} {label} {message}");
        Console.ForegroundColor = originalColor;
    }
}