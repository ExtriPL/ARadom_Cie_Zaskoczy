using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ChanceCard))]
public class ChanceCardEditor : Editor
{
    private bool showActions = false;
    private ActionCard.ActionType currentType;
    private List<bool> actionIsOpen = new List<bool>();

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        ChanceCard card = (ChanceCard)target;
        
        RecreateActions(card);

        AddMenu(card);
        showActions = EditorGUILayout.BeginFoldoutHeaderGroup(showActions, "Actions (" + card.actionStrings.Count + ")");

        if(showActions)
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
        if(GUILayout.Button("Add"))
        {
            ActionString actionString = new ActionString();

            switch(currentType)
            {
                case ActionCard.ActionType.Money:
                    {
                        actionString = new ActionString(ActionCard.ActionType.Money, new List<string>() { "Bank", "Player", "0" });
                        actionIsOpen.Add(false);
                    }
                    break;
            }

            card.actionStrings.Add(actionString);
            EditorUtility.SetDirty(target);
        }

        EditorGUILayout.EndHorizontal();
    }

    private void RecreateActions(ChanceCard card)
    {
        if (actionIsOpen.Count == 0)
        {
            foreach (ActionString actionString in card.actionStrings)
                actionIsOpen.Add(false);
        }

    }

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
            }
        }
    }

    #region Pokazywanie akcji

    private void ShowMoneyAction(ActionString moneyString)
    {
        MoneyAction.MoneyActor payer = (MoneyAction.MoneyActor)Enum.Parse(typeof(MoneyAction.MoneyActor),moneyString.variables[0]);
        moneyString.variables[0] = EditorGUILayout.EnumPopup("Payer", payer).ToString();

        MoneyAction.MoneyActor receiver = (MoneyAction.MoneyActor)Enum.Parse(typeof(MoneyAction.MoneyActor), moneyString.variables[1]);
        moneyString.variables[1] = EditorGUILayout.EnumPopup("Receiver", receiver).ToString();

        float amount = float.Parse(moneyString.variables[2]);
        moneyString.variables[2] = EditorGUILayout.FloatField("Amount", amount).ToString();

        EditorUtility.SetDirty(target);
    }

    #endregion Pokazywanie akcji 
}