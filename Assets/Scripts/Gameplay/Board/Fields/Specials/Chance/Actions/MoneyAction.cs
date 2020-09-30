using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class MoneyAction : ActionCard
{
    public MoneyActor payer, receiver;
    public float amount;

    public MoneyAction(MoneyActor payer, MoneyActor receiver, float amount)
    {
        this.payer = payer;
        this.receiver = receiver;
        this.amount = amount;
    }

    public override void Call(Player caller)
    {
        //Obsługa przypadku dla banku nie jest konieczna, ponieważ bank nie ma konta
        switch(receiver)
        {
            case MoneyActor.All:
                ReceiverAll(caller);
                break;
            case MoneyActor.Others:
                ReceiverOthers(caller);
                break;
            case MoneyActor.Player:
                ReceiverPlayer(caller);
                break;
        }

        //Jedynie obsługa przypadku dla wszystkich i gracza jest konieczna, ponieważ reszta jest już wyżej obsłużona, albo nie ma sensu
        switch(payer)
        {
            case MoneyActor.All:
                PayerAll(caller);
                break;
            case MoneyActor.Player:
                PayerPlayer(caller);
                break;
        }

        Debug.LogError("Brak komunikatów");
    }

    private void ReceiverAll(Player caller)
    {
        GameSession session = GameplayController.instance.session;

        //Płatność dla wszystkich ma jedynie sens, gdy źródłem jest bank
        if (payer == MoneyActor.Bank)
        {
            for (int i = 0; i < session.playerCount; i++)
            {
                Player p = session.FindPlayer(i);
                p.IncreaseMoney(amount);
                //Komunikat dla wszystkich graczy o otrzymaniu pieniędzy
            }
        }
    }

    private void ReceiverOthers(Player caller)
    {
        GameSession session = GameplayController.instance.session;
        BankingController banking = GameplayController.instance.banking;

        for (int i = 0; i < session.playerCount; i++)
        {
            Player p = session.FindPlayer(i);
            if (!p.NetworkPlayer.IsLocal)
            {
                /*
                 Jedynie takie płatności mają sens:
                    Bank => Others
                    Player => Others
                 Wszystkie inne skutkowałyby przelewaniem swoich pieniędzy do siebie i pokazywaniem zbędnych komunikatów
                 */

                if (payer == MoneyActor.Player)
                    banking.Pay(session.localPlayer, p, amount);
                else if (payer == MoneyActor.Bank)
                {
                    p.IncreaseMoney(amount);
                    //Dodać komunikat
                }
            }
        }
    }

    private void ReceiverPlayer(Player caller)
    {
        GameSession session = GameplayController.instance.session;
        BankingController banking = GameplayController.instance.banking;

        /*
            Źródłem pieniędzy dla gracza mogą być jedynie Bank i Others.
            Nie może być nim Player, ponieważ przelałby sam sobie pieniądze.
            Nie mogą być nim też All, ponieważ uwzględnia to w sobie Player
        */

        if (payer == MoneyActor.Bank)
            session.localPlayer.IncreaseMoney(amount);
        else if (payer == MoneyActor.Others)
        {
            for (int i = 0; i < session.playerCount; i++)
            {
                Player p = session.FindPlayer(i);
                if (!p.NetworkPlayer.IsLocal)
                {
                    banking.Pay(p, session.localPlayer, amount);
                    //Odpowiedni komunikat
                }
            }
        }
    }

    private void PayerAll(Player caller)
    {
        GameSession session = GameplayController.instance.session;

        //Wszyscy oprócz Bank są już obsłużeni, albo nie mają sensu
        if (receiver == MoneyActor.Bank)
        {
            for(int i = 0; i < session.playerCount; i++)
            {
                Player p = session.FindPlayer(i);

                p.DecreaseMoney(amount);
            }
        }
    }

    private void PayerPlayer(Player caller)
    {
        if(receiver == MoneyActor.Bank)
            caller.DecreaseMoney(amount);
    }

    public enum MoneyActor
    {
        Bank, All, Others, Player
    }
}
