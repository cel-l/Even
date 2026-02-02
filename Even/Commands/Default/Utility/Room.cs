using System;
using System.Threading.Tasks;
using Even.Utils;
using GorillaNetworking;
using GorillaTagScripts;
using Photon.Pun;
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
                    
                    Notification.Show("Disconnected from room", 0.6f, false, true);
                    Audio.PlaySound("success.wav", 0.74f);
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

public sealed class Rejoin : IEvenCommand
{
    public Command Create()
    {
        return new Command(
            name: "rejoin",
            category: "Utility",
            description: "Leave the current room",
            action: async void () =>
            {
                try
                {
                    if (!NetworkSystem.Instance.InRoom) return;
                    
                    var currentRoomName = NetworkSystem.Instance.RoomName;
                    var joinType = FriendshipGroupDetection.Instance.IsInParty ? JoinType.JoinWithParty : JoinType.Solo;
                    
                    Notification.Show($"Trying to rejoin room {currentRoomName}", 0.6f, false, true);
                    
                    await NetworkSystem.Instance.ReturnToSinglePlayer();
                    await Task.Delay(300);
                        
                    PhotonNetworkController.Instance.AttemptToJoinSpecificRoom(currentRoomName, joinType);
                }
                catch (Exception e)
                {
                    Logger.Error($"Failed to run command 'rejoin': {e}");
                }
            },
            keywords: ["rejoin", "reconnect", "rejoin room"]
        );
    }
}

public sealed class LobbyHop : IEvenCommand
{
    public Command Create()
    {
        return new Command(
            name: "lobby hop",
            category: "Utility",
            description: "Leave the current room (if any) and join a new public lobby",
            action: async void () =>
            {
                try
                {
                    if (PhotonNetwork.InRoom)
                    {
                        await NetworkSystem.Instance.ReturnToSinglePlayer();
                        await Task.Delay(200);
                    }

                    var trigger = PhotonNetworkController.Instance.currentJoinTrigger
                                  ?? GorillaComputer.instance.GetJoinTriggerForZone("forest");
                    
                    Notification.Show($"Trying to find new room", 0.6f, false, true);
                    PhotonNetworkController.Instance.AttemptToJoinPublicRoom(
                        trigger,
                        FriendshipGroupDetection.Instance.IsInParty ? JoinType.JoinWithParty : JoinType.Solo
                    );
                }
                catch (Exception e)
                {
                    Logger.Error($"Failed to run command 'lobby hop': {e}");
                }
            },
            keywords: ["new room", "new lobby"]
        );
    }
}

public sealed class RoomInfo : IEvenCommand
{
    public Command Create()
    {
        return new Command(
            name: "room info",
            category: "Utility",
            description: "Show details about the current room (if in a room)",
            action: void () =>
            {
                try
                {
                    if (!NetworkSystem.Instance.InRoom) return;

                    var roomName = NetworkSystem.Instance.RoomName;
                    var players = NetworkSystem.Instance.RoomPlayerCount;
                    var queue = GorillaComputer.instance.currentQueue;
                    
                    var message = $"You are in a {char.ToUpper(queue[0]) + queue[1..].ToLower()} room {roomName} with {players}/10 players";
                }
                catch (Exception e)
                {
                    Logger.Error($"Failed to run command 'lobby hop': {e}");
                }
            },
            keywords: ["current room", "where am i", "what room am i in"]
        );
    }
}