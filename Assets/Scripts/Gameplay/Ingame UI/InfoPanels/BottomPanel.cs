using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BottomPanel : MonoBehaviour
{
    private UIPanels UIPanels;
    public TextMeshProUGUI nickNameText;
    public TextMeshProUGUI moneyText;
    private GameplayController gc;

    public void Init(UIPanels UIPanels, Photon.Realtime.Player player)
    {
        this.UIPanels = UIPanels;
        gc = GameplayController.instance;
        FillContent(player);        
    }

    private void FillContent(Photon.Realtime.Player player) 
    {
        nickNameText.text = player.NickName;
        moneyText.text = gc.session.FindPlayer(player.NickName).Money.ToString();
        nickNameText.color = moneyText.color = gc.session.FindPlayer(player.NickName).MainColor;
    }

    public void Deinit() 
    {
    }
}
