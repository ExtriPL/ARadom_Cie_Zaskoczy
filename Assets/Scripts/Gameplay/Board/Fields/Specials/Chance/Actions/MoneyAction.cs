using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class MoneyAction : ActionCard
{
    /// <summary>
    /// Osoba, która jest źródłem pieniędzy
    /// </summary>
    public MoneyActor payer;
    /// <summary>
    /// Osoba, która otrzyma pieniądze
    /// </summary>
    public MoneyActor receiver;
    /// <summary>
    /// Ilość pieniędzy, jaka zostanie wymieniona
    /// </summary>
    public float amount;

    public MoneyAction(MoneyActor payer, MoneyActor receiver, float amount)
    {
        this.payer = payer;
        this.receiver = receiver;
        this.amount = amount;
    }

    public override void Call(Player caller, bool showMessage = false)
    {
        //Obsługa przypadku dla banku nie jest konieczna, ponieważ bank nie ma konta
        switch(receiver)
        {
            case MoneyActor.All:
                ReceiverAll(caller, showMessage);
                break;
            case MoneyActor.Others:
                ReceiverOthers(caller, showMessage);
                break;
            case MoneyActor.Player:
                ReceiverPlayer(caller, showMessage);
                break;
        }

        //Jedynie obsługa przypadku dla wszystkich i gracza jest konieczna, ponieważ reszta jest już wyżej obsłużona, albo nie ma sensu
        switch(payer)
        {
            case MoneyActor.All:
                PayerAll(caller, showMessage);
                break;
            case MoneyActor.Player:
                PayerPlayer(caller, showMessage);
                break;
        }
    }

    private void ReceiverAll(Player caller, bool showMessage)
    {
        GameSession session = GameplayController.instance.session;

        //Płatność dla wszystkich ma jedynie sens, gdy źródłem jest bank
        if (payer == MoneyActor.Bank)
        {
            for (int i = 0; i < session.playerCount; i++)
            {
                Player p = session.FindPlayer(i);
                p.IncreaseMoney(amount);

                if (!p.GetName().Equals(caller.GetName()))
                    ShowReceiveFromBankMessage(p);
                else if (showMessage)
                    ShowReceiveFromBankMessage(p);
            }
        }
    }

    private void ReceiverOthers(Player caller, bool showMessage)
    {
        GameSession session = GameplayController.instance.session;
        BankingController banking = GameplayController.instance.banking;

        for (int i = 0; i < session.playerCount; i++)
        {
            Player p = session.FindPlayer(i);
            if (!p.GetName().Equals(caller.GetName()))
            {
                /*
                 Jedynie takie płatności mają sens:
                    Bank => Others
                    Player => Others
                 Wszystkie inne skutkowałyby przelewaniem swoich pieniędzy do siebie i pokazywaniem zbędnych komunikatów
                 */

                if (payer == MoneyActor.Player)
                    banking.Pay(caller, p, amount);
                else if (payer == MoneyActor.Bank)
                {
                    p.IncreaseMoney(amount);
                    ShowReceiveFromBankMessage(p);
                }
            }
        }
    }

    private void ReceiverPlayer(Player caller, bool showMessage)
    {
        GameSession session = GameplayController.instance.session;
        BankingController banking = GameplayController.instance.banking;

        /*
            Źródłem pieniędzy dla gracza mogą być jedynie Bank i Others.
            Nie może być nim Player, ponieważ przelałby sam sobie pieniądze.
            Nie mogą być nim też All, ponieważ uwzględnia to w sobie Player
        */

        if (payer == MoneyActor.Bank)
        {
            caller.IncreaseMoney(amount);
            if (showMessage)
                ShowReceiveFromBankMessage(caller);
        }
        else if (payer == MoneyActor.Others)
        {
            for (int i = 0; i < session.playerCount; i++)
            {
                Player p = session.FindPlayer(i);
                if (!p.GetName().Equals(caller.GetName()))
                {
                    banking.Pay(p, caller, amount);
                    ShowPayToPlayerMessage(p, caller);
                }
            }
        }
    }

    private void PayerAll(Player caller, bool showMessage)
    {
        GameSession session = GameplayController.instance.session;

        //Wszyscy oprócz Bank są już obsłużeni, albo nie mają sensu
        if (receiver == MoneyActor.Bank)
        {
            for(int i = 0; i < session.playerCount; i++)
            {
                Player p = session.FindPlayer(i);

                p.DecreaseMoney(amount);

                if (!p.GetName().Equals(caller.GetName()) || showMessage)
                    ShowPayToBankMessage(p);
            }
        }
    }

    private void PayerPlayer(Player caller, bool showMessage)
    {
        if (receiver == MoneyActor.Bank)
        {
            caller.DecreaseMoney(amount);

            if (showMessage)
                ShowPayToBankMessage(caller);
        }
    }

    private void ShowReceiveFromBankMessage(Player target)
    {
        string[] message = new string[] { lang.PackKey("YOU_RECEIVED"), amount.ToString(), lang.PackKey("RADOM_PENNIES"), lang.PackKey("FROM_BANK") };
        EventManager.instance.SendPopupMessage(message, IconPopupType.Message, target);
    }

    private void ShowPayToBankMessage(Player target)
    {
        string[] message = new string[] { lang.PackKey("YOU_PAY"), amount.ToString(), lang.PackKey("RADOM_PENNIES"), lang.PackKey("FOR_BANK") };
        EventManager.instance.SendPopupMessage(message, IconPopupType.Message, target);
    }

    private void ShowPayToPlayerMessage(Player target, Player moneyReceiver)
    {
        //Zapłaciłęś graczowi moneyReceiver x groszy radomskich
        string[] message = new string[] { lang.PackKey("YOU_PAID_FOR_PLAYER"), moneyReceiver.GetName(),  " " + amount.ToString(), lang.PackKey("RADOM_PENNIES") };
        EventManager.instance.SendPopupMessage(message, IconPopupType.Message, target);
    }

    public enum MoneyActor
    {
        Bank, All, Others, Player
    }
}
