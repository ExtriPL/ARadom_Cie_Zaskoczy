using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerListing : MonoBehaviourPunCallbacks
{
    private Photon.Realtime.Player player;
    private BasePool pool;
    public GameObject readyButton;
    public GameObject kickButton;
    MainMenuController mainMenuController;
    public TextMeshProUGUI playerNickname;
    public void Init(MainMenuController mainMenuController, Photon.Realtime.Player player, BasePool pool)
    {
        this.mainMenuController = mainMenuController;
        this.player = player;
        this.pool = pool;
        kickButton.SetActive(PhotonNetwork.LocalPlayer.IsMasterClient);
        readyButton.SetActive(player.IsLocal);
        readyButton.GetComponent<Toggle>().interactable = player.IsLocal;
        playerNickname.text = player.NickName;
    }

    public void Deinit()
    {
        player = null;
        pool.ReturnObject(gameObject);
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        if (player == otherPlayer) Deinit();
    }

    public override void OnLeftRoom()
    {
        Deinit();
        mainMenuController.OpenPanel(4);
    }
}
