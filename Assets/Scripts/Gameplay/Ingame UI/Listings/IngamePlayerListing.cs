using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class IngamePlayerListing : MonoBehaviour
{

    public TextMeshProUGUI nickName;
    public TextMeshProUGUI money;
    public GameObject showBuildingsButton;
    private UIPanels UIPanels;
    private Player player;
    public GameObject tradeButton;

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
        GameplayController gC = GameplayController.instance;
        showBuildingsButton.SetActive(!player.NetworkPlayer.IsLocal);
        tradeButton.SetActive(!player.NetworkPlayer.IsLocal && gC.session.FindPlayer(gC.board.dice.currentPlayer).NetworkPlayer.IsLocal);

    }

    public void PreInit()
    {
    }

    public void OpenPlayerInfo() 
    {
        UIPanels.CloseLeftPanel(player);
    }

    public void OpenTrading() 
    {
        UIPanels.OpenRightPanel(player);
    }
}
