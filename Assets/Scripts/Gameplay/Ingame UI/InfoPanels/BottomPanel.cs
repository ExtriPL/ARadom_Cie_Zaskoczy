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

    private GameplayController gc;
    private BasePool buildingsPool;
    private List<BuildingListing> buildingListings;

    public void PreInit()
    {
        buildingListings = new List<BuildingListing>();
        buildingsPool = new BasePool(buildingsInfoHolder, buildingListing, Keys.Board.PLACE_COUNT / 2);
        buildingsPool.Init();
    }

    public void Init(UIPanels UIPanels)
    {
        this.UIPanels = UIPanels;
        gc = GameplayController.instance;
    }

    public void Init(UIPanels UIPanels, Player player)
    {
        Init(UIPanels);
        FillContent(player);        
    }

    private void FillContent(Player player) 
    {
        nickNameText.text = player.GetName();
        moneyText.text = player.Money.ToString();
        nickNameText.color = moneyText.color = player.MainColor;

        foreach(int placeId in player.GetOwnedPlaces())
        {
            Field field = gc.board.GetField(placeId);
            BuildingListing listing = buildingsPool.TakeObject().GetComponent<BuildingListing>();
            listing.Init(field, UIPanels);
            buildingListings.Add(listing);
        }

        if (player.NetworkPlayer == PhotonNetwork.LocalPlayer)
        {
            title.text = "Twoje budynki"; // do przetlumacznia
        }
        else 
        {
            title.text = "Budynki gracza " + player.GetName(); // do przetlumacznia
        }
    }

    public void DeInit() 
    {
        foreach(BuildingListing building in buildingListings)
        {
            building.DeInit();
            buildingsPool.ReturnObject(building.gameObject);
        }

        buildingListings.Clear();
    }
}
