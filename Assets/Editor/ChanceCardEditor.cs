using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ChanceCard))]
public class ChanceCardEditor : Editor
{
    private bool showActions = true;
    private ActionCard.ActionType currentType;
    private List<bool> actionIsOpen = new List<bool>();

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        ChanceCard card = (ChanceCard)target;

        RecreateActions(card);

        AddMenu(card);
        showActions = EditorGUILayout.BeginFoldoutHeaderGroup(showActions, "Actions (" + card.actionStrings.Count + ")");

        if (showActions)
        {
            for (int i = 0; i < card.actionStrings.Count; i++)
            {
                ActionString actionString = card.actionStrings[i];
                ShowActionCard(card, actionString, i);
            }
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    /// <summary>
    /// Otwiera menu do dodawania poszczególnych akcji
    /// </summary>
    /// <param name="card">Obiekt, na którym działa edytor</param>
    private void AddMenu(ChanceCard card)
    {
        EditorGUILayout.BeginHorizontal();

        currentType = (ActionCard.ActionType)EditorGUILayout.EnumPopup("New Action Card", currentType);
        if (GUILayout.Button("Add"))
        {
            ActionString actionString = new ActionString();

            switch (currentType)
            {
                case ActionCard.ActionType.Money:
                    {
                        actionString = new ActionString(ActionCard.ActionType.Money, new List<string>() { "Bank", "Player", "0" });
                    }
                    break;
                case ActionCard.ActionType.Wait:
                    {
                        actionString = new ActionString(ActionCard.ActionType.Wait, new List<string>() { "Player", "0" });
                    }
                    break;
                case ActionCard.ActionType.Move:
                    {
                        actionString = new ActionString(ActionCard.ActionType.Move, new List<string>() { "By", "0", "PlaceId", "0", "Prison"});
                    }
                    break;
            }

            actionIsOpen.Add(true);
            card.actionStrings.Add(actionString);
            EditorUtility.SetDirty(target);
        }

        EditorGUILayout.EndHorizontal();
    }

    /// <summary>
    /// Odtwarza listę przechowującą informację o otwarciu karty z informacjami o akcji
    /// </summary>
    private void RecreateActions(ChanceCard card)
    {
        if (card.actionStrings == null)
            card.actionStrings = new List<ActionString>();

        if (actionIsOpen.Count == 0)
        {
            foreach (ActionString actionString in card.actionStrings)
                actionIsOpen.Add(false);
        }

    }

    /// <summary>
    /// Wyświetla informacje o akcji w danej karcie na podstawie actionStringa
    /// </summary>
    /// <param name="card">Karta, której akcje chcemy wyświetlić</param>
    /// <param name="actionString">Informacje o karcie w postaci listy stringów</param>
    /// <param name="index">Numer akcji na liście wszystkich akcji</param>
    private void ShowActionCard(ChanceCard card, ActionString actionString, int index)
    {
        EditorGUILayout.BeginHorizontal();

        actionIsOpen[index] = EditorGUILayout.Foldout(actionIsOpen[index], actionString.actionType.ToString());
        if (GUILayout.Button("X", GUILayout.Width(20)))
        {
            card.actionStrings.RemoveAt(index);
            actionIsOpen.RemoveAt(index);
        }

        EditorGUILayout.EndHorizontal();

        if (actionIsOpen.Count > index && actionIsOpen[index])
        {
            switch (actionString.actionType)
            {
                case ActionCard.ActionType.Money:
                    ShowMoneyAction(actionString);
                    break;
                case ActionCard.ActionType.Wait:
                    ShowWaitAction(actionString);
                    break;
                case ActionCard.ActionType.Move:
                    ShowMoveAction(actionString);
                    break;
            }
        }

        EditorUtility.SetDirty(target);
    }

    #region Pokazywanie akcji

    private void ShowMoneyAction(ActionString moneyString)
    {
        MoneyAction.MoneyActor payer = (MoneyAction.MoneyActor)Enum.Parse(typeof(MoneyAction.MoneyActor), moneyString.variables[0]);
        moneyString.variables[0] = EditorGUILayout.EnumPopup("Payer", payer).ToString();

        MoneyAction.MoneyActor receiver = (MoneyAction.MoneyActor)Enum.Parse(typeof(MoneyAction.MoneyActor), moneyString.variables[1]);
        moneyString.variables[1] = EditorGUILayout.EnumPopup("Receiver", receiver).ToString();

        float amount = float.Parse(moneyString.variables[2]);
        moneyString.variables[2] = EditorGUILayout.FloatField("Amount", amount).ToString();
    }

    private void ShowWaitAction(ActionString waitString)
    {
        WaitAction.WaitActor target = (WaitAction.WaitActor)Enum.Parse(typeof(WaitAction.WaitActor), waitString.variables[0]);
        waitString.variables[0] = EditorGUILayout.EnumPopup("Target", target).ToString();

        int rounds = int.Parse(waitString.variables[1]);
        waitString.variables[1] = EditorGUILayout.IntField("Rounds", rounds).ToString();
    }

    private void ShowMoveAction(ActionString moveString)
    {
        MoveAction.Mode mode = (MoveAction.Mode)Enum.Parse(typeof(MoveAction.Mode), moveString.variables[0]);
        moveString.variables[0] = EditorGUILayout.EnumPopup("Mode", mode).ToString();

        if(mode == MoveAction.Mode.By)
        {
            int byAmount = int.Parse(moveString.variables[1]);
            moveString.variables[1] = EditorGUILayout.IntField("Amount", byAmount).ToString();
        }
        else if(mode == MoveAction.Mode.To)
        {
            MoveAction.ToTarget toTarget = (MoveAction.ToTarget)Enum.Parse(typeof(MoveAction.ToTarget), moveString.variables[2]);
            moveString.variables[2] = EditorGUILayout.EnumPopup("Target", toTarget).ToString();

            if(toTarget == MoveAction.ToTarget.PlaceId)
            {
                int targetId = int.Parse(moveString.variables[3]);
                moveString.variables[3] = EditorGUILayout.IntField("Place Id", targetId).ToString();
            }
            else if(toTarget == MoveAction.ToTarget.PlaceType)
            {
                MoveAction.PlaceTypeTarget targetType = (MoveAction.PlaceTypeTarget)Enum.Parse(typeof(MoveAction.PlaceTypeTarget), moveString.variables[4]);
                moveString.variables[4] = EditorGUILayout.EnumPopup("Place Type", targetType).ToString();
            }
        }
    }

    #endregion Pokazywanie akcji 
}