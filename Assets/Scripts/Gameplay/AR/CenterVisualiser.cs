using System;
using System.Collections.Generic;
using UnityEngine;

public class CenterVisualiser : Visualiser
{
    private Field currentField;
    private Animator animationControler;

    public override void SubscribeEvents() {}

    public override void UnsubscribeEvents() {}

    /// <summary>
    /// Akcja zmiany budynku, gdy animacja ukrywania poprzedniego zakończy się
    /// </summary>
    private Action changeBuildingOnAnimmationEnd;

    public void Init()
    {
        ARController = GameplayController.instance.arController;
        InitModels();
        visible = false;

        animationControler = GetComponent<Animator>();
        animationControler.runtimeAnimatorController = ARController.centerBuildingAnimator;
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
        if (animatingShow || animatingHide)
            return;

        if (!visible)
        {
            currentField = field;
            showedModel = placeId;
            ToggleVisibility(true);
        }
        else
        {
            changeBuildingOnAnimmationEnd += delegate { ShowField(field, placeId); };
            ToggleVisibility(false);
        }
    }

    public override void ToggleVisibility(bool visible)
    {
        if (!visible)
            animationControler.SetTrigger("Hide");
        else
        {
            transform.localScale = Vector3.zero;
            base.ToggleVisibility(visible);
            animationControler.SetTrigger("Show");
        }
    }

    public override void OnCloseAnimationEnd()
    {
        base.OnCloseAnimationEnd();
        base.ToggleVisibility(false);

        if(changeBuildingOnAnimmationEnd != null)
        {
            changeBuildingOnAnimmationEnd.Invoke();
            changeBuildingOnAnimmationEnd = null;
        }

        animationControler.SetTrigger("EndRegular");
    }

    public override void OnShowAnimationStart()
    {
        base.OnShowAnimationStart();
        animationControler.SetTrigger("Regular");
    }

    public override void OnClick()
    {
        base.OnClick();
        if (visible)
        {
            ARController.buildingInfoPanel.GetComponent<BuildingInfoPanel>().FillBuildingInfo(currentField);
            ARController.buildingInfoPanel.GetComponent<BuildingInfoPanel>().Open();
        }
    }
}
