using Even.Commands.Runtime;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace Even.Models.Callbacks;

internal class Photon : MonoBehaviourPunCallbacks
{
    private PlayerCommandRegistry _playerCommands;

    internal void Initialize(PlayerCommandRegistry playerCommands)
    {
        _playerCommands = playerCommands;
    }

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.CurrentRoom == null)
            return;

        _playerCommands?.OnJoinedRoom();
    }

    public override void OnLeftRoom()
    {
        _playerCommands?.OnLeftRoom();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (newPlayer == null)
            return;

        _playerCommands?.OnPlayerEnteredRoom(newPlayer);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (otherPlayer == null)
            return;

        _playerCommands?.OnPlayerLeftRoom(otherPlayer);
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
    }
}