using Photon.Pun;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

[System.Serializable]
public class CommandInvoker
{
    private Queue<Command> commandQueue = new Queue<Command>();
    private Action<Command> onCommandStarted;
    private Action<Command> onCommandFinished;
    private Action onExecutionFinished;

    /// <summary>
    /// Konstruktor CommandInvokera
    /// </summary>
    /// <param name="onCommandFinished">Akcje wywoływanie po zakończeniu egzekucji pojedyńczej komendy</param>
    /// <param name="onExecutionFinished">Akcje wywoływane po zakończeniu egzekucji wszystkjich zaplanowanych komend</param>
    public CommandInvoker(Action<Command> onCommandStarted,  Action<Command> onCommandFinished, Action onExecutionFinished)
    {
        this.onCommandStarted = onCommandStarted;
        this.onCommandFinished = onCommandFinished;
        this.onExecutionFinished = onExecutionFinished;
    }

    /// <summary>
    /// Rozpoczyna wczytywanie kolejki komend
    /// </summary>
    public void Start()
    {
        if (commandQueue?.Count > 0)
        {
            onCommandStarted?.Invoke(commandQueue?.Peek());
            commandQueue?.Peek()?.StartExecution(OnCommandFinished);
        }
        else End();
    }

    /// <summary>
    /// Wykonywanie funkcji Update w komendach.
    /// </summary>
    public void Update()
    {
        if (commandQueue?.Count > 0) commandQueue?.Peek()?.UpdateExecution();
    }

    /// <summary>
    /// Funkcja kończąca wykonwyanie kolejki egzekucji komend
    /// </summary>
    private void End()
    {
        onExecutionFinished?.Invoke();

        onCommandStarted = null;
        onCommandFinished = null;
        onExecutionFinished = null;
    }

    /// <summary>
    /// Dodawanie komendy do kolejki wykonywania
    /// </summary>
    /// <param name="command">Komenda, która zostanie dodana do kolejki</param>
    public void AddCommand(Command command)
    {
        commandQueue?.Enqueue(command);
    }

    /// <summary>
    /// Dodawanie listy komend do kolejki wykonywania
    /// </summary>
    /// <param name="commands">Lista komendy do dadania do kolejki wykonywania</param>
    public void AddCommand(Command[] commands)
    {
        foreach (Command command in commands) AddCommand(command);
    }

    /// <summary>
    /// Akcja wywoływana, gdy pojedyńcza komenda zostanie zakończona
    /// </summary>
    /// <param name="command">Komenda, która skończyła swoją egzekucję</param>
    private void OnCommandFinished(Command command)
    {
        commandQueue?.Dequeue();
        onCommandFinished?.Invoke(command);

        if (commandQueue != null && commandQueue.Count > 0)
        {
            onCommandStarted?.Invoke(commandQueue.Peek());
            commandQueue?.Peek()?.StartExecution(OnCommandFinished);
        }
        else End();
    }
}
