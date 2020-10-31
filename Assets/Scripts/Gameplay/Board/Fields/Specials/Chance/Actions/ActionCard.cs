using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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
            case ActionType.WithUser:
                {
                    ActionType insideType = (ActionType)Enum.Parse(typeof(ActionType), form.variables[0]);
                    if (insideType != ActionType.WithUser)
                    {
                        ActionString insideActionString = ActionString.FromString(form.variables[(int)insideType + 1]);
                        ActionCard insideAction = Create(insideActionString);

                        card = new WithUserAction(insideAction);
                    }
                    else
                        Debug.LogError("Nie można zagnieżdzać typu WithUser w akcji typu WithUser");
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
        Move,
        WithUser
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
            case ActionCard.ActionType.WithUser:
                {
                    List<string> variables = new List<string>();
                    variables.Add(ActionCard.ActionType.Money.ToString());

                    //Ustawianie domyślnych wartości dla wszystkich możliwych ActionType
                    foreach(ActionCard.ActionType type in Enum.GetValues(typeof(ActionCard.ActionType)))
                    {
                        if (type == ActionCard.ActionType.WithUser)
                            variables.Add(new ActionString(ActionCard.ActionType.WithUser, new List<string>()).ToString());
                        else
                            variables.Add(GenerateDefault(type).ToString());
                    }

                    return new ActionString(ActionCard.ActionType.WithUser, variables);
                }
        }

        return null;
    }

    public static ActionString FromString(string values)
    {
        ActionCard.ActionType actionType = ActionCard.ActionType.Money;
        List<string> unpackedValues = new List<string>();
        List<string> list = SplitString(values, '{', '}');

        for(int i = 0; i < list.Count; i++)
        {
            string unpacked = list[i].Substring(1, list[i].Length - 2);

            if (i == 0)
                actionType = (ActionCard.ActionType)Enum.Parse(typeof(ActionCard.ActionType), unpacked);
            else
                unpackedValues.Add(unpacked);
        }

        return new ActionString(actionType, unpackedValues);
    }

    private static  List<string> SplitString(string values, char beginSign, char endSign)
    {
        List<string> list = new List<string>();

        int start = 0;
        int end = 0;
        int beginSigns = 0;
        int endSigns = 0;

        for(int i = 1; i < values.Length - 1; i++)
        {
            char current = values[i];
            if (current == beginSign)
            {
                if(beginSigns == 0)
                    start = i;
                beginSigns++;
            }
            else if (current == endSign)
            {
                end = i;
                endSigns++;
            }

            if(beginSigns == endSigns)
            {
                beginSigns = 0;
                endSigns = 0;

                list.Add(values.Substring(start, end - start + 1));
            }
        }

        return list;
    }

    public override string ToString()
    {
        string values = "{{" +  actionType.ToString() + "}";
        
        for(int i = 0; i < variables.Count; i++)
        {
            values += "{" + variables[i] + "}";
        }

        values += "}";

        return values;
    }
}