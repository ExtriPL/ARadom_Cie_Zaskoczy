using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class BoardCommand : GroupCommand
{
    private Board board;

    /// <summary>
    /// Komenda służąca do inicjalizacji planszy
    /// </summary>
    /// <param name="board">Instancja planszy</param>
    public BoardCommand(Board board, Action<Command> onStageStarted = null, Action<Command> onStageFinished = null)
        : base ("Board", onStageStarted, onStageFinished)
    {
        this.board = board;
    }

    public override void StartExecution(Action<Command> OnCommandFinished)
    {
        base.StartExecution(OnCommandFinished);

        Command randomDice = new RandomDiceCommand(board);
        Command loadPlaces = new ActionCommand(
            "LoadPlaces",
            board.LoadPlaces,
            delegate { return PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("board_places"); }
            );
        Command loadTiers = new ActionCommand(
            "LoadTiers",
            board.LoadTiers,
            delegate { return PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("board_tiers"); }
            );

        invoker.AddCommand(randomDice);
        invoker.AddCommand(loadPlaces);
        invoker.AddCommand(loadTiers);

        invoker.Start();
    }
}
