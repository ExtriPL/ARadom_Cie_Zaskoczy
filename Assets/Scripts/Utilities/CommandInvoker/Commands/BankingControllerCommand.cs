using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class BankingControllerCommand : GroupCommand
{
    private BankingController banking;

    /// <summary>
    /// Komenda odpowiedzialna za inicjalizację BankingControllera
    /// </summary>
    /// <param name="banking">Obiekt przechowujący BankingController</param>
    public BankingControllerCommand(BankingController banking, Action<Command> onStageStarted = null, Action<Command> onStageFinished = null)
        : base ("BankingController", onStageStarted, onStageFinished)
    {
        this.banking = banking;
    }

    public override void StartExecution(Action<Command> OnCommandFinished)
    {
        base.StartExecution(OnCommandFinished);

        Command initBanking = new ActionCommand(
            "InitBanking",
            banking.InitBanking
            );

        invoker.AddCommand(initBanking);

        invoker.Start();
    }
}
