using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingListing : MonoBehaviour, IInitiable<Field>
{
    public Image icon;
    public TextMeshProUGUI fieldName;

    private Field field;

    public void DeInit() {}

    public void Init(Field field)
    {
        this.field = field;
        fieldName.text = field.GetFieldName();
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

    }
}