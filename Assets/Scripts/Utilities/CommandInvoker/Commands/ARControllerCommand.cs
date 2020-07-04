using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ARControllerCommand : GroupCommand
{
    private ARController arController;

    /// <summary>
    /// Komenda odpowiedzialna za inicjalizację ARControllera
    /// </summary>
    /// <param name="arController">Instancja ARControllera</param>
    public ARControllerCommand(ARController arController, Action<Command> onStageStarted = null, Action<Command> onStageFinished = null)
        : base ("ARController", onStageStarted, onStageFinished)
    {
        this.arController = arController;
    }

    public override void StartExecution(Action<Command> OnCommandFinished)
    {
        base.StartExecution(OnCommandFinished);

        Command targetFrameRate = new ActionCommand(
            "TargetFrameRate",
            delegate { Application.targetFrameRate = Keys.Gameplay.TARGET_FRAMERATE; }
            );

        Command initBoard = new ActionCommand(
            "InitBoard",
            arController.InitBoard
            );

        invoker.AddCommand(targetFrameRate);
        invoker.AddCommand(initBoard);

        invoker.Start();
    }
}