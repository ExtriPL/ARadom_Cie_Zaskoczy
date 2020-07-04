using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[System.Serializable]
public abstract class Command
{
    /// <summary>
    /// Akcje wywoływane, gdy komenda zakończy wykonywanie
    /// </summary>
    protected Action<Command> onCommandFinished;
    /// <summary>
    /// Nazwa komendy
    /// </summary>
    public string commandName { get; private set; }

    /// <summary>
    /// Konstruktor bazowej komendy
    /// </summary>
    /// <param name="commandName">Nazwa komnendy</param>
    protected Command(string commandName)
    {
        this.commandName = commandName;
    }

    /// <summary>
    /// Rozpoczyna egzekucję komendy
    /// </summary>
    /// <param name="onCommandFinished">Akcje wywoływane, gdy komenda zakończy egzekucję</param>
    public virtual void StartExecution(Action<Command> onCommandFinished)
    {
        this.onCommandFinished = onCommandFinished;
    }

    /// <summary>
    /// Wykonuje się wielokrotnie w trakcie cyklu egzekucji komendy
    /// </summary>
    public virtual void UpdateExecution() {}

    /// <summary>
    /// Kończy wywoływanie komendy
    /// </summary>
    protected virtual void FinishExecution()
    {
        onCommandFinished?.Invoke(this);
    }
}