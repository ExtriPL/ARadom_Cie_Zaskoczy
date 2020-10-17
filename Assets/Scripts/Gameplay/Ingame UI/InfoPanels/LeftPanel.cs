using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LeftPanel : MonoBehaviour, IInitiable<UIPanels>
{
    private BasePool basePool;
    public GameObject content;
    public GameObject template;
    private UIPanels UIPanels;
    private List<IngamePlayerListing> playerListings;


    public void PreInit()
    {
        playerListings = new List<IngamePlayerListing>();
        basePool = new BasePool(content, template, Keys.Menu.MAX_PLAYERS_COUNT);
        basePool.Init();
    }

    public void Init(UIPanels UIPanels)
    {
        this.UIPanels = UIPanels;
        foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
        {
            Player _player = GameplayController.instance.session.FindPlayer(player.NickName);
            IngamePlayerListing listing = basePool.TakeObject().GetComponent<IngamePlayerListing>();
            listing.Init(_player, UIPanels);
            playerListings.Add(listing);
        }
    }

    public void DeInit()
    {
        foreach (IngamePlayerListing player in playerListings)
        {
            player.DeInit();
            basePool.ReturnObject(player.gameObject);
        }

        playerListings.Clear();
    }
}
