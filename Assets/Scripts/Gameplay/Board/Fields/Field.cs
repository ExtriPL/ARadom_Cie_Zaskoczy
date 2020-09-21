using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Field : ScriptableObject
{
    /// <summary>
    /// Nazwa pola
    /// </summary>
    [SerializeField, Tooltip("Nazwa pola")]
    protected string fieldName;
    /// <summary>
    /// Określa, czy dany budynek może znaleźć się na mapie
    /// </summary>
    [SerializeField, Tooltip("Czy dany budynek może zostać umieszczony na mapie")]
    protected bool canBePlaced = true;

    /// <summary>
    /// Zdarzenia wywoływane, gdy gracz wejdzie na pole
    /// </summary>
    /// <param name="player">Gracz, który wywołał zdarzenie</param>
    /// <param name="visualiser">Pole, na którym zostało wywołane zdarzenie</param>
    public virtual void OnPlayerEnter(Player player, PlaceVisualiser visualiser) { }

    /// <summary>
    /// Zdarzenia wywoływane, gdy gracz zejdzie z pola
    /// </summary>
    /// <param name="player">Gracz, który wywołał zdarzenie</param>
    /// <param name="visualiser">Pole, na którym zostało wywołane zdarzenie</param>
    public virtual void OnPlayerLeave(Player player, PlaceVisualiser visualiser) { }

    /// <summary>
    /// Zdarzenie wywoływane, gdy gracz przejdzie przez pole
    /// </summary>
    /// <param name="player">Gracz, który wywołał zdarzenie</param>
    /// <param name="visualiser">Pole, na którym zostało wywołane zdarzenie</param>
    public virtual void OnPlayerPassby(Player player, PlaceVisualiser visualiser) { }

    /// <summary>
    /// Funkcja zwraca model domyślny stojący na danym polu
    /// </summary>
    /// <returns>Model domyślny stojący na danym polu</returns>
    public abstract GameObject GetStartModel();

    /// <summary>
    /// Określa, czy dany budynek może znaleźć się na mapie
    /// </summary>
    public bool CanBePlaced() => canBePlaced;

    /// <summary>
    /// Zwraca nazwę pola
    /// </summary>
    /// <returns>Nazwa pola</returns>
    public string GetFieldName() => fieldName;
}
