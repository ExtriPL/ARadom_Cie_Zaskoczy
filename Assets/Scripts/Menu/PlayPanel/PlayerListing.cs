using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public TextMeshProUGUI readyText;
    private LanguageController lC;

    public void Init(MainMenuController mainMenuController, Photon.Realtime.Player player, BasePool pool)
    {
        this.mainMenuController = mainMenuController;
        lC = SettingsController.instance.languageController;
        this.player = player;
        this.pool = pool;
        SubscribeEvents();
        kickButton.SetActive(!player.IsLocal && PhotonNetwork.LocalPlayer.IsMasterClient);
        readyButton.GetComponent<Toggle>().interactable = player.IsLocal;

        int sameNamePlayers = PhotonNetwork.CurrentRoom.Players.Where(p => p.Value.NickName.Equals(PhotonNetwork.LocalPlayer.NickName)).Count();
        playerNickname.text = sameNamePlayers > 1 ? player.NickName + (sameNamePlayers - 1) : player.NickName;

        if (player.CustomProperties.ContainsKey("Room_PlayerReady"))
        {
            readyText.text = (bool)player.CustomProperties["Room_PlayerReady"] ? lC.GetWord("READY") : lC.GetWord("NOT_READY");
            readyButton.GetComponent<Toggle>().isOn = (bool)player.CustomProperties["Room_PlayerReady"];
        }
        else
        {
            readyButton.GetComponent<Toggle>().isOn = false;
            readyText.text = lC.GetWord("NOT_READY");
        }

        if (player.IsLocal)
        {
            readyButton.GetComponent<Toggle>().isOn = false;
            PlayerReady(false);
        }
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
        readyText.text = ready ? lC.GetWord("READY") : lC.GetWord("NOT_READY");
        if (player.IsLocal) EventManager.instance.SendOnPlayerReady(player.NickName, ready);
    }

    public void OnPlayerReady(string playerName, bool ready)
    {
        if (player.NickName == playerName && !player.IsLocal)
        {
            readyButton.GetComponent<Toggle>().isOn = ready;
            readyText.text = ready ? lC.GetWord("READY") : lC.GetWord("NOT_READY");
        }
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
