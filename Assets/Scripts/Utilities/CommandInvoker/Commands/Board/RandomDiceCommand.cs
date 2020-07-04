using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class RandomDiceCommand : Command
{
    private Board board;

    /// <summary>
    /// Komenda służąca do inicjalizacji kostki
    /// </summary>
    /// <param name="board">Instancja planszy</param>
    public RandomDiceCommand(Board board)
        : base ("RandomDice")
    {
        this.board = board;
    }

    public override void StartExecution(Action<Command> OnCommandFinished)
    {
        base.StartExecution(OnCommandFinished);

        if (GameplayController.instance.session.roomOwner.IsLocal)
        {
            board.dice = new RandomDice(0, 0, GameplayController.instance.session.playerOrder[0], 0, 0);
            board.dice.RollDice();
        }
        else board.dice = new RandomDice();
    }

    public override void UpdateExecution()
    {
        base.UpdateExecution();
        if (ProportiesExists()) FinishExecution();
    }

    /// <summary>
    /// Sprawdza, czy wszystkie wartości sieciowe kostki istnieją
    /// </summary>
    /// <returns></returns>
    private bool ProportiesExists()
    {
        bool exists = true;
        List<string> proporties = new List<string>
        {
            "dice_last1",
            "dice_last2",
            "dice_currentPlayer",
            "dice_amountOfRolls",
            "dice_round"
        };

        foreach (string proporty in proporties) exists = PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(proporty);

        return exists;
    }
}
