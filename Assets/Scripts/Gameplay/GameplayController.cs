using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

public class GameplayController : MonoBehaviour, IEventSubscribable
{
    public static GameplayController instance;

    public BankingController banking = new BankingController();
    public GameSession session = new GameSession();
    public GameMenu menu;
    public Board board = new Board();
    public ARController arController;
    public FlowController flow = new FlowController();
    public DiceController diceController;

    public GameObject GameSaveText;

    [SerializeField]
    private TutorialScreen tutorialScreen;
    [SerializeField]
    private GameObject mainScreenUI;

    /// <summary>
    /// Plik zapisu gry wczytany z pliku
    /// </summary>
    private GameSave save;

    private IEnumerator masterInactiveCheck;

    public CommandInvoker invoker;
    /// <summary>
    /// Flaga określająca czy gra przeszła przez komand invoker
    /// </summary>
    public bool GameInitialized { get; private set; }

    private int currentAutosave = 0;

    #region Inicjalizacja

    private void Start()
    {
        masterInactiveCheck = MasterInactiveCheck();
        StartCoroutine(masterInactiveCheck);
        GameInitialized = false;
        instance = this;
        if(!SettingsController.instance.Loaded)
        {
            SettingsController.instance.Init();
        }
    
        AddCommands();
        invoker.Start();
    }

    private void OnDisable()
    {
        session.UnsubscribeEvents();
        menu.UnsubscribeEvents();
        board.UnsubscribeEvents();
        banking.UnsubscribeEvents();
        arController.UnsubscribeEvents();
        flow.UnsubscribeEvents();
        UnsubscribeEvents();
    }

    private void Update()
    {
        invoker.Update();

        if (GameInitialized)
        {
            flow.Update();
            session.Update();
            board.Update();
            arController.UpdateAR();
        }
    }

    public void SubscribeEvents()
    {
        EventManager.instance.onTurnChanged += OnTurnChanged;
        EventManager.instance.onPlayerLostGame += OnPlayerLostGame;
        EventManager.instance.onGameStateChanged += OnGameStateChanged;
        EventManager.instance.onMessageArrival += OnMessageArrival;
    }

    public void UnsubscribeEvents()
    {
        EventManager.instance.onTurnChanged -= OnTurnChanged;
        EventManager.instance.onPlayerLostGame -= OnPlayerLostGame;
        EventManager.instance.onGameStateChanged -= OnGameStateChanged;
        EventManager.instance.onMessageArrival -= OnMessageArrival;
    }

    /// <summary>
    /// Dodaje komendy do startowego CommandInvokera
    /// </summary>
    private void AddCommands()
    {
        SyncCommand sync0 = new SyncCommand(0);
        SyncCommand sync1 = new SyncCommand(1);
        Command temporaryRoom = new TemporaryRoomCommand();
        Command subscribeEvents = new SubscribeEventsCommand(new List<IEventSubscribable>
        {
            instance,
            this.banking,
            this.session,
            menu,
            this.board,
            this.arController,
            this.flow,
            sync0,
            sync1
        });
        Command session = new SessionCommand(this.session);
        Command board = new BoardCommand(this.board);
        Command gameMenu = new GameMenuCommand(menu);
        Command banking = new BankingControllerCommand(this.banking);
        Command arController = new ARControllerCommand(this.arController);
        Command gameplayController = new GameplayControllerCommand(instance);
        Command loadFromSave = new LoadFromSaveCommand();
        Command popupSystem = new PopupSystemCommand();

        invoker = new CommandInvoker(null, null, delegate { OnExecutionFinished(); sync0.UnsubscribeEvents(); sync1.UnsubscribeEvents(); });

        invoker.AddCommand(temporaryRoom);
        invoker.AddCommand(subscribeEvents);
        invoker.AddCommand(session);
        invoker.AddCommand(board);
        invoker.AddCommand(gameMenu);
        invoker.AddCommand(banking);
        invoker.AddCommand(gameplayController);
        invoker.AddCommand(loadFromSave);
        invoker.AddCommand(sync0);
        invoker.AddCommand(arController);
        invoker.AddCommand(popupSystem);
        invoker.AddCommand(sync1);
    }

    #endregion Inicjalizacja
    
    #region Sterowanie rozgrywką

    /// <summary>
    /// Sprawdza, czy gracze są aktywni, wyrzuca nieaktywnych. Jeśli master jest nieaktywny niszczy grę.
    /// </summary>
    private IEnumerator InactiveCheck() 
    {
        while (true)
        {
            foreach (string playerName in session.playerOrder)
            {
                if (session.FindPlayer(playerName).NetworkPlayer.IsInactive)
                    session.KickPlayer(session.FindPlayer(playerName));
            }

            yield return new WaitForSeconds(Keys.Session.PLAYER_TTL);
        }
    }

    private IEnumerator MasterInactiveCheck() 
    {
        while (true)
        {
            if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("LevelLoader_startMasterName"))
            {
                if (PhotonNetwork.MasterClient.NickName != (string)PhotonNetwork.CurrentRoom.CustomProperties["LevelLoader_startMasterName"])
                {
                    PhotonNetwork.LeaveRoom();
                    PhotonNetwork.LoadLevel(Keys.SceneNames.MAIN_MENU);
                }
            }
            yield return new WaitForSeconds(Keys.Session.PLAYER_TTL);
        }
    }

    /// <summary>
    /// Zapisuje stan gry ze zmiennej save do pliku
    /// </summary>
    public void SaveToInstance()
    {
        save.applicationVersion = Application.version;
        save.date = DateTime.Now;
        save.roomName = PhotonNetwork.CurrentRoom.Name;

        FileManager.SaveGame(save, PhotonNetwork.CurrentRoom.Name);

        StartCoroutine(GameSavedText());
    }

    private IEnumerator GameSavedText()
    {
        GameSaveText.SetActive(true);
        yield return new WaitForSeconds(2f);
        GameSaveText.SetActive(false);
    }

    private void MakeAutosave()
    {
        save.applicationVersion = Application.version;
        save.date = DateTime.Now;
        save.roomName = PhotonNetwork.CurrentRoom.Name;

        FileManager.SaveGame(save, Keys.Gameplay.AUTOSAVE_NAME + " " + (currentAutosave + 1));

        currentAutosave++;
        if (currentAutosave == Keys.Gameplay.AUTOSAVE_COUNT)
            currentAutosave = 0;
    }

    /// <summary>
    /// Zapisuje obecny postęp rozgrywki do zmiennej lokalnej
    /// </summary>
    public void SaveProgress()
    {
        session.SaveProgress(ref save);
        board.SaveProgress(ref save);
    }

    /// <summary>
    /// Funkcja zaczynająca grę.
    /// </summary>
    private void StartGame()
    {
        StartCoroutine(InactiveCheck());
        StartCoroutine(TutorialTime());
    }

    private IEnumerator TutorialTime() 
    {
        int timer = 10;
        while (timer > 0)
        {
            yield return new WaitForSeconds(1);
            tutorialScreen.timer = --timer;
        }
        tutorialScreen.gameObject.SetActive(false);
        mainScreenUI.SetActive(true);
        flow.StartGame();
    }

    public void Imprison(Player player)
    {
        if (board.PlaceExists(typeof(PrisonSpecial)))
        {
            int placeIndex = board.GetPlaceIndex("Prison");
            board.TeleportPlayer(player, placeIndex);
            player.Imprisoned = true;
            EventManager.instance.SendOnPlayerImprisoned(player.GetName());
        }
        else
            Debug.LogError("Brak więzienia na planszy!");
    }

    public void Imprison(string playerName)
    {
        Player player = session.FindPlayer(playerName);
        Imprison(player);
    }

    #endregion Sterowanie rozgrywką

    #region Obsługa eventów

    /// <summary>
    /// Funkcja która realizuje event OnTurnChanged.
    /// Zapisuje postęp gry na samym początku tury
    /// </summary>
    /// <param name="previousPlayerName"></param>
    /// <param name="nextPlayerName"></param>
    private void OnTurnChanged(string previousPlayerName, string nextPlayerName)
    {
        SaveProgress(); //Na samym początku tury zapisujemy postęp rozgrywki
        MakeAutosave();
    }

    /// <summary>
    /// Event wywoływany gdy CommandInvoker skończy wykonywanie
    /// </summary>
    private void OnExecutionFinished()
    {
        Debug.Log("Koniec egzekucji");

        GameInitialized = true;
        StopCoroutine(masterInactiveCheck);
        StartGame();
    }

    /// <summary>
    /// Wyświetla stosowne komunikaty przy wowołaniu eventu przegranej gracza
    /// </summary>
    /// <param name="playerName">Gracz, który przegrał grę</param>
    private void OnPlayerLostGame(string playerName)
    {
        Player player = session.FindPlayer(playerName);
        LanguageController language = SettingsController.instance.languageController;

        //Gracz, który przegrał, jest graczem lokalnym
        if (player.NetworkPlayer.IsLocal)
        {
            string message = language.GetWord("YOU_LOST_THE_GAME");
            QuestionPopup playerLostGame = QuestionPopup.CreateOkDialog(message);

            PopupSystem.instance.AddPopup(playerLostGame);
        }
        else
        {
            string message = language.GetWord("PLAYER") + playerName + language.GetWord("LOST_THE_GAME");
            IconPopup playerLostGame = new IconPopup(IconPopupType.PlayerLost, message);

            PopupSystem.instance.AddPopup(playerLostGame);
        }
    }

    private void OnGameStateChanged(GameState previousState, GameState newState)
    {
        if(newState == GameState.ended)
        {
            LanguageController language = SettingsController.instance.languageController;

            Player winner = GetWinner();
            string message;
            if (winner != null)
            {
                if (winner.NetworkPlayer.IsLocal) message = language.GetWord("YOU_WON_GAME");
                else message = language.GetWord("GAME_ENDED") + " " + language.GetWord("WINNER_IS") + winner.GetName();
            }
            else message = language.GetWord("GAME_ENDED");

            QuestionPopup endGame = QuestionPopup.CreateOkDialog(message, delegate { menu.QuitGame(); } );
            PopupSystem.instance.AddPopup(endGame);
        }
    }

    private void OnMessageArrival(string[] message, IconPopupType iconType)
    {
        string unpackedMessage = "";
        LanguageController lang = SettingsController.instance.languageController;

        foreach (string word in message)
        {
            if (lang.IsPacked(word))
                unpackedMessage += lang.GetWord(lang.UnpackKey(word));
            else
                unpackedMessage += word;
        }

        IconPopup messagePopup = new IconPopup(iconType, unpackedMessage);

        PopupSystem.instance.AddPopup(messagePopup);
    }

    #endregion Obsługa eventów

    #region Warunki przegranej/wygranej  

    /// <summary>
    /// Zmienia status gracza na przegrany
    /// </summary> 
    public void LosePlayer(Player player)
    {
        player.IsLoser = true;

        //Odbieranie pól graczowi
        foreach (int placeId in player.GetOwnedPlaces())
        {
            if (board.GetField(placeId) is NormalBuilding)
                board.SetTier(placeId, 0);
        }

        player.ClearOwnership();
        player.PlaceId = -1;

        EventManager.instance.SendOnPlayerLostGame(player.GetName());
    }

    /// <summary>
    /// Zwraca instancję gracza, który wygrał grę.
    /// Jeżeli taki gracz nie istnieje, zwraca null.
    /// </summary>
    /// <returns>Instancja wygrywającego gracza</returns>
    public Player GetWinner()
    {
        //Warunek musi tutaj być, by w przypadku, gdy gra się dalej toczy, funkcja nie zwróciła instancji pierwszego dalej grającego gracza
        if (WinnerExists())
        {
            for(int i = 0; i < session.playerCount; i++)
            {
                Player player = session.FindPlayer(i);
                if (!player.IsLoser) return player;
            }
        }

        return null;
    }

    /// <summary>
    /// Sprawdza, czy istnieje zwycięzca rozgrywki
    /// </summary>
    /// <returns>Informacja, czy istnieje zwycięzca rozgrywki</returns>
    public bool WinnerExists()
    {
        //Licza graczy, którzy dalej grają
        int stillPlaying = 0;

        for (int i = 0; i < session.playerCount; i++)
        {
            Player player = session.FindPlayer(i);
            if (!player.IsLoser) stillPlaying++;
        }

        return stillPlaying <= 1; //Istnieje zwycięzca, gdy liczba grających osób jest równa 1. Gdy jest mniejsza od jednego gra się kończy ze zwycięzcą równym null
    }

    #endregion Warunki przegranej/wygranej
}
