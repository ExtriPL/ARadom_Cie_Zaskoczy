using UnityEngine;

public class WaitAction : ActionCard
{
    /// <summary>
    /// Osoba, która będzie czekąc podaną ilość rund
    /// </summary>
    public WaitActor target;
    /// <summary>
    /// Ilość rund, które będzie musiała zaczekać dana osoba
    /// </summary>
    public int rounds;

    public WaitAction(WaitActor target, int rounds)
    {
        this.target = target;
        this.rounds = rounds;
    }

    public override void Call(Player caller, bool showMessage = false)
    {
        switch(target)
        {
            case WaitActor.Player:
                TargetPlayer(caller, showMessage);
                break;
            case WaitActor.Others:
                TargetOthers(caller, showMessage);
                break;
        }
    }

    private void TargetPlayer(Player caller, bool showMessage)
    {
        caller.AddTurnsToSkip(rounds);

        if (showMessage)
            ShowMessage(caller);
    }

    private void TargetOthers(Player caller, bool showMessage)
    {
        GameSession session = GameplayController.instance.session;

        for(int i = 0; i < session.playerCount; i++)
        {
            Player p = session.FindPlayer(i);

            if (!p.GetName().Equals(caller.GetName()))
            {
                ShowMessage(p);
                p.AddTurnsToSkip(rounds);
            }
        }
    }

    private void ShowMessage(Player calller)
    {
        if (calller.NetworkPlayer.IsLocal)
        {
            string message = lang.GetWord("TURNS_LOST") + rounds;
            IconPopup popup = new IconPopup(IconPopupType.Message, message);
            PopupSystem.instance.AddPopup(popup);
        }
        else
        {
            string[] message = new string[] { lang.PackKey("TURNS_LOST"), rounds.ToString() };
            EventManager.instance.SendPopupMessage(message, IconPopupType.Message, calller);
        }
    }

    public enum WaitActor
    {
        Player, Others
    }
}