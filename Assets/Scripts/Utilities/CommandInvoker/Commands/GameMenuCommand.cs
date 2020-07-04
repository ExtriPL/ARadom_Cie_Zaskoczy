using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GameMenuCommand : GroupCommand
{
    private GameMenu gameMenu;

    /// <summary>
    /// Komenda służąca do inicjalizacji GameMenu
    /// </summary>
    /// <param name="gameMenu">Obiekt przechowujący GameMenu</param>
    public GameMenuCommand(GameMenu gameMenu, Action<Command> onStageStarted = null, Action<Command> onStageFinished = null)
        : base("GameMenu", onStageStarted, onStageFinished)
    {
        this.gameMenu = gameMenu;
    }

    public override void StartExecution(Action<Command> OnCommandFinished)
    {
        base.StartExecution(OnCommandFinished);

        gameMenu.InitMenu();
        Command initMenu = new ActionCommand(
            "InitMenu",
            gameMenu.InitMenu
            );

        invoker.AddCommand(initMenu);

        invoker.Start();
    }
} 