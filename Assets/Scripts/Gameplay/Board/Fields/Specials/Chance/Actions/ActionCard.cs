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
            case ActionType.Wait:
                {
                    WaitAction.WaitActor target = (WaitAction.WaitActor)Enum.Parse(typeof(WaitAction.WaitActor), form.variables[0]);
                    int rounds = int.Parse(form.variables[1]);

                    card = new WaitAction(target, rounds);
                }
                break;
            case ActionType.Move:
                {
                    MoveAction.Mode mode = (MoveAction.Mode)Enum.Parse(typeof(MoveAction.Mode), form.variables[0]);
                    int byAmount = int.Parse(form.variables[1]);
                    MoveAction.ToTarget toTarget = (MoveAction.ToTarget)Enum.Parse(typeof(MoveAction.ToTarget), form.variables[2]);
                    int targetId = int.Parse(form.variables[3]);
                    MoveAction.PlaceTypeTarget targetType = (MoveAction.PlaceTypeTarget)Enum.Parse(typeof(MoveAction.PlaceTypeTarget), form.variables[4]);
                    MoveAction.MovementType movementType = (MoveAction.MovementType)Enum.Parse(typeof(MoveAction.MovementType), form.variables[5]);

                    card = new MoveAction(mode, byAmount, toTarget, targetId, targetType, movementType);
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
        Money,
        Wait,
        Move
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

    public static ActionString GenerateDefault(ActionCard.ActionType actionType)
    {
        switch(actionType)
        {
            case ActionCard.ActionType.Money:
                return new ActionString(ActionCard.ActionType.Money, new List<string>() { "Bank", "Player", "0" });
            case ActionCard.ActionType.Wait:
                return new ActionString(ActionCard.ActionType.Wait, new List<string>() { "Player", "0" });
            case ActionCard.ActionType.Move:
                return new ActionString(ActionCard.ActionType.Move, new List<string>() { "By", "0", "PlaceId", "0", "Prison", "Regular" });
        }

        return null;
    }
}