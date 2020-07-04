using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ActionCommand : Command
{
    /// <summary>
    /// Delegat wywoływany przez komende
    /// </summary>
    private Action action;
    /// <summary>
    /// >Warunek, pod jakim kończy wykonywanie się komendy
    /// </summary>
    private Func<bool> condition;

    /// <summary>
    /// Uniwersalna komenda służaca do wywoływania krótkich akcji
    /// </summary>
    /// <param name="commandName">Nazwa komendy</param>
    /// <param name="action">Delegat wywoływany przez komende</param>
    public ActionCommand(string commandName, Action action)
        : base (commandName)
    {
        this.action = action;
        condition = delegate { return true; };
    }

    /// <summary>
    /// Uniwersalna komenda służaca do wywoływania krótkich akcji
    /// </summary>
    /// <param name="commandName">Nazwa komendy</param>
    /// <param name="action">Delegat wywoływany przez komende</param>
    /// <param name="condition">Warunek, pod jakim kończy wykonywanie się komendy</param>
    public ActionCommand(string commandName, Action action, Func<bool> condition)
        : base (commandName)
    {
        this.action = action;
        this.condition = condition;
    }

    public override void StartExecution(Action<Command> OnCommandFinished)
    {
        base.StartExecution(OnCommandFinished);

        action?.Invoke();
    }

    public override void UpdateExecution()
    {
        base.UpdateExecution();
        if (condition != null && condition.Invoke()) FinishExecution();
        else if (condition == null) FinishExecution();
    }
}