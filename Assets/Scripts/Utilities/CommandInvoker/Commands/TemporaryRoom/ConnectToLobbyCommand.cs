using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ConnectToLobbyCommand : Command
{
    /// <summary>
    /// Nazwa gracza
    /// </summary>
    private string playerNick;

    /// <summary>
    /// Komenda łącząca gracza do głównego lobby w serwerach Photon
    /// </summary>
    /// <param name="playerNick">Nazwa gracza</param>
    public ConnectToLobbyCommand(string playerNick)
        : base ("Connect To Lobby")
    {
        this.playerNick = playerNick;
    }

    public override void StartExecution(Action<Command> OnCommandFinished)
    {
        base.StartExecution(OnCommandFinished);

        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.NickName = playerNick;
        PhotonNetwork.JoinLobby();
    }

    public override void UpdateExecution()
    {
        base.UpdateExecution();
        if (PhotonNetwork.NetworkClientState == ClientState.JoinedLobby) FinishExecution();
    }
}
