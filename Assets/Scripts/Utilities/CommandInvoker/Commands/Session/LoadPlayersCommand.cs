using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class LoadPlayersCommand : Command
{
    private GameSession session;

    /// <summary>
    /// Komenda inicjująca listę graczy
    /// </summary>
    /// <param name="session">Obiekt przechowujący sesję gry</param>
    public LoadPlayersCommand(GameSession session)
        : base ("Load Players")
    {
        this.session = session;
    }

    public override void StartExecution(Action<Command> OnCommandFinished)
    {
        base.StartExecution(OnCommandFinished);

        List<string> playerOrder = new List<string>();
        List<Color> usedColors = new List<Color>();

        foreach (Photon.Realtime.Player p in PhotonNetwork.CurrentRoom.Players.Values)
        {
            Color mainColor;

            do
            {
                mainColor = Keys.Gameplay.PLAYER_COLORS[Random.Range(0, 6)];
            }
            while (usedColors.Contains(mainColor));
            usedColors.Add(mainColor);

            Player player;
            if (session.roomOwner.IsLocal) player = new Player(p, mainColor, Keys.Board.Backlight.SECONDARY_COLOR);
            else player = new Player(p);
            session.AddPlayer(player);

            if (session.roomOwner.IsLocal) playerOrder.Add(player.GetName());
        }

        if (session.roomOwner.IsLocal) session.playerOrder = playerOrder;
    }

    public override void UpdateExecution()
    {
        base.UpdateExecution();
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("session_playerOrder")) FinishExecution();
    }
}