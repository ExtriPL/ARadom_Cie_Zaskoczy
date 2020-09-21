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
    /// <param name="placeId">Obecny poziom budynku (liczony od 0)</param>
    /// <returns>Czy istnieje następny poziom budynku</returns>
    public bool HasNextTier(int placeId)
    {
        int currentTier = GameplayController.instance.board.GetTier(placeId);

        return currentTier >= 0 && currentTier < tiersCount - 1;
    }

    /// <summary>
    /// Pozwala określić, czy istnieje poprzedni poziom budynku
    /// </summary>
    /// <param name="placeId">Obecny poziom budynku (liczony od 0)</param>
    /// <returns>Czy istnieje poprzedni poziom budynku</returns>
    public bool HasPreviousTier(int placeId)
    {
        int currentTier = GameplayController.instance.board.GetTier(placeId);

        return currentTier <= tiersCount - 1 && currentTier > 0;
    }

    /// <summary>
    /// Zwraca właściwości danego poziomu budynku
    /// </summary>
    /// <param name="tier">Obecny poziom budynku (liczony od 0)</param>
    /// <returns>Właściowości podanego poziomu budynku</returns>
    public Tier GetTier(int tier)
    {
        if (tier >= 0 && tier <= tiersCount - 1)
        {
            return tiers[tier];
        }
        else
        {
            Debug.LogError("Poziom " + tier + " budynku o nazwie " + fieldName + " nie istnieje!");
            return tiers[0];
        }
    }

    public override GameObject GetStartModel()
    {
        return GetTier(0).model;
    }

    public override void OnBuy(Player player, PlaceVisualiser visualiser)
    {
        base.OnBuy(player, visualiser);

        visualiser.ShowNextModel();
        GameplayController.instance.board.NextTier(visualiser.placeIndex);
    }

    public override void OnEnter(Player player, PlaceVisualiser visualiser)
    {
        base.OnEnter(player, visualiser);

        if (player.NetworkPlayer.IsLocal)
        {
            if (GameplayController.instance.board.GetOwner(visualiser.placeIndex) != null)
            {
                //Pole ma właściciela

                if (GameplayController.instance.board.GetOwner(visualiser.placeIndex).GetName() == player.GetName())
                {
                    //Jeżeli gracz, który jest właścicielem stanął na polu
                    ShowUpgradePopup(player, visualiser, SettingsController.instance.languageController);
                }
                else
                {
                    //Jeżeli gracz, który nie jest właścicielem stanął na polu
                    ShowPayPopup(player, visualiser, GetTier(GameplayController.instance.board.GetTier(visualiser.placeIndex)).enterCost);
                }
            }
        }
    }

    public void OnUpgrade(Player player, PlaceVisualiser visualiser)
    {
        visualiser.ShowNextModel();
    }

    public override float GetInitialPrice() => GetTier(1).buyPrice;

    /// <summary>
    /// Pokazuje wskazanemu graczowi poppup z pytaniem, czy chce on ulepszyć dane pole
    /// </summary>
    /// <param name="player">Gracz, któremu pokazujemy popup</param>
    /// <param name="visualiser">Instancja visualisera, który przechowuje dane pole</param>
    /// <param name="language">Odwołanie do LanguageControllera</param>
    private void ShowUpgradePopup(Player player, PlaceVisualiser visualiser, LanguageController language)
    {
        //Tylko budynki o typie NormalBuilding można ulepszyć
        int currentTier = GameplayController.instance.board.GetTier(visualiser.placeIndex);

        //Pytamy o możliwość ulepszenia tylko wtedy, gdy istnieje następny poziom budynku
        if (HasNextTier(visualiser.placeIndex) && player.Money >= GameplayController.instance.banking.GetUpgradePrice(visualiser.placeIndex))
        {
            float upgradePrice = GetTier(currentTier + 1).buyPrice;

            //Sprawdzamy, czy gracz ma wystraczająco pieniędzy na ulepszenie
            if (player.Money >= upgradePrice)
            {
                string message = language.GetWord("DO_YOU_WANT_TO_UPGRADE") + GetFieldName() + "? \n" + language.GetWord("UPGRADE_COST") + upgradePrice;
                QuestionPopup upgrade = new QuestionPopup(message);

                Popup.PopupAction yesAction = delegate (Popup source)
                {
                    visualiser.onAnimationEnd += delegate { GameplayController.instance.EndTurn(); };
                    GameplayController.instance.banking.UpgradeBuilding(player, visualiser.placeIndex);
                    Popup.Functionality.Destroy(source).Invoke(source);
                };

                Popup.PopupAction noAction = delegate (Popup source)
                {
                    GameplayController.instance.EndTurn();
                    Popup.Functionality.Destroy(source).Invoke(source);
                };

                string yes = language.GetWord("YES");
                string no = language.GetWord("NO");

                upgrade.AddButton(no, noAction);
                upgrade.AddButton(yes, yesAction);

                PopupSystem.instance.AddPopup(upgrade);
            }
        }
        else
        {
            //Jeżeli nasz budynek nie ma następnego tieru, albo nie stać nas, by go ulepszyć
            GameplayController.instance.EndTurn();
        }
    }
}
