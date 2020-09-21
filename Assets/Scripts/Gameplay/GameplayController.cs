using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;
using System;
using Photon.Realtime;
using Hashtable =  ExitGames.Client.Photon.Hashtable;

public class GameplayController : MonoBehaviour, IEventSubscribable
{
    public static GameplayController instance;

    public BankingController banking = new BankingController();
    public GameSession session = new GameSession();
    public GameMenu menu;
    public Board board = new Board();
    public ARController arController;

    /// <summary>
    /// Plik zapisu gry wczytany z pliku
    /// </summary>
    private GameSave save;

    private IEnumerator masterInactiveCheck;

    private CommandInvoker invoker;
    /// <summary>
    /// Flaga określająca czy gra przeszła przez komand invoker
    /// </summary>
    private bool gameInitialized;
    /// <summary>
    /// Flaga określająca, czy popup mówiący o skońceniu tury ma się pokazać graczowi
    /// </summary>
    public bool showEndOfTurnPopup
    { 
        get
        {
            return (bool)PhotonNetwork.CurrentRoom.CustomProperties["gameplayController_showEndOfTurnPopup"];
        }
        set
        {
            Hashtable table = new Hashtable();
            table.Add("gameplayController_showEndOfTurnPopup", value);
            PhotonNetwork.CurrentRoom.SetCustomProperties(table);
        }
    }

    #region Inicjalizacja

    private void Start()
    {
        masterInactiveCheck = MasterInactiveCheck();
        StartCoroutine(masterInactiveCheck);
        gameInitialized = false;
        instance = this;

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
        UnsubscribeEvents();
    }

    private void Update()
    {
        invoker.Update();

        if (gameInitialized)
        {
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
    }

    public void UnsubscribeEvents()
    {
        EventManager.instance.onTurnChanged -= OnTurnChanged;
        EventManager.instance.onPlayerLostGame -= OnPlayerLostGame;
        EventManager.instance.onGameStateChanged -= OnGameStateChanged;
    }

    /// <summary>
    /// Dodaje komendy do startowego CommandInvokera
    /// </summary>
    private void AddCommands()
    {
        SyncCommand sync = new SyncCommand();
        Command temporaryRoom = new TemporaryRoomCommand();
        Command subscribeEvents = new SubscribeEventsCommand(new List<IEventSubscribable>
        {
            instance,
            this.banking,
            this.session,
            menu,
            this.board,
            this.arController,
            sync
        });
        Command session = new SessionCommand(this.session);
        Command board = new BoardCommand(this.board);
        Command gameMenu = new GameMenuCommand(menu);
        Command banking = new BankingControllerCommand(this.banking);
        Command arController = new ARControllerCommand(this.arController);
        Command gameplayController = new GameplayControllerCommand(instance);
        Command loadFromSave = new LoadFromSaveCommand();
        Command popupSystem = new PopupSystemCommand();

        invoker = new CommandInvoker(null, null, delegate { OnExecutionFinished(); sync.UnsubscribeEvents(); });

        invoker.AddCommand(temporaryRoom);
        invoker.AddCommand(subscribeEvents);
        invoker.AddCommand(session);
        invoker.AddCommand(board);
        invoker.AddCommand(gameMenu);
        invoker.AddCommand(banking);
        invoker.AddCommand(gameplayController);
        invoker.AddCommand(loadFromSave);
        invoker.AddCommand(sync);
        invoker.AddCommand(arController);
        invoker.AddCommand(popupSystem);
        invoker.AddCommand(sync);
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
                if (session.FindPlayer(playerName).NetworkPlayer.IsInactive) session.KickPlayer(session.FindPlayer(playerName));
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

        FileManager.SaveGame(save, PhotonNetwork.CurrentRoom.Name);
        MessageSystem.instance.AddMessage("<color=green>Gra zostałą zapisana</color>", MessageType.MediumMessage);
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
        if (session.roomOwner.IsLocal) EventManager.instance.SendOnTurnChanged("", board.dice.currentPlayer);
    }

    /// <summary>
    /// Kończy turę obecnie aktywnego gracza i rozpoczyna kolejną.
    /// Przed przekazaniem tury sprawdza warunki przegrania i wygrania
    /// </summary>
    public void EndTurn()
    {
        if (session.localPlayer.Money < 0f) 
        {
            //Przegrana gracza przez bankructwo

            //Jeżeli gracz może zaciągnąć pożyczkę
            if(banking.CanTakeLoan(session.localPlayer))
            {
                //Danie graczowi szansy na zaciągnięcie pożyczki, by mógł ocalić się przed bankructwem
                ShowLastChanceLoanMessage();
            }
            else
            {
                //Jeżeli dojdzie do tego miejsca, gracz nie ma już żadnych szans na ratunek i przegrywa
                LosePlayer();
                CheckWin();
            }
        }
        else NextTurn();
    }

    /// <summary>
    /// Zmienia turę na nestępną.
    /// </summary>
    private void NextTurn()
    {
        string previousPlayer = board.dice.currentPlayer;
        board.dice.NextTurn();
        board.dice.RollDice();
        string nextPlayer = board.dice.currentPlayer;

        EventManager.instance.SendOnTurnChanged(previousPlayer, nextPlayer);
    }

    #endregion Sterowanie rozgrywką

    #region Obsługa eventów

    /// <summary>
    /// Funkcja która realizuje event OnTurnChanged.
    /// Przekazuje ture kolejnemu graczowi, daje mu rzut kostką oraz przemieszcza gracza.
    /// </summary>
    /// <param name="previousPlayerName"></param>
    /// <param name="nextPlayerName"></param>
    private void OnTurnChanged(string previousPlayerName, string nextPlayerName)
    {
        SaveProgress(); //Na samym początku tury zapisujemy postęp rozgrywki
    }

    /// <summary>
    /// Event wywoływany gdy CommandInvoker skończy wykonywanie
    /// </summary>
    private void OnExecutionFinished()
    {
        Debug.Log("Koniec egzekucji");

        gameInitialized = true;
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
            QuestionPopup playerLostGame = QuestionPopup.CreateOkDialog(message, Popup.Functionality.Destroy());

            PopupSystem.instance.AddPopup(playerLostGame);
        }
        else
        {
            string message = language.GetWord("PLAYER") + playerName + language.GetWord("LOST_THE_GAME");
            InfoPopup playerLostGame = new InfoPopup(message, 1.5f);

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

    #endregion Obsługa eventów

    #region Warunki przegranej/wygranej  

    /// <summary>
    /// Pokazuje popup, mówiący o bankructwie i dający szanse wzięcia pożyczki pozwalającej na dalszą grę
    /// </summary>
    private void ShowLastChanceLoanMessage()
    {
        LanguageController language = SettingsController.instance.languageController;
        string message = language.GetWord("LAST_CHANCE_WANT_TO_TAKE_LOAN");

        Popup.PopupAction yesAction = delegate (Popup source)
        {
            banking.TakeLoan(session.localPlayer);
            EndTurn();
            Popup.Functionality.Destroy().Invoke(source);
        };
        Popup.PopupAction noAction = delegate (Popup source)
        {
            LosePlayer();
            CheckWin();
            Popup.Functionality.Destroy().Invoke(source);
        };

        QuestionPopup lastChange = QuestionPopup.CreateYesNoDialog(message, yesAction, noAction);
        PopupSystem.instance.AddPopup(lastChange);
    }

    /// <summary>
    /// Zmienia status gracza na przegrany
    /// </summary> 
    private void LosePlayer()
    {
        session.localPlayer.IsLoser = true;

        //Odbieranie pól graczowi
        foreach (int placeId in session.localPlayer.GetOwnedFields())
        {
            if (board.GetField(placeId) is NormalBuilding)
                board.SetTier(placeId, 0);
        }

        session.localPlayer.ClearOwnership();
        session.localPlayer.PlaceId = -1;

        EventManager.instance.SendOnPlayerLostGame(session.localPlayer.GetName());
    }

    /// <summary>
    /// Sprawdza, czy istnieje zwycięzca gry. Jeżeli tak jest wyświetla komunikat. Jeżeli nie, rozpoczyna kolejną turę
    /// </summary>
    private void CheckWin()
    {
        if (WinnerExists())
        {
            //Informacja o wygranej jakiegoś gracza
            //Zakończenie rozgrywki
            session.gameState = GameState.ended;
        }
        else NextTurn();
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
