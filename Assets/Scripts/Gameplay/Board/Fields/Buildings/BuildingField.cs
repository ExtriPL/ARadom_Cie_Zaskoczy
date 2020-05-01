using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public abstract class BuildingField : Field
{
    /// <summary>
    /// Zdarzenie wywoływane przy zakupie pola
    /// </summary>
    /// <param name="player">Gracz, który kupił pole</param>
    /// <param name="visualiser">Pole, na którym zostało wywołane zdarzenie</param>
    public abstract void OnBuyBuilding(Player player, PlaceVisualiser visualiser);

    /// <summary>
    /// Zdarzenie wywoływane przy sprzedaniu pola
    /// </summary>
    /// <param name="player">Gracz, który sprzedał pole</param>
    /// <param name="visualiser">Pole, na którym zostało wywołane zdarzenie</param>
    public abstract void OnSellBuilding(Player player, PlaceVisualiser visualiser);
}