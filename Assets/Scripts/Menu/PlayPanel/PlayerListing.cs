﻿using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerListing : MonoBehaviourPunCallbacks, IEventSubscribable
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
        SubscribeEvents();
        kickButton.SetActive(!player.IsLocal && PhotonNetwork.LocalPlayer.IsMasterClient);
        readyButton.GetComponent<Toggle>().interactable = player.IsLocal;
        playerNickname.text = player.NickName;
        if (player.CustomProperties.ContainsKey("Room_PlayerReady")) readyButton.GetComponent<Toggle>().isOn = (bool)player.CustomProperties["Room_PlayerReady"];
        else readyButton.GetComponent<Toggle>().isOn = false;
    }

    public void Deinit()
    {
        player = null;
        pool.ReturnObject(gameObject);
        UnsubscribeEvents();
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        if (player == otherPlayer) Deinit();
    }

    public override void OnLeftRoom()
    {
        Deinit();
        PhotonNetwork.LeaveLobby();
        mainMenuController.OpenPanel(4);
    }

    public void KickPlayer() 
    {
        PhotonNetwork.CloseConnection(player);
    }

    public void PlayerReady(bool ready) 
    {
        Hashtable table = new Hashtable();
        table.Add("Room_PlayerReady", ready);
        player.SetCustomProperties(table);
        if (player.IsLocal) EventManager.instance.SendOnPlayerReady(player.NickName, ready);
    }

    public void OnPlayerReady(string playerName, bool ready)
    {
        if (player.NickName == playerName && !player.IsLocal) readyButton.GetComponent<Toggle>().isOn = ready;
    }

    public void SubscribeEvents()
    {
        EventManager.instance.onPlayerReady += OnPlayerReady;
    }

    public void UnsubscribeEvents()
    {
        EventManager.instance.onPlayerReady -= OnPlayerReady;
    }
}