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

    public override void Call(Player caller)
    {
        switch(target)
        {
            case WaitActor.Player:
                TargetPlayer(caller);
                break;
            case WaitActor.Others:
                TargetOthers(caller);
                break;
        }

        Debug.LogError("Brak komunikatów");
    }

    private void TargetPlayer(Player caller)
    {
        caller.AddTurnsToSkip(rounds);
    }

    private void TargetOthers(Player caller)
    {
        GameSession session = GameplayController.instance.session;

        for(int i = 0; i < session.playerCount; i++)
        {
            Player p = session.FindPlayer(i);

            if (!p.NetworkPlayer.IsLocal)
                p.AddTurnsToSkip(rounds);
        }
    }

    public enum WaitActor
    {
        Player, Others
    }
}