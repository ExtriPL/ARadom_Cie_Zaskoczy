using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RightPanel : MonoBehaviour
{
    private BasePool basePool;
    public GameObject content;
    public GameObject template;
    private UIPanels UIPanels;
    private List<TradeListing> tradeListings;
    public void PreInit()
    {
        tradeListings = new List<TradeListing>();
        basePool = new BasePool(content, template, Keys.Menu.MAX_PLAYERS_COUNT);
        basePool.Init();
    }


    public void Init(UIPanels controller, Player player)
    {
        Debug.Log(player.GetName());
    }

    public void DeInit()
    {
        
    }
}
