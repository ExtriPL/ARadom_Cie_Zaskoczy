using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class IngamePlayerListing : MonoBehaviour
{

    public TextMeshProUGUI nickName;
    public TextMeshProUGUI money;
    public GameObject button;
    private UIPanels UIPanels;
    private Player player;
    public void DeInit()
    {
        player = null;
        nickName.text = "";
        money.text = "";
        nickName.color = money.color = Color.white;
    }

    public void Init(Player player, UIPanels UIPanels)
    {
        this.player = player;
        this.UIPanels = UIPanels;
        nickName.text = player.GetName();
        money.text = player.Money.ToString();
        nickName.color = money.color = player.MainColor;
        //if (player.NetworkPlayer == PhotonNetwork.LocalPlayer) 
        //{
        //    button.SetActive(false);
        //}
    }

    public void PreInit()
    {
        //throw new System.NotImplementedException();
    }

    public void OpenPlayerInfo() 
    {
        UIPanels.CloseLeftPanel(player);
    }
}
