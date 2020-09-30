﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "ChanceCard", menuName = "ARadom/ChanceCard")]
[Serializable]
public class ChanceCard : ScriptableObject
{
    [SerializeField]
    private string cardName;
    [SerializeField, TextArea]
    private string content;
    [SerializeField, Tooltip("Określa, czy karta może zostać użyta")]
    private bool active;
    [HideInInspector]
    public List<ActionString> actionStrings;

    /// <summary>
    /// Czy karta może zostać użyta
    /// </summary>
    /// <returns>Możliwość użycia karty</returns>
    public bool IsActive() => active;

    /// <summary>
    /// Wywołuje wszystkie akcje przypisane do karty
    /// </summary>
    /// <param name="caller"></param>
    public void CallActions(Player caller)
    {
        foreach (ActionString actionString in actionStrings)
        {
            ActionCard action = ActionCard.Create(actionString);
            action.Call(caller);
        }
    }

    /// <summary>
    /// Wyświetla graczowi okno z kartą
    /// </summary>
    /// <param name="caller">Gracz, dla którego otwierane jest okno z kartą</param>
    public void OpenCard(Player caller)
    {
        Debug.LogError("Zaimplementować wyświetlanie karty");
    }
}
