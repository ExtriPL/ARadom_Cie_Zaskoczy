using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Serializable]
public abstract class ActionCard
{
    /// <summary>
    /// Wywołanie akcji
    /// </summary>
    /// <param name="caller">Gracz, dla którego zostanie wywołana akcja</param>
    public abstract void Call(Player caller);

    /// <summary>
    /// Tworzy obiekt Akcji na podstawie informacji zawartych w ActionStringu(Nazwa klasy + lista zmiennych)
    /// </summary>
    /// <param name="form">Obiekt przechowujący informacje o ActionCardzie</param>
    /// <returns></returns>
    public static ActionCard Create(ActionString form)
    {
        ActionCard card = null;

        switch(form.actionType)
        {
            case ActionType.Money:
                {
                    MoneyAction.MoneyActor payer = (MoneyAction.MoneyActor)Enum.Parse(typeof(MoneyAction.MoneyActor), form.variables[0]);
                    MoneyAction.MoneyActor receiver = (MoneyAction.MoneyActor)Enum.Parse(typeof(MoneyAction.MoneyActor), form.variables[1]);
                    float value = float.Parse(form.variables[2]);

                    card = new MoneyAction(payer, receiver, value);
                }
                break;
        }

        return card;
    }

    /// <summary>
    /// Lista wszystkich możliwych typów akcji
    /// </summary>
    public enum ActionType
    {
        Money
    }
}

[Serializable]
public class ActionString
{
    /// <summary>
    /// Typ klasy, której informacje przechowuje obiekt
    /// </summary>
    public ActionCard.ActionType actionType;
    /// <summary>
    /// Wartości zmiennych, podane w kolejności zgodnej z deklaracją w odpowiedniek klasie Action
    /// </summary>
    public List<string> variables;

    public ActionString()
    {
        actionType = ActionCard.ActionType.Money;
        variables = new List<string>();
    }

    public ActionString(ActionCard.ActionType actionType, List<string> variables)
    {
        this.actionType = actionType;
        this.variables = variables;
    }
}