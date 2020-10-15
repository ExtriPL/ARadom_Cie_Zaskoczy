using System;
using UnityEngine;

public class PopupSystemCommand : GroupCommand
{

    /// <summary>
    /// Komenda odpowiedzialna za inicjację PopupSystemu
    /// </summary>
    public PopupSystemCommand(Action<Command> onStageStarted = null, Action<Command> onStageFinished = null)
        : base ("PopupSystem", onStageStarted, onStageFinished)
    {}

    public override void StartExecution(Action<Command> OnCommandFinished)
    {
        base.StartExecution(OnCommandFinished);
        Debug.Log("Popup System init");

        invoker = new CommandInvoker(OnStageStarted, OnStageFinished, null);

        Command destroyPools = new ActionCommand(
            "DestroyPools",
            PopupSystem.instance.DestroyPools
            );
        Command createPools = new ActionCommand(
            "CreatePools",
            PopupSystem.instance.CreatePools
            );
        Command initPools = new ActionCommand(
            "InitPools",
            PopupSystem.instance.InitPools
            );

        invoker.AddCommand(destroyPools);
        invoker.AddCommand(createPools);
        invoker.AddCommand(initPools);

        invoker.Start();
        invoker.onExecutionFinished += FinishExecution;
    }

    public override void UpdateExecution()
    {
        base.UpdateExecution();
        invoker.Update();
    }
}
