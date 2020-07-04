using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class GameplayControllerCommand : GroupCommand
{
    private GameplayController gameplayController;

    /// <summary>
    /// Komenda inicjująca GameplayController
    /// </summary>
    /// <param name="gameplayController">Obiekt przechowujący GameplayController</param>
    public GameplayControllerCommand(GameplayController gameplayController, Action<Command> onStageStarted = null, Action<Command> onStageFinished = null)
        : base ("GameplayController", onStageStarted, onStageFinished)
    {
        this.gameplayController = gameplayController;
    }

    public override void StartExecution(Action<Command> OnCommandFinished)
    {
        base.StartExecution(OnCommandFinished);

        Command analyticsEvent = new ActionCommand(
            "AnalyticsEvent",
            delegate { if (gameplayController.session.roomOwner.IsLocal) AnalyticsEventManager.instance.SendOnStartGame(); }
            );

        invoker.AddCommand(analyticsEvent);

        invoker.Start();
    }
}
