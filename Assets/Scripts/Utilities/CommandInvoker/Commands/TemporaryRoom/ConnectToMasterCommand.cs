using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ConnectToMasterCommand : Command
{
    /// <summary>
    /// Komenda łącząca grę z serwerami Photon
    /// </summary>
    public ConnectToMasterCommand()
        : base("Connect To Master")
    { }

    public override void StartExecution(Action<Command> OnCommandFinished)
    {
        base.StartExecution(OnCommandFinished);

        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.GameVersion = Application.version;
    }

    public override void UpdateExecution()
    {
        base.UpdateExecution();

        if (PhotonNetwork.NetworkClientState == ClientState.ConnectedToMasterServer) FinishExecution();
    }
}
