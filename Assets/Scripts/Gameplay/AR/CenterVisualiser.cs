using System.Collections.Generic;
using UnityEngine;

public class CenterVisualiser : Visualiser
{
    private Field currentField;

    public override void SubscribeEvents() {}

    public override void UnsubscribeEvents() {}

    public void Init()
    {
        ARController = GameplayController.instance.arController;
        InitModels();
    }

    protected override void InitModels()
    {
        for (int i = 0; i < Keys.Board.PLACE_COUNT; i++)
        {
            Field field = GameplayController.instance.board.GetField(i);
            GameObject model = Instantiate(field.GetModel(), transform);
            model.SetActive(false);
            models.Add(model);
        }
    }

    /// <summary>
    /// Wyświetla wybrany budynek jako budynek centralny
    /// </summary>
    /// <param name="field">Obeikt przechowujący informacje o polu do wyświetlenia</param>
    /// <param name="placeId">Numer pola na planszy, które chcemy wyświetlić</param>
    public void ShowField(Field field, int placeId)
    {
        currentField = field;
        ShowModel(placeId);
        ToggleVisibility(true);
    }

    public override void OnClick()
    {
        base.OnClick();

        ARController.buildingInfoPanel.GetComponent<BuildingInfoPanel>().FillBuildingInfo(currentField);
        ARController.buildingInfoPanel.GetComponent<BuildingInfoPanel>().Open();
    }
}
