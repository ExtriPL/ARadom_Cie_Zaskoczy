using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Tier
{
    /// <summary>
    /// Nazwa budynku w tej wersji
    /// </summary>
    [Tooltip("Nazwa budynku w tej wersji")]
    public string name;
    /// <summary>
    /// Ilość pieniędzy, jaką musimy zapłacić by kupić tę wersję budynku. Jeżeli mamy poprzednie wersje, jest to koszt ulepszenia
    /// </summary>
    [Tooltip("Ilość pieniędzy, jaką musimy zapłacić by kupić tę wersję budynku. Jeżeli mamy poprzednie wersje, jest to koszt ulepszeni")]
    public float buyPrice;
    /// <summary>
    /// Ilość pieniędzy, jaką zapłaci gracz, gdy stanie na danym polu
    /// </summary>
    [Tooltip("Ilość pieniędzy, jaką zapłaci gracz, gdy stanie na danym polu")]
    public float enterCost;
    /// <summary>
    /// Model danego budynku
    /// </summary>
    [Tooltip("Model danego budynku")]
    public GameObject model;
}

[CreateAssetMenu(fileName = "Normal Building", menuName = "ARadom/Field/Building/Normal Building")]
public class NormalBuilding : BuildingField
{
    /// <summary>
    /// Lista przechowująca dane na temat kolejnych stopni budynku, jego kolejnych wersji
    /// </summary>
    [SerializeField, Tooltip("Lista poziomów budynków")]
    public List<Tier> tiers = new List<Tier>();

    /// <summary>
    /// Liczba poziomów budynku
    /// </summary>
    public int tiersCount
    {
        get
        {
            return tiers.Count;
        }
    }

    /// <summary>
    /// Pozwala określić, czy istnieje następny poziom budynku
    /// </summary>
    /// <param name="currentTier">Obecny poziom budynku (liczony od 0)</param>
    /// <returns>Czy istnieje następny poziom budynku</returns>
    public bool HasNextTier(int currentTier)
    {
        return currentTier >= 0 && currentTier < tiersCount - 1;
    }

    /// <summary>
    /// Pozwala określić, czy istnieje poprzedni poziom budynku
    /// </summary>
    /// <param name="currentTier">Obecny poziom budynku (liczony od 0)</param>
    /// <returns>Czy istnieje poprzedni poziom budynku</returns>
    public bool HasPreviousTier(int currentTier)
    {
        return currentTier <= tiersCount - 1 && currentTier > 0;
    }

    /// <summary>
    /// Zwraca właściwości danego poziomu budynku
    /// </summary>
    /// <param name="currentTier">Obecny poziom budynku (liczony od 0)</param>
    /// <returns>Właściowości podanego poziomu budynku</returns>
    public Tier GetTier(int currentTier)
    {
        if (currentTier >= 0 && currentTier <= tiersCount - 1)
        {
            return tiers[currentTier];
        }
        else
        {
            Debug.LogError("Poziom " + currentTier + " budynku o nazwie " + fieldName + " nie istnieje!");
            return tiers[0];
        }
    }

    public override GameObject GetStartModel()
    {
        return GetTier(0).model;
    }

    public override void OnBuyBuilding(Player player, PlaceVisualiser visualiser)
    {
        throw new System.NotImplementedException();
    }

    public override void OnSellBuilding(Player player, PlaceVisualiser visualiser)
    {
        throw new System.NotImplementedException();
    }

    public override void OnPlayerEnter(Player player, PlaceVisualiser visualiser)
    {
        throw new System.NotImplementedException();
    }

    public override void OnPlayerLeave(Player player, PlaceVisualiser visualiser)
    {
        throw new System.NotImplementedException();
    }

    public override void OnPlayerPassby(Player player, PlaceVisualiser visualiser)
    {
        throw new System.NotImplementedException();
    }

    public override List<string> GetFieldInfo()
    {
        List<string> info = new List<string>
        {
            this.fieldName,
            GetTier(0).buyPrice.ToString(),
            GetTier(0).enterCost.ToString()
        };

        int index = GameplayController.instance.board.GetPlaceIndex(this);
        if (GameplayController.instance.board.GetOwner(index) != null) 
        {
            info.Add(GameplayController.instance.board.GetOwner(index).GetName());
        }
        else 
        {
            info.Add("Budynek nie ma właściciela!");
        }
        

        return info;
    }
}
