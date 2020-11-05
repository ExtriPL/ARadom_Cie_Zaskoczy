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
    private GameplayController gc;


    public void PreInit()
    {
        playerListings = new List<IngamePlayerListing>();
        basePool = new BasePool(content, template, Keys.Menu.MAX_PLAYERS_COUNT);
        basePool.Init();
        gc = GameplayController.instance;
    }

    public void Init(UIPanels UIPanels)
    {
        this.UIPanels = UIPanels;
        foreach (string playerName in gc.session.playerOrder)
        {
            Player _player = GameplayController.instance.session.FindPlayer(playerName);
            IngamePlayerListing listing = basePool.TakeObject().GetComponent<IngamePlayerListing>();
            listing.Init(_player, UIPanels);
            playerListings.Add(listing);
        }
    }

    public void DeInit()
    {
        for (int i = playerListings.Count - 1; i >= 0; i--)
        {
            IngamePlayerListing player = playerListings[i];
            player.DeInit();
            basePool.ReturnObject(player.gameObject);
        }

        playerListings.Clear();
    }
}
