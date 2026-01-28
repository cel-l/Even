using System;
using System.Threading.Tasks;
using GorillaNetworking;
using GorillaTagScripts;
using Logger = Even.Utils.Logger;
namespace Even.Commands.Default.Utility;

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