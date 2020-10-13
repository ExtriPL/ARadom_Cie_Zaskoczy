using ExitGames.Client.Photon;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class SyncCommand : Command, IEventSubscribable//GroupCommand
{
    private float lastTime;
    private int syncCommand;

    /// <summary>
    /// string - nazwa gracza
    /// 1: bool - czy dostaliśmy odpowiedź na event
    /// 2: czy dostaliśmy wysłanie na cudzy event
    /// </summary>
    private Dictionary<string, Tuple<bool, bool>> players = new Dictionary<string, Tuple<bool, bool>>();

    /// <summary>
    /// Komenda służąca do zapewnienia synchronizacji między różnymi graczami
    /// </summary>
    public SyncCommand(int syncCommand,Action<Command> onStageStarted = null, Action<Command> onStageFinished = null)
        : base ("Sync")
    {
        this.syncCommand = syncCommand;
    }

    public override void StartExecution(Action<Command> OnCommandFinished)
    {
        base.StartExecution(OnCommandFinished);
        lastTime = Time.time;

        players.Clear();

        foreach (string playerName in GameplayController.instance.session.playerOrder)
        {
            if (!GameplayController.instance.session.localPlayer.GetName().Equals(playerName)) players.Add(playerName, Tuple.Create(false, false));
        }
    }  

    public override void UpdateExecution()
    {
        base.UpdateExecution();
        
        if(Time.time - lastTime >= 0.2f)
        {
            bool canPass = true;

            lastTime = Time.time;
            foreach (string playerName in GameplayController.instance.session.playerOrder)
            {
                if (!GameplayController.instance.session.localPlayer.GetName().Equals(playerName))
                {
                    if (!players[playerName].Item1)
                    {
                        EventManager.instance.SendSyncEvent(0, GameplayController.instance.session.localPlayer.GetName(), playerName, syncCommand);
                        canPass = false;
                    }
                    else
                    {
                        if (!(players[playerName].Item1 && players[playerName].Item2))
                        {
                            canPass = false;
                            break;
                        }
                    }
                }
            }

            if (canPass) FinishExecution();
        } 
    }

    protected override void FinishExecution()
    {
        base.FinishExecution();
    }

    public void SubscribeEvents()
    {
        EventManager.instance.onSync += OnSync;
    }

    public void UnsubscribeEvents()
    {
        EventManager.instance.onSync -= OnSync;
    }

    private void OnSync(int syncNumber, string source, string target, int syncCommand)
    {
        if (this.syncCommand != syncCommand)
            return;

        if (players.ContainsKey(source) && GameplayController.instance.session.FindPlayer(target).NetworkPlayer.IsLocal)
        {
            if (syncNumber == 0)
            {
                players[source] = Tuple.Create(players[source].Item1, true);
                EventManager.instance.SendSyncEvent(1, target, source, syncCommand);
            }
            else if (syncNumber == 1)
            {
                players[source] = Tuple.Create(true, players[source].Item2);
            }
        }
    }
}