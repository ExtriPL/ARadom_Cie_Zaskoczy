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
    }

    public void UnsubscribeEvents()
    {
        EventManager.instance.onTurnChanged -= OnTurnChanged;
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
            }
            else
            {
                //Jeżeli dojdzie do tego miejsca, gracz nie ma już żadnych szans na ratunek i przegrywa
                EventManager.instance.SendOnPlayerLostGame(session.localPlayer.GetName());
                session.localPlayer.IsLoser = true;

                if (WinnerExists())
                {
                    //Informacja o wygranej jakiegoś gracza
                    //Zakończenie rozgrywki
                }
                else NextTurn();
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

        if (session.FindPlayer(previousPlayerName) != null && session.FindPlayer(previousPlayerName).NetworkPlayer.IsLocal)
        {
            QuestionPopup endTurn = new QuestionPopup(SettingsController.instance.languageController.GetWord("TURN_ENDED"));
            endTurn.AddButton("Ok", Popup.Functionality.Destroy(endTurn));
            PopupSystem.instance.AddPopup(endTurn);
        }
        if (session.FindPlayer(nextPlayerName).NetworkPlayer.IsLocal)
        {
            QuestionPopup startTurn = new QuestionPopup(SettingsController.instance.languageController.GetWord("TURN_STARTED"));
            startTurn.AddButton("Ok", Popup.Functionality.Destroy(startTurn));

            IconPopup dice = new IconPopup(IconPopupType.None);
            dice.onClick += Popup.Functionality.Destroy(dice);
            Popup.PopupAction rolldice = delegate (Popup source)
            {
                int firstThrow = board.dice.last1;
                int secondThrow = board.dice.last2;
                InfoPopup rollResult = new InfoPopup(SettingsController.instance.languageController.GetWord("YOU_GOT") + firstThrow + SettingsController.instance.languageController.GetWord("AND") + secondThrow, 1.5f);
                PopupSystem.instance.AddPopup(rollResult);
                board.MovePlayer(session.FindPlayer(nextPlayerName), firstThrow + secondThrow);
            };
            dice.onClose += rolldice;
            startTurn.onClose += Popup.Functionality.Show(dice);

            PopupSystem.instance.AddPopup(startTurn);
        }
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

    #endregion Obsługa eventów

    #region Warunki przegranej/wygranej

    /// <summary>
    /// Sprawdza, czy istnieje zwycięzca rozgrywki
    /// </summary>
    /// <returns>Informacja, czy istnieje zwycięzca rozgrywki</returns>
    private bool WinnerExists()
    {
        //Licza graczy, którzy dalej grają
        int stillPlaying = 0;

        for(int i = 0; i < session.playerCount; i++)
        {
            Player player = session.FindPlayer(i);
            if (!player.IsLoser) stillPlaying++;
        }

       return !(session.playerCount != 1) && stillPlaying == 1; //Jeżeli gramy w pokoju dla jednego gracza (testy), jedyny gracz nie może od razu wygrać
    }

    #endregion Warunki przegranej/wygranej
}
