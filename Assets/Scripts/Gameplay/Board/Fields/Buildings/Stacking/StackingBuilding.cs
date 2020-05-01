using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StackingBuilding : BuildingField
{
    /// <summary>
    /// Model budynku stojącego na polu
    /// </summary>
    [SerializeField, Tooltip("Model budynku stojącego na polu")]
    private GameObject model;
    /// <summary>
    /// Cena zakupu pola
    /// </summary>
    [SerializeField, Tooltip("Cena zakupu pola")]
    private float buyPrice;
    /// <summary>
    /// Ilość pieniędzy, jaką zapłaci gracz za wejście na pole
    /// </summary>
    [SerializeField, Tooltip("Ilość pieniędzy, jaką zapłaci gracz za wejście na pole")]
    private float enterCost;
    /// <summary>
    /// O ile zwiększa się koszt wejścia na pole, przy posiadaniu tego budynku w grupie budynków typu Stacking
    /// </summary>
    [SerializeField, Tooltip("O ile zwiększa się koszt wejścia na pole, przy posiadaniu tego budynku w grupie budynków typu Stacking")]
    private float stackingEnterCost;
    /// <summary>
    /// Grupa budynków specjalnych, do których należy pole
    /// </summary>
    protected StackingBuildingType stackingType;

    public override GameObject GetStartModel() => model;

    /// <summary>
    /// Zwraca cenę zakupu pola
    /// </summary>
    /// <returns>Cena zakupu pola</returns>
    public float GetBuyPrice() => buyPrice;

    /// <summary>
    /// Podstawowy koszt wejścia na pole, jeżeli jest ono w posiadaniu jakiegoś gracza
    /// </summary>
    /// <returns>Podstawowy koszt wejścia na pole</returns>
    public float GetEnterCost() => enterCost;

    /// <summary>
    /// O ile zwiększa się koszt wejścia na pole, przy posiadaniu tego budynku w grupie budynków typu Stacking
    /// </summary>
    /// <returns>Zwiększenie kosztu wejścia na pola w grupie</returns>
    public float GetStackingEnterCost() => stackingEnterCost;

    /// <summary>
    /// Grupa budynków specjalnych, do których należy pole
    /// </summary>
    /// <returns>Grupa budynku specjalnego</returns>
    public StackingBuildingType GetStackingType() => stackingType;

    public override void OnBuyBuilding(Player player, PlaceVisualiser visualiser)
    {
        //Jeżeli kupimy budynek, który należy do tej samej grupy budynków stackujących, wywołujemy funkcje OnSameGroupBuy
        if (visualiser.field is StackingBuilding && (visualiser.field as StackingBuilding).GetStackingType() == GetStackingType()) OnSameGroupBuy(player, visualiser);
    }

    public override void OnSellBuilding(Player player, PlaceVisualiser visualiser)
    {
        //Jeżeli sprzedamy budynek, który należy do tej samej grupy budynków stackujących, wywołujemy funkcje OnSameGroupSell
        if (visualiser.field is StackingBuilding && (visualiser.field as StackingBuilding).GetStackingType() == GetStackingType()) OnSameGroupBuy(player, visualiser);
    }

    /// <summary>
    /// Funkcjonalność wywoływana, gdy zostanie zakupiony ten sam budynek z grupy
    /// </summary>
    /// <param name="player">Gracz, który kupił pole</param>
    /// <param name="visualiser">Pole, na którym zostało wywołane zdarzenie</param>
    public abstract void OnSameGroupBuy(Player player, PlaceVisualiser visualiser);

    public abstract void OnSameGroupSell(Player player, PlaceVisualiser visualiser);
}
