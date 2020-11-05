using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class BuildingInfoPanel : MonoBehaviour
{

    public GameObject buildingName;
    public GameObject buildingInfo;
    public GameObject buildingHistory;
    public Image icon;
    private LanguageController lC;
    private UIPanels uIPanels;
    private UIPanels.InGameUIPanels tempPanelEnum;

    public void PreInit(UIPanels controller) 
    {
        uIPanels = controller;
    }


    private void Start()
    {
        lC = SettingsController.instance.languageController;
    }

    private void Update()
    {
    }
    public void FillBuildingInfo(Field field)
    {
        ClearBuildingInfo();
        icon.sprite = field.FieldImage;
        if (field is NormalBuilding normalbuilding)
        {
            int placeId = GameplayController.instance.board.GetPlaceIndex(normalbuilding);
            string type = lC.GetWord("NORMAL_BUILDING");
            string tier = lC.GetWord("LEVEL") + ": " + GameplayController.instance.board.GetTier(placeId).ToString();
            
            string prices = lC.GetWord("PRICE") + ": " + normalbuilding.tiers[1].buyPrice.ToString();
            if(GameplayController.instance.board.GetOwner(placeId) != null) prices += "<br>" + lC.GetWord("ENTER_COST") + ": " + normalbuilding.tiers[GameplayController.instance.board.GetTier(placeId)].enterCost.ToString();

            if (GameplayController.instance.board.GetOwner(placeId) != null && GameplayController.instance.board.GetOwner(placeId).NetworkPlayer.IsLocal)
            {
                if (GameplayController.instance.board.GetTier(placeId) != normalbuilding.tiersCount - 1) prices += "<br>" + lC.GetWord("UPGRADE_COST") +  normalbuilding.tiers[GameplayController.instance.board.GetTier(placeId) + 1].buyPrice.ToString();
                if (GameplayController.instance.board.GetTier(placeId) != normalbuilding.tiersCount - 1) prices += "<br>" + lC.GetWord("NEXT_TIER_ENTER_COST")+": " + normalbuilding.tiers[GameplayController.instance.board.GetTier(placeId) + 1].enterCost.ToString();
            }

            string owner1 = lC.GetWord("OWNER") + ": ";
            string owner2 = GameplayController.instance.board.GetOwner(placeId) != null ? (GameplayController.instance.board.GetOwner(placeId)).GetName() : "--";
            string buildingInfoText = type + "<br>" + tier + "<br>" + prices + "<br>" + owner1 + owner2;
            buildingName.GetComponent<TextMeshProUGUI>().text = normalbuilding.name;

            buildingInfo.GetComponent<TextMeshProUGUI>().text = buildingInfoText;
            buildingHistory.GetComponent<TextMeshProUGUI>().text = normalbuilding.FieldHistory;
        }
        else if (field is ChurchStacking churchStacking)
        {
            int placeId = GameplayController.instance.board.GetPlaceIndex(churchStacking);
            Player owner = GameplayController.instance.board.GetOwner(placeId);

            string type = lC.GetWord("CHURCH_STACKING");

            string prices = lC.GetWord("PRICE") + ": " + churchStacking.BuyPrice.ToString();
            if(owner != null) prices += "<br>" + lC.GetWord("ENTER_COST") + ": " + churchStacking.GetEnterCost(placeId).ToString();

            if (owner != null) prices += "<br>" + lC.GetWord("OWNED_BY_PLAYER") + " " + GameplayController.instance.board.CountPlacesOfType(owner, churchStacking.GetType());

            string owner1 = lC.GetWord("OWNER") + ": ";
            string owner2 = GameplayController.instance.board.GetOwner(placeId) != null ? (GameplayController.instance.board.GetOwner(placeId)).GetName() : "--";
            string buildingInfoText = type + "<br>" + prices + "<br>" + owner1 + owner2;
            buildingName.GetComponent<TextMeshProUGUI>().text = churchStacking.name;

            buildingInfo.GetComponent<TextMeshProUGUI>().text = buildingInfoText;
            buildingHistory.GetComponent<TextMeshProUGUI>().text = churchStacking.FieldHistory;
        }
        else if (field is StartSpecial startSpecial)
        {
            buildingName.GetComponent<TextMeshProUGUI>().text = startSpecial.name;
            buildingInfo.GetComponent<TextMeshProUGUI>().text = lC.GetWord("START_SPECIAL");
            buildingHistory.GetComponent<TextMeshProUGUI>().text = startSpecial.FieldHistory;
        }
        else if (field is PrisonSpecial prisonSpecial)
        {
            buildingName.GetComponent<TextMeshProUGUI>().text = prisonSpecial.name;
            buildingInfo.GetComponent<TextMeshProUGUI>().text = lC.GetWord("PRISON_SPECIAL");
            buildingHistory.GetComponent<TextMeshProUGUI>().text = prisonSpecial.FieldHistory;
        }
        else if (field is ChanceSpecial chanceSpecial) 
        {
            buildingName.GetComponent<TextMeshProUGUI>().text = chanceSpecial.name;
            buildingInfo.GetComponent<TextMeshProUGUI>().text = lC.GetWord("CHANCE_SPECIAL");
            buildingHistory.GetComponent<TextMeshProUGUI>().text = chanceSpecial.FieldHistory;
        }
    }

    public void Open()
    {
        tempPanelEnum = uIPanels.currentOpenPanel;
        uIPanels.currentOpenPanel = UIPanels.InGameUIPanels.BuildingInfoPanel;
        if (!gameObject.GetComponent<Animation>().isPlaying)
        {

            gameObject.GetComponent<Animation>().Play("LeftToMiddle");
        }
    }

    public void Close() 
    {
        uIPanels.currentOpenPanel = tempPanelEnum;
        if (!gameObject.GetComponent<Animation>().isPlaying)
        {
            gameObject.GetComponent<Animation>().Play("MiddleToLeft");
        }
    }

    public void ClearBuildingInfo() 
    {
        buildingName.GetComponent<TextMeshProUGUI>().text = "";
        buildingInfo.GetComponent<TextMeshProUGUI>().text = "";
        buildingHistory.GetComponent<TextMeshProUGUI>().text = "";
    }
}
