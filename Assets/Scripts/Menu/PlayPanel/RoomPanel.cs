using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class RoomPanel : MonoBehaviourPunCallbacks, IPanelInitable
{
    private BasePool basePool;
    public GameObject content;
    public GameObject template;
    public GameObject startGameButton;
    MainMenuController mainMenuController;

    public void PreInit()
    {
        basePool = new BasePool(content, template, Keys.Menu.MAX_PLAYERS_COUNT);
        basePool.Init();
    }

    public void Init(MainMenuController mainMenuController)
    {
        this.mainMenuController = mainMenuController;
        startGameButton.SetActive(PhotonNetwork.LocalPlayer.IsMasterClient);
        if (PhotonNetwork.IsMasterClient) startGameButton.GetComponent<Button>().interactable = PhotonNetwork.CurrentRoom.PlayerCount >= Keys.Menu.MIN_PLAYERS_COUNT;

        foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
        {
            basePool.TakeObject().gameObject.GetComponent<PlayerListing>().Init(mainMenuController, player, basePool);
        }
    }
    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        basePool.TakeObject().gameObject.GetComponent<PlayerListing>().Init(mainMenuController, newPlayer, basePool);
        startGameButton.GetComponent<Button>().interactable = PhotonNetwork.CurrentRoom.PlayerCount >= Keys.Menu.MIN_PLAYERS_COUNT && PhotonNetwork.IsMasterClient;
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        startGameButton.GetComponent<Button>().interactable = PhotonNetwork.CurrentRoom.PlayerCount >= Keys.Menu.MIN_PLAYERS_COUNT && PhotonNetwork.IsMasterClient;
    }

    public void StartGame()
    {
        Hashtable table = new Hashtable();
        table.Add("LevelLoader_startMasterName", PhotonNetwork.MasterClient.NickName);
        PhotonNetwork.CurrentRoom.SetCustomProperties(table);
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;
        PhotonNetwork.LoadLevel(Keys.SceneNames.GAME);
    }

    public void LeaveRoom()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            Debug.Log("PlayerCount == 1");
            PhotonNetwork.CurrentRoom.IsVisible = false;
        }
        PhotonNetwork.LeaveRoom();
    }
}