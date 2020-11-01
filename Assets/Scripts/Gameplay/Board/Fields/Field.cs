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

    [SerializeField, Tooltip("Opis pola"), TextArea]
    protected string fieldHistory;

    [SerializeField, Tooltip("Zdjęcie budynku")]
    protected Sprite fieldImage;

    /// <summary>
    /// Zdarzenia wywoływane, gdy gracz wejdzie na pole
    /// </summary>
    /// <param name="player">Gracz, który wywołał zdarzenie</param>
    /// <param name="visualiser">Pole, na którym zostało wywołane zdarzenie</param>
    public virtual void OnEnter(Player player, PlaceVisualiser visualiser)
    {
        if(player.NetworkPlayer.IsLocal && this is BuildingField)
            GameplayController.instance.arController.centerBuilding.GetComponent<CenterVisualiser>().ShowField(this, visualiser.placeIndex);
    }

    /// <summary>
    /// Zdarzenia wywoływane, gdy gracz zejdzie z pola
    /// </summary>
    /// <param name="player">Gracz, który wywołał zdarzenie</param>
    /// <param name="visualiser">Pole, na którym zostało wywołane zdarzenie</param>
    public virtual void OnLeave(Player player, PlaceVisualiser visualiser) { }

    /// <summary>
    /// Zdarzenie wywoływane, gdy gracz przejdzie przez pole
    /// </summary>
    /// <param name="player">Gracz, który wywołał zdarzenie</param>
    /// <param name="visualiser">Pole, na którym zostało wywołane zdarzenie</param>
    public virtual void OnPassby(Player player, PlaceVisualiser visualiser) { }

    /// <summary>
    /// Zdarzenie wywoływane gdy gracz rozpocznie rundę na danym polu
    /// </summary>
    /// <param name="player">Instancja gracza</param>
    /// <param name="visualiser">Instancja pola</param>
    public virtual void OnAwake(Player player, PlaceVisualiser visualiser) 
    {
        GameplayController.instance.flow.DefaultBegining();
    }

    /// <summary>
    /// Zdarzenie wywoływane gdy gracz zakończy rundę na danym polu
    /// </summary>
    /// <param name="player">Instancja gracza</param>
    /// <param name="visualiser">Instancja pola</param>
    public virtual void OnEnd(Player player, PlaceVisualiser visualiser) {}

    /// <summary>
    /// Funkcja zwraca model domyślny stojący na danym polu
    /// </summary>
    /// <returns>Model domyślny stojący na danym polu</returns>
    public abstract GameObject GetStartModel();

    /// <summary>
    /// Zwraca model budynku, który stoi na danym polu.
    /// W przypadku gdy jest więcej niż 1 możliwy model, zwraca ostatni dostępny
    /// </summary>
    public abstract GameObject GetModel();

    /// <summary>
    /// Określa, czy dany budynek może znaleźć się na mapie
    /// </summary>
    public bool CanBePlaced() => canBePlaced;

    /// <summary>
    /// Zwraca nazwę pola
    /// </summary>
    /// <returns>Nazwa pola</returns>
    public string GetFieldName() => fieldName;


    /// <summary>
    /// Historia budynku, który stoi na danym polu
    /// </summary>
    /// <returns>Historia budynku</returns>
    public string FieldHistory { get => fieldHistory; }

    /// <summary>
    /// Ikona pola
    /// </summary>
    /// <returns>Ikona pola</returns>
    public Sprite FieldImage { get => fieldImage; }
}
