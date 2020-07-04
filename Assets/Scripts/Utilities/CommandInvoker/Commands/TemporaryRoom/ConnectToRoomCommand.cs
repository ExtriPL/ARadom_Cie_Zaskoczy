using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ConnectToRoomCommand : Command
{
    /// <summary>
    /// Nazwa pokoju
    /// </summary>
    private string roomName;

    /// <summary>
    /// Komenda łącząca gracza do pokoju na serwerze Photon
    /// </summary>
    /// <param name="roomName">Nazwa pokoju</param>
    public ConnectToRoomCommand(string roomName)
        : base ("Connect To Room")
    {
        this.roomName = roomName;
    }

    public override void StartExecution(Action<Command> OnCommandFinished)
    {
        base.StartExecution(OnCommandFinished);
        RoomOptions roomOptions = new RoomOptions() { IsVisible = false, IsOpen = false, MaxPlayers = 1 };
        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }

    public override void UpdateExecution()
    {
        base.UpdateExecution();
        if (PhotonNetwork.NetworkClientState == ClientState.Joined) FinishExecution();
    }
}