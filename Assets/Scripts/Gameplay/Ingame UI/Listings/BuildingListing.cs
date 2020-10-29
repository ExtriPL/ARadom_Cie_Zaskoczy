using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingListing : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI fieldName;
    UIPanels UIPanels;
    GameplayController gc;
    private Coroutine highlight;

    private Field field;

    public void DeInit() {}

    public void Init(Field field, UIPanels UIPanels)
    {
        this.UIPanels = UIPanels;
        this.field = field;
        fieldName.text = field.GetFieldName();
        gc = GameplayController.instance;
    }

    public void PreInit() {}

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
}