using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BottomPanel : MonoBehaviour, IInitiable<UIPanels>
{
    private UIPanels UIPanels;
    public TextMeshProUGUI nickNameText;
    public TextMeshProUGUI moneyText;
    public GameObject buildingListing;
    [Tooltip("Element przechowujący wszystkie BuildingListingi")]
    public GameObject buildingsInfoHolder;
    public TextMeshProUGUI title;
    public Player player;

    public GameObject playersButton;
    public GameObject closeButton;
    public GameObject confirmSeletionButton;

    private GameplayController gc;
    private BasePool buildingsPool;
    private List<BuildingListing> buildingListings;
    private LanguageController lC;

    public void PreInit(UIPanels UIPanels)
    {
        this.UIPanels = UIPanels;
        buildingListings = new List<BuildingListing>();
        buildingsPool = new BasePool(buildingsInfoHolder, buildingListing, Keys.Board.PLACE_COUNT / 2);
        buildingsPool.Init();
        lC = SettingsController.instance.languageController;
    }

    public void Init()
    {
        gc = GameplayController.instance;
        closeButton.SetActive(true);
        confirmSeletionButton.SetActive(false);
    }

    public void Init(Player player, bool trading = false)
    {
        this.player = player;
        Init();
        FillContent(player, trading);
        closeButton.SetActive(!trading);
        confirmSeletionButton.SetActive(trading);
        playersButton.SetActive(!trading);
    }

    private void FillContent(Player player, bool trading) 
    {
        nickNameText.text = player.GetName();
        moneyText.text = player.Money.ToString();
        nickNameText.color = moneyText.color = player.MainColor;

        foreach(int placeId in player.GetOwnedPlaces())
        {
            Field field = gc.board.GetField(placeId);
            BuildingListing listing = buildingsPool.TakeObject().GetComponent<BuildingListing>();
            listing.Init(field, UIPanels, trading);
            buildingListings.Add(listing);
        }

        if (player.NetworkPlayer == PhotonNetwork.LocalPlayer)
        {
            title.text = lC.GetWord("YOUR_BUILDINGS");
        }
        else 
        {
            title.text = lC.GetWord("PLAYERS_BUILDINGS") == "Budynki gracza" ? lC.GetWord("PLAYERS_BUILDINGS") + " " + player.GetName() : player.GetName() + lC.GetWord("PLAYERS_BUILDINGS");
        }
    }

    public void DeInit() 
    {
        for (int i = buildingListings.Count - 1; i >= 0; i--)
        {
            BuildingListing building = buildingListings[i];
            building.DeInit();
            buildingsPool.ReturnObject(building.gameObject);
        }

        buildingListings.Clear();
    }

    public void ConfirmBuildingSelection() 
    {
        UIPanels.currentOpenPanel = UIPanels.InGameUIPanels.RightPanel;
        gameObject.GetComponent<Animation>().Play("MiddleToLeft");
        UIPanels.rightPanel.GetComponent<Animation>().Play("RightToMiddle");
    }
}
