using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;
using System;
using Photon.Realtime;

public class GameplayController : MonoBehaviour
{
    public static GameplayController instance;

    public GameSession session = new GameSession();
    public GameMenu menu;
    public Board board = new Board();
    public ARController arController;

    /// <summary>
    /// Plik zapisu gry wczytany z pliku
    /// </summary>
    [HideInInspector] public GameSave save;
    /// <summary>
    /// Określa, czy gra zostałą załadowana z pliku
    /// </summary>
    public bool loadedFromSave { get; private set; }

    private void OnEnable()
    {
        if (!instance) instance = this;
        else Destroy(this);

        session.SubscribeEvents();
        menu.SubscribeEvents();
        board.SubscribeEvents();
        /*
         * Rozwiązać wychodzenie z aplikacji eventami pausy i focusu
         * Jeżeli gracz wyjdzie z gry jest jednocześnie wyrzucany z listy graczy
        */
    }

    private void OnDisable()
    {
        session.UnsubscribeEvents();
        menu.UnsubscribeEvents();
        board.UnsubscribeEvents();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            Init();

            session.Init(instance);
            menu.Init(instance);
            board.Init(instance);
            arController.InitBoard();
        }
        else if (Application.isEditor) StartCoroutine(CreateTemporaryRoom());
        else Debug.LogError("Nie udało połączyć się z serwerem. Prawdopodobnie gra została uruchomiona ze złej sceny");
    }

    /// <summary>
    /// Inicjalizacja rozgrywki
    /// </summary>
    private void Init()
    {
        loadedFromSave = false;
        save = new GameSave();

        if (session.roomOwner.IsLocal)
        {
            //Jeżeli jest ustawiona odpowiednia zmienna na true, stan gry ładowany jest z pliku
            loadedFromSave = PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("loadSavedGame") && (bool)PhotonNetwork.CurrentRoom.CustomProperties["loadSavedGame"];

            if (loadedFromSave)
            {
                string fileName = (string)PhotonNetwork.CurrentRoom.CustomProperties["saveFileName"];
                FileManager.LoadGame(ref save, fileName);
            }
        }
    }

    public void SaveToInstance()
    {
        save.applicationVersion = Application.version;
        session.SaveToInstance(ref save);
        board.SaveToInstance(ref save);

        FileManager.SaveGame(save, PhotonNetwork.CurrentRoom.Name);
        MessageSystem.instance.AddMessage("<color=green>Gra zostałą zapisana</color>", MessageType.MediumMessage);
    }

    /// <summary>
    /// Tworzenie tymczasowego pokoju umożliwiającego uruchomienie gry zaczynając od sceny Game
    /// </summary>
    private IEnumerator CreateTemporaryRoom()
    {
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.GameVersion = Application.version;
        yield return new WaitUntil(() => PhotonNetwork.NetworkClientState == ClientState.ConnectedToMasterServer);

        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.NickName = Keys.DefaultRoom.NICKNAME;
        PhotonNetwork.JoinLobby();

        yield return new WaitUntil(() => PhotonNetwork.NetworkClientState == ClientState.JoinedLobby);

        RoomOptions roomOptions = new RoomOptions() { IsVisible = false, IsOpen = false, MaxPlayers = 1 };
        PhotonNetwork.CreateRoom(Keys.DefaultRoom.ROOM_NAME, roomOptions);
        yield return new WaitUntil(() => PhotonNetwork.NetworkClientState == ClientState.Joined);

        Start();
        Debug.LogWarning("Uruchomiono grę na awaryjnym pokoju");
    }
}
