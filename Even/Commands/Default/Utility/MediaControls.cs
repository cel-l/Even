using System;
using Even.Commands.Helpers;
using Logger = Even.Utils.Logger;
namespace Even.Commands.Default.Utility;

public sealed class PlayPauseCommand : IEvenCommand
{
    public Command Create()
    {
        return new Command(
            name: "play pause",
            category: "Utility",
            description: "Toggles play/pause for the current media session",
            action: () =>
            {
                try
                {
                    Multimedia.PlayPause();
                }
                catch (Exception e)
                {
                    Logger.Error($"Failed to run command 'play pause': {e}");
                }
            },
            keywords: ["play", "pause", "resume", "play music", "pause music", "play song", "pause song"]
        );
    }
}

public sealed class NextTrackCommand : IEvenCommand
{
    public Command Create()
    {
        return new Command(
            name: "next song",
            category: "Utility",
            description: "Skips to the next track",
            action: () =>
            {
                try
                {
                    Multimedia.NextTrack();
                }
                catch (Exception e)
                {
                    Logger.Error($"Failed to run command 'next song': {e}");
                }
            },
            keywords: ["next track", "skip song", "skip track"]
        );
    }
}

public sealed class PreviousTrackCommand : IEvenCommand
{
    public Command Create()
    {
        return new Command(
            name: "previous song",
            category: "Utility",
            description: "Goes back to the previous track",
            action: () =>
            {
                try
                {
                    Multimedia.PreviousTrack();
                }
                catch (Exception e)
                {
                    Logger.Error($"Failed to run command 'previous song': {e}");
                }
            },
            keywords: ["last song", "last track", "previous track"]
        );
    }
}