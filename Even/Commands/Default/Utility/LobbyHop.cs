using System;
using System.Threading.Tasks;
using Even.Utils;
using GorillaNetworking;
using GorillaTagScripts;
using Photon.Pun;
using Logger = Even.Utils.Logger;
namespace Even.Commands.Default.Utility;

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
                    
                    Notification.Show($"Left current room, trying to find new room", 0.6f, false, true);
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