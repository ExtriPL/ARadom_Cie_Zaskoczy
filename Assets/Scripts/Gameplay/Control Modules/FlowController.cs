using Photon.Pun;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class FlowController : IEventSubscribable
{
    private GameplayController gameplayController;

    /// <summary>
    /// Czas rozpoczęcia tury obecnego gracza
    /// </summary>
    private float beginTime;
    /// <summary>
    /// Obecny czas, przez jaki działa przepływ
    /// </summary>
    private float timePassed;
    /// <summary>
    /// Czas, po jakim pokaże się przycizk zakończenia tury
    /// </summary>
    public float showTime;
    /// <summary>
    /// Czas, po jakim tura automatycznie się zakończy
    /// </summary>
    public float endTime;
    /// <summary>
    /// Czas przed zakończeniem tury, po którego osiągnięciu włącza się licznik pokazujący, ile jeszcze zostało tury
    /// </summary>
    public float countingTime;
    /// <summary>
    /// Czas po rozpoczęciu tury, po którym kostka rzuci się automatycznie
    /// </summary>
    public float autoDiceRollTime;
    /// <summary>
    /// Gracz, którego obecnie jest tura
    /// </summary>
    public Player CurrentPlayer
    {
        get
        {
            if (gameplayController == null)
                gameplayController = GameplayController.instance;

            return gameplayController.session.FindPlayer(gameplayController.board.dice.currentPlayer);
        }
    }
    /// <summary>
    /// Flaga określająca, czy przepływ jest w tej chwili zastopowany
    /// </summary>
    public bool FlowPaused { get; private set; }
    /// <summary>
    /// Czas, który minął od rozpoczęcia tury obenego gracza
    /// </summary>
    private float TurnTime { get => Time.time - beginTime; }

    private List<Popup> closeOnDiceCloseList = new List<Popup>();
    private bool diceClosed;

    public bool FlowStarted { get; private set; }

    #region Inicjalizacja

    public void SubscribeEvents() 
    {
        EventManager.instance.onTurnChanged += OnTurnChanged;
        EventManager.instance.onPlayerLostGame += OnPlayerLost;
    }

    public void UnsubscribeEvents() 
    {
        EventManager.instance.onTurnChanged -= OnTurnChanged;
        EventManager.instance.onPlayerLostGame -= OnPlayerLost;
    }

    public void StartGame()
    {
        gameplayController = GameplayController.instance;
        ResetSettings();
        FlowStarted = true;
        if (gameplayController.session.roomOwner.IsLocal)
            EventManager.instance.SendOnTurnChanged("", gameplayController.board.dice.currentPlayer);
    }

    public void Update()
    {
        if (FlowStarted && gameplayController.session.gameState == GameState.running)
        {
            if (CurrentPlayer == null || CurrentPlayer.IsLoser)
                return;

            //Przepływem zajmuje się tylko i wyłącznie gracz, którego jest obecnie tura
            if (CurrentPlayer.NetworkPlayer.IsLocal)
            {
                if (!FlowPaused)
                {
                    timePassed += Time.deltaTime;

                    //Po minięciu czasu pokazuje się przycisk, umożliwiający zmienienie tury
                    if (timePassed >= showTime)
                        GameplayController.instance.menu.SetActiveNextTurnButton(true);

                    //Po minięciu czasu pokazuje się timer, który wskazuje ile czasu zostało do automatycznego zakończenia tury
                    if (timePassed >= endTime - countingTime)
                    {
                        GameplayController.instance.menu.SetNextTurnButtonTimer((int)Mathf.Ceil(endTime - timePassed));
                        GameplayController.instance.menu.SetActiveNextTurnButtonTimer(true);
                    }

                    //Po minięciu odpowiedniej ilości czasu, automatycznie kończy rundę
                    if (timePassed >= endTime)
                        EndTurn();
                }

                if (TurnTime > autoDiceRollTime && !diceClosed)
                {
                    foreach (Popup popup in closeOnDiceCloseList)
                        PopupSystem.instance.ClosePopup(popup);

                    IconBox diceBox = PopupSystem.instance.DiceBox;
                    if (diceBox != null)
                    {
                        diceClosed = true;
                        PopupSystem.instance.ClosePopup(diceBox.source);
                    }
                }
            }
        }
    }

    #endregion Inicjalizacja

    #region Sterowanie przepływem

    /// <summary>
    /// Wstrzymuje przepływ
    /// </summary>
    public void Pause()
    {
        FlowPaused = true;
    }

    /// <summary>
    /// Przywraca działanie przepływu
    /// </summary>
    public void Resume()
    {
        FlowPaused = false;
    }

    /// <summary>
    /// Przewija przepływ do momentu pokazania przycisku zakończenia tury z czasem
    /// </summary>
    public void RewindToCounting()
    {
        timePassed = showTime;
        if (TurnTime < showTime)
            beginTime -= (showTime - TurnTime);
    }

    /// <summary>
    /// Przwija przepływ do momentu pokazania przycisku zakończenia tury
    /// </summary>
    public void RewindToSkiping()
    {
        timePassed = endTime - countingTime;
        if (TurnTime < (endTime - countingTime))
            beginTime -= (TurnTime - endTime + countingTime);
    }

    /// <summary>
    /// Przywraca domyślne ustawienia przepływu
    /// </summary>
    private void ResetSettings()
    {
        timePassed = 0;
        beginTime = Time.time;
        FlowPaused = false;
        closeOnDiceCloseList.Clear();
        diceClosed = false;

        GameplayController.instance.menu.SetActiveNextTurnButton(false);
        GameplayController.instance.menu.SetActiveNextTurnButtonTimer(false);
    }

    public void CloseOnDiceClose(Popup popup)
    {
        closeOnDiceCloseList.Add(popup);
    }

    #endregion Sterowanie przepływem

    #region Sterowanie rozgrywką

    /// <summary>
    /// Końcowa faza zakończenia rundy
    /// </summary>
    public void End()
    {
        Resume();

        if (CurrentPlayer != null && CurrentPlayer.NetworkPlayer != null && !CurrentPlayer.IsLoser)
        {
            PopupSystem.instance.ClosePopups(AutoCloseMode.EndOfTurn);
            GameplayController.instance.menu.SetActiveNextTurnButton(false);
            GameplayController.instance.arController.centerBuilding.GetComponent<CenterVisualiser>().ToggleVisibility(false);
            DefaultEnding();
        }

        ResetSettings();
        NextTurn();
    }

    /// <summary>
    /// Próbuje zakończyć obecną turę. Jeżeli gracz zbankrutował, daje mu możliwość ocalenia się.
    /// Sprawdza, czy któryś z graczy nie wygrał.
    /// </summary>
    public void EndTurn()
    {
        if (CurrentPlayer.Money < 0f)
        {
            Pause();
            //Przegrana gracza przez bankructwo

            //Jeżeli gracz może zaciągnąć pożyczkę
            if (gameplayController.banking.CanTakeLoan(CurrentPlayer))
            {
                //Danie graczowi szansy na zaciągnięcie pożyczki, by mógł ocalić się przed bankructwem
                ShowLastChanceLoanMessage();
            }
            else
            {
                //Jeżeli dojdzie do tego miejsca, gracz nie ma już żadnych szans na ratunek i przegrywa
                gameplayController.LosePlayer(CurrentPlayer);
                CheckWin();
            }
        }
        else End();
    }

    /// <summary>
    /// Zmienia turę na nestępną.
    /// </summary>
    private void NextTurn()
    {
        Board board = gameplayController.board;
        string previousPlayer = board.dice.currentPlayer;
        board.dice.NextTurn();
        board.dice.RollDice();
        string nextPlayer = board.dice.currentPlayer;

        EventManager.instance.SendOnTurnChanged(previousPlayer, nextPlayer);
    }

    /// <summary>
    /// Pokazuje popup, mówiący o bankructwie i dający szanse wzięcia pożyczki pozwalającej na dalszą grę
    /// </summary>
    private void ShowLastChanceLoanMessage()
    {
        LanguageController language = SettingsController.instance.languageController;
        string message = language.GetWord("LAST_CHANCE_WANT_TO_TAKE_LOAN");

        Popup.PopupAction yesAction = delegate (Popup source)
        {
            gameplayController.banking.TakeLoan(CurrentPlayer);
            End();
        };
        Popup.PopupAction noAction = delegate (Popup source)
        {
            gameplayController.LosePlayer(CurrentPlayer);
            CheckWin();
        };

        QuestionPopup lastChange = QuestionPopup.CreateYesNoDialog(message, yesAction, noAction);
        PopupSystem.instance.AddPopup(lastChange);
    }

    /// <summary>
    /// Sprawdza, czy istnieje zwycięzca gry. Jeżeli tak jest wyświetla komunikat. Jeżeli nie, rozpoczyna kolejną turę.
    /// Uwaga! Nie powinno byćwywoływane nigdzie poza funkcją session.KickPlayer!
    /// </summary>
    public void CheckWin()
    {
        if (gameplayController.WinnerExists())
        {
            //Informacja o wygranej jakiegoś gracza
            //Zakończenie rozgrywki
            gameplayController.session.gameState = GameState.ended;
        }
        else End();
    }

    /// <summary>
    /// Domyślne akcje wywoływane na początku tury
    /// </summary>
    public void DefaultBegining()
    {
        if(CurrentPlayer.NetworkPlayer.IsLocal)
        {
            GameplayController.instance.flow.Pause();;
            string message = SettingsController.instance.languageController.GetWord("TURN_STARTED");
            QuestionPopup startTurn = QuestionPopup.CreateOkDialog(message);
            startTurn.onClose += delegate { PopupSystem.instance.ShowDice(RollResult()); };
            CloseOnDiceClose(startTurn);

            PopupSystem.instance.AddPopup(startTurn);
        }
    }

    public Popup.PopupAction RollResult()
    {
        Board board = GameplayController.instance.board;

        Popup.PopupAction rolldice = delegate (Popup source)
        {
            int firstThrow = board.dice.last1;
            int secondThrow = board.dice.last2;
            GameplayController.instance.diceController.Roll(firstThrow, secondThrow,
                delegate 
                {
                    board.MovePlayer(CurrentPlayer, firstThrow + secondThrow);
                });
        };

        return rolldice;
    }

    /// <summary>
    /// Akcje wywoływane w momencie końca tury
    /// </summary>
    private void DefaultEnding()
    {
        if(CurrentPlayer.NetworkPlayer.IsLocal)
        {
            string message = SettingsController.instance.languageController.GetWord("TURN_ENDED");
            QuestionPopup endTurn = QuestionPopup.CreateOkDialog(message);
            PopupSystem.instance.AddPopup(endTurn);
        }
    }

    #endregion Sterowanie rozgrywką

    private void OnTurnChanged(string previousPlayerName, string currentPlayerName)
    {
        //Gracz, który rozpoczął teraz turę, ma resetowane ustawienia FlowControllera
        if (CurrentPlayer != null && CurrentPlayer.NetworkPlayer.IsLocal)
            ResetSettings();
    }

    private void OnPlayerLost(string playerName)
    {
        //Gracz, który przegrał ma resetowane ustawienia
        if (CurrentPlayer != null && CurrentPlayer.NetworkPlayer.IsLocal)
            ResetSettings();
    }
}
