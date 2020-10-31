using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingListing : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI fieldName;
    UIPanels UIPanels;
    GameplayController gc;
    public GameObject showOnBoardButton;
    public GameObject addToTradeButton;
    public Toggle tradeToggle;

    private Field field;

    public void DeInit() {}

    public void Init(Field field, UIPanels UIPanels, bool trade = false)
    {
        this.UIPanels = UIPanels;
        this.field = field;
        fieldName.text = field.GetFieldName();
        icon.sprite = field.FieldImage;
        gc = GameplayController.instance;
        showOnBoardButton.SetActive(!trade);
        addToTradeButton.SetActive(trade);

        if (trade) 
        {
            if (UIPanels.rightPanel.myListings.Find(listing => listing.tradeField == field) == null && UIPanels.rightPanel.theirListings.Find(listing => listing.tradeField == field) == null)
            {
                tradeToggle.isOn = false;

            }
            else
            {
                tradeToggle.isOn = true;
            }
        }
    }

    public void PreInit() {}

    #region Obsługa guzikow

    public void ShowInfo()
    {
        BuildingInfoPanel infoPanel = GameplayController.instance.arController.buildingInfoPanel.GetComponent<BuildingInfoPanel>();
        infoPanel.FillBuildingInfo(field);
        infoPanel.Open();
    }

    public void ShowOnBoard()
    {
        UIPanels.CloseBottomPanel();
        gc.arController.GetPlaceVisualiser(field).Highlight();
    }

    /// <summary>
    /// Dodaj/usun z listy budynkow
    /// true = dodaj
    /// false = usuń
    /// </summary>
    /// <param name="addRemove"></param>
    public void OnToggle(bool addRemove)
    {
        if (addRemove) AddToTradeList();
        else RemoveFromTradeList();
    }
    public void AddToTradeList() => UIPanels.rightPanel.AddToTradeList(UIPanels.bottomPanel.player, field);
    public void RemoveFromTradeList() => UIPanels.rightPanel.RemoveFromTradeList(UIPanels.bottomPanel.player, field);
    #endregion
}