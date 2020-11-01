using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class RoomPanel : MonoBehaviourPunCallbacks, IInitiable<MainMenuController>, IEventSubscribable
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

        //Ustawianie przycisku rozpoczęcia gry na nieaktywny dla graczy, którzy nie są masterem
        startGameButton.SetActive(PhotonNetwork.LocalPlayer.IsMasterClient);
        //Wyłączanie możliwości rozpoczęcia gry bez spełnienia wymagań co do  ilości i gotowości graczy
        if (PhotonNetwork.IsMasterClient) startGameButton.GetComponent<Button>().interactable = false;

        //ładowanie graczy
        foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
        {
            basePool.TakeObject().gameObject.GetComponent<PlayerListing>().Init(mainMenuController, player, basePool);
        }

        //animacja konca ladowania
        mainMenuController.EndLoadingScreen();
    }

    public void DeInit() {}

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) LeaveRoom();
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        basePool.TakeObject().gameObject.GetComponent<PlayerListing>().Init(mainMenuController, newPlayer, basePool);
        CheckPlayerReadyStatus();
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        //jeżeli wyjdzie master, gracz który zostaje nowym masterem zaczyna widzieć przycisk rozpoczęcia gry
        startGameButton.SetActive(PhotonNetwork.LocalPlayer.IsMasterClient);
        CheckPlayerReadyStatus();
    }

    public void StartGame()
    {
        startGameButton.GetComponent<Button>().interactable = false;
        mainMenuController.StartLoadingScreen();
        Hashtable table = new Hashtable();
        table.Add("LevelLoader_startMasterName", PhotonNetwork.MasterClient.NickName);
        PhotonNetwork.CurrentRoom.SetCustomProperties(table);
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;
        PhotonNetwork.LoadLevel(Keys.SceneNames.GAME);
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public void OnPlayerReady(string playerName, bool ready)
    {
        CheckPlayerReadyStatus();
    }

    private void CheckPlayerReadyStatus() 
    {
        float readyPlayerCount = 0f;
        foreach (Photon.Realtime.Player player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            if (player.CustomProperties.ContainsKey("Room_PlayerReady") && (bool)player.CustomProperties["Room_PlayerReady"]) readyPlayerCount++;
        }
        startGameButton.GetComponent<Button>().interactable = PhotonNetwork.CurrentRoom.PlayerCount >= Keys.Menu.MIN_PLAYERS_COUNT && readyPlayerCount >= PhotonNetwork.CurrentRoom.PlayerCount/2.0f;
    }

//#if UNITY_EDITOR || DEVELOPMENT_BUILD
//startGameButton.GetComponent<Button>().interactable = true;
//#endif

    public void SubscribeEvents()
    {
        EventManager.instance.onPlayerReady += null;
        EventManager.instance.onPlayerReady += OnPlayerReady;
    }

    public void UnsubscribeEvents()
    {
        EventManager.instance.onPlayerReady -= OnPlayerReady;
    }
}