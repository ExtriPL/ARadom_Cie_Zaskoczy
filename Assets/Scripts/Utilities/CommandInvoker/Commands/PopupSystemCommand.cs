using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }

    public override void UpdateExecution()
    {
        base.UpdateExecution();
        if (PopupSystem.instance.boxPools.Count > 0) FinishExecution();
    }
}
