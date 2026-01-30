using System;

namespace Even.Commands.Helpers;

public static class StatusText
{
    /// <summary>
    /// Converts a bool into a colored "enabled"/"disabled" string for notifications
    /// </summary>
    /// <param name="enabled">The boolean value</param>
    /// <returns>Rich text string with colored status</returns>
    public static string FromBool(bool enabled)
    {
        return enabled
            ? "<color=#00FF00>enabled</color>"
            : "<color=#FF0000>disabled</color>";
    }
}