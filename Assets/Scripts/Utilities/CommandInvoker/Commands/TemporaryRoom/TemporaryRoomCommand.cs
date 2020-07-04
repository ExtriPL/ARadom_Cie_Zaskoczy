﻿using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class TemporaryRoomCommand : GroupCommand
{
    /// <summary>
    /// Komenda odpowiadająca za utworzenie pokoju tymczasowego, jeżeli takowy jest potrzebny
    /// </summary>
    public TemporaryRoomCommand(Action<Command> onStageStarted = null, Action<Command> onStageFinished = null)
        : base ("Network Connection", onStageStarted, onStageFinished)
    {}

    public override void StartExecution(Action<Command> OnCommandFinished)
    {
        base.StartExecution(OnCommandFinished);
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom) FinishExecution();
        else
        {
            Command connectToMaster = new ConnectToMasterCommand();
            Command connectToLobby = new ConnectToLobbyCommand(Keys.DefaultRoom.NICKNAME);
            Command connectToRoom = new ConnectToRoomCommand(Keys.DefaultRoom.ROOM_NAME);

            invoker.AddCommand(connectToMaster);
            invoker.AddCommand(connectToLobby);
            invoker.AddCommand(connectToRoom);

            invoker.Start();
            UnityEngine.Debug.LogWarning("Uruchamianie gry na awaryjnym pokoju");
        }
    }
}
