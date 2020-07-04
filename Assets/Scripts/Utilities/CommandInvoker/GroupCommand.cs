using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public abstract class GroupCommand : Command
{
    /// <summary>
    /// CommandInvoker odpowiedzialny za inicjalizację elementów składowych komendy
    /// </summary>
    public CommandInvoker invoker { get; protected set; }
    private Action<Command> onStageStarted;
    private Action<Command> onStageFinished;

    public GroupCommand(string commandName, Action<Command> onStageStarted, Action<Command> onStageFinished)
        : base (commandName)
    { 
        this.onStageStarted = onStageStarted;
        this.onStageFinished = onStageFinished;
    }

    public override void StartExecution(Action<Command> onCommandFinished)
    {
        base.StartExecution(onCommandFinished);
        invoker = new CommandInvoker(OnStageStarted, OnStageFinished, FinishExecution);
    }

    public override void UpdateExecution()
    {
        base.UpdateExecution();

        invoker?.Update();
    }

    protected virtual void OnStageStarted(Command command)
    {
        onStageStarted?.Invoke(command);
    }

    protected virtual void OnStageFinished(Command command)
    {
        onStageFinished?.Invoke(command);
    }
}
