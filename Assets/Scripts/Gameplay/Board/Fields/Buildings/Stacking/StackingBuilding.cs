﻿using System.Collections;
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
    /// Ustawienia budynku typu stacking
    /// </summary>
    [SerializeField, Tooltip("Ustawienia budynków typu stacking")]
    protected StackingSettings stackingSettings;
    public override GameObject GetStartModel() => model;

    public override GameObject GetModel() => model;

    /// <summary>
    /// Zwraca cenę zakupu pola
    /// </summary>
    /// <returns>Cena zakupu pola</returns>
    public float BuyPrice { get => stackingSettings.BuyPrice; }

    /// <summary>
    /// Grupa budynków specjalnych, do których należy pole
    /// </summary>
    /// <returns>Grupa budynku specjalnego</returns>
    public StackingBuildingType StackingType { get => stackingSettings.StackingType; }

    public override float GetInitialPrice() => BuyPrice;

    /// <summary>
    /// Koszt wejścia na pole, jeżeli jest ono w posiadaniu jakiegoś gracza
    /// </summary>
    /// <param name="placeId">Numer pola na planszy</param>
    /// <returns></returns>
    public float GetEnterCost(int placeId)
    {
        int buildingCount = GameplayController.instance.board.CountPlacesOfType(placeId);
        return stackingSettings.GetEnterCost(buildingCount);
    }

    public override void OnBuy(Player player, PlaceVisualiser visualiser)
    {
        base.OnBuy(player, visualiser);

        //Jeżeli kupimy budynek, który należy do tej samej grupy budynków stackujących, wywołujemy funkcje OnSameGroupBuy
        if (visualiser.field is StackingBuilding && (visualiser.field as StackingBuilding).StackingType == StackingType) OnSameGroupBuy(player, visualiser);
    }

    public override void OnSell(Player player, PlaceVisualiser visualiser)
    {
        base.OnSell(player, visualiser);

        //Jeżeli sprzedamy budynek, który należy do tej samej grupy budynków stackujących, wywołujemy funkcje OnSameGroupSell
        if (visualiser.field is StackingBuilding && (visualiser.field as StackingBuilding).StackingType == StackingType) OnSameGroupBuy(player, visualiser);
    }

    /// <summary>
    /// Funkcjonalność wywoływana, gdy zostanie zakupiony ten sam budynek z grupy
    /// </summary>
    /// <param name="player">Gracz, który kupił pole</param>
    /// <param name="visualiser">Pole, na którym zostało wywołane zdarzenie</param>
    public virtual void OnSameGroupBuy(Player player, PlaceVisualiser visualiser) { }

    public virtual void OnSameGroupSell(Player player, PlaceVisualiser visualiser) { }

    public override void OnEnter(Player player, PlaceVisualiser visualiser)
    {
        base.OnEnter(player, visualiser);

        if(player.NetworkPlayer.IsLocal)
        {
            if (GameplayController.instance.board.GetOwner(visualiser.placeIndex) != null)
            {
                //Pole ma właściciela

                if (GameplayController.instance.board.GetOwner(visualiser.placeIndex).GetName() != player.GetName())
                {
                    //Jeżeli gracz, który nie jest właścicielem stanął na polu
                    ShowPayPopup(player, visualiser, GetEnterCost(visualiser.placeIndex));
                }
            }
        } 
    }
}
