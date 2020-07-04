using ExitGames.Client.Photon;
using Photon.Pun;
using System;
using System.Collections.Generic;
using UnityEngine;

public class LoadFromSaveCommand : GroupCommand
{
    /// <summary>
    /// Komenda odpowiedzialna za wczytywanie gry z pliku
    /// </summary>
    public LoadFromSaveCommand(Action<Command> onStageStarted = null, Action<Command> onStageFinished = null)
        : base ("LoadFromSave", onStageStarted, onStageFinished)
    {}

    public override void StartExecution(Action<Command> OnCommandFinished)
    {
        base.StartExecution(OnCommandFinished);
        invoker = new CommandInvoker(OnStageStarted, OnStageFinished, null);

        bool loadFromSave = PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("loadSavedGame") && (bool)PhotonNetwork.CurrentRoom.CustomProperties["loadSavedGame"];

        if (GameplayController.instance.session.roomOwner.IsLocal && loadFromSave)
        {
            Action switchLoadSaveState = delegate
            {
                Hashtable table = new Hashtable();
                table.Add("loadSavedGame", false);
                PhotonNetwork.CurrentRoom.SetCustomProperties(table);
            };

            invoker = new CommandInvoker(OnStageStarted, OnStageFinished, switchLoadSaveState);
            GameSave save = new GameSave();

            string fileName = (string)PhotonNetwork.CurrentRoom.CustomProperties["saveFileName"];
            FileManager.LoadGame(ref save, fileName);

            Command loadSaveSession = new ActionCommand(
                "LoadSaveSession",
                delegate { GameplayController.instance.session.LoadFromSave(ref save); }
                );
            Command loadSaveBoard = new ActionCommand(
                "LoadSaveBoard",
                delegate { GameplayController.instance.board.LoadFromSave(ref save); }
                );

            invoker.AddCommand(loadSaveSession);
            invoker.AddCommand(loadSaveBoard);
        }

        invoker.Start();
    }

    public override void UpdateExecution()
    {
        base.UpdateExecution();
        if (!PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("loadSavedGame") || !(bool)PhotonNetwork.CurrentRoom.CustomProperties["loadSavedGame"]) FinishExecution();
    }
}