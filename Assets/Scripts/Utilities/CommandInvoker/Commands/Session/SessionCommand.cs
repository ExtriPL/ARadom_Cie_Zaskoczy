using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class SessionCommand : GroupCommand
{
    private GameSession session;

    /// <summary>
    /// Komenda odpowiedzialna za inicjalizację sesji gry
    /// </summary>
    /// <param name="session">Obiekt przechowujący sesję</param>
    public SessionCommand(GameSession session, Action<Command> onStageStarted = null, Action<Command> onStageFinished = null)
        : base ("Session", onStageStarted, onStageFinished)
    {
        this.session = session;
    }

    public override void StartExecution(Action<Command> OnCommandFinished)
    {
        base.StartExecution(OnCommandFinished);

        Command loadPlayers = new LoadPlayersCommand(session);
        Command sessionVariables = new ActionCommand(
            "SessionVariables",
            delegate
            {
                if (session.roomOwner.IsLocal)
                {
                    session.gameState = GameState.running;
                    session.lastGameTime = 0;
                }
            },
            delegate { return PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("session_gameState") && PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("session_gameTime"); }
            );
        //Command sessionVariables = new SessionVariablesCommand(session);

        invoker.AddCommand(loadPlayers);
        invoker.AddCommand(sessionVariables);

        invoker.Start();
    }
}
