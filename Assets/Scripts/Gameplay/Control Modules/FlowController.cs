using Photon.Pun;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class FlowController : IEventSubscribable
{
    private GameplayController gameplayController;

    private List<Tuple<IFlowControlable, object[]>> controllerQueue = new List<Tuple<IFlowControlable, object[]>>();
    /// <summary>
    /// Obeikt obecnie sprawujący kontrole nad przepływem gry
    /// </summary>
    private IFlowControlable flowMaster;

    /// <summary>
    /// Czas rozpoczęcia etapu lub przyznania kontroli nowemu obiektowi
    /// </summary>
    private float beginTime;
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
    /// Gracz, którego obecnie jest tura
    /// </summary>
    public Player CurrentPlayer
    {
        get => gameplayController.session.FindPlayer(gameplayController.board.dice.currentPlayer);
    }

    #region Inicjalizacja

    public void SubscribeEvents() {}

    public void UnsubscribeEvents() {}

    public void StartGame()
    {
        gameplayController = GameplayController.instance;
        ResetSettings();
        if (gameplayController.session.roomOwner.IsLocal)
            EventManager.instance.SendOnTurnChanged("", gameplayController.board.dice.currentPlayer);
    }

    public void Update()
    {
        //Przepływem zajmuje się tylko i wyłącznie gracz, którego jest obecnie tura
        if (CurrentPlayer.NetworkPlayer.IsLocal)
        {
            float time = Time.time - beginTime;
            
            //Po minięciu czasu pokazuje się przycisk, umożliwiający zmienienie tury
            if (time >= showTime)
                GameplayController.instance.menu.SetActiveNextTurnButton(true);

            //Po minięciu czasu pokazuje się timer, który wskazuje ile czasu zostało do automatycznego zakończenia tury
            if(time >= endTime - countingTime)
            {
                GameplayController.instance.menu.SetNextTurnButtonTimer((int)Mathf.Ceil(endTime - time));
                GameplayController.instance.menu.SetActiveNextTurnButtonTimer(true);
            }

            //Po minięciu odpowiedniej ilości czasu, automatycznie kończy rundę
            if (time >= endTime)
                EndTurn();
        }
    }

    #endregion Inicjalizacja

    #region Sterowanie przepływem

    /// <summary>
    /// Umieszcza obiekt w kolejce do kontroli przepływu
    /// </summary>
    /// <param name="controlable">Obiekt, który chce przejąć kontrole nad przepływem</param>
    /// <param name="args">Argumenty potrzebne do rozpoczęcia kontroli przepływu przez obiekt</param>
    public void Enqueue(IFlowControlable controlable, object[] args = null)
    {
        if (flowMaster == null)
        {
            flowMaster = controlable;
            beginTime = Time.deltaTime;
        }
        else if(!InQueue(controlable))
        {
            int putIndex = 0;

            for (int i = 0; i < controllerQueue.Count; i++)
            {
                if (controllerQueue[i].Item1.FlowPriority < controlable.FlowPriority)
                {
                    putIndex = i;
                    break;
                }
            }

            controllerQueue.Insert(putIndex, Tuple.Create(controlable, args));
        }
    }

    /// <summary>
    /// Kończy kontrole obecnego obiektu i przekazuje ją kolejnemu, jeżeli taki istnieje
    /// </summary>
    /// <param name="controlable">Obiekt, który kończy swoją kontrole</param>
    public void ReturnControl(IFlowControlable controlable)
    {
        //Zakończyć etap może tylko obecnie sprawujący kontrolę obiekt
        if (IsFlowMaster(controlable))
        {
            if (controllerQueue.Count > 0)
            {
                GiveControl(controllerQueue[0]);
                controllerQueue.RemoveAt(0);
            }
        }
        else
            Debug.LogError("Podany obiekt nie ma kontroli nad przepływem, a próbuje ją zwrócić");
    }

    /// <summary>
    /// Daje natychmiastowo kontrolę nad przepływem rozgrywki
    /// </summary>
    /// <param name="newMaster">Nowy kontroler przepływu</param>
    private void GiveControl(Tuple<IFlowControlable, object[]> newMaster)
    {
        flowMaster = newMaster.Item1;
        beginTime = Time.deltaTime;
        flowMaster.TransmitFlow(newMaster.Item2);
    }

    /// <summary>
    /// Sprawdza, czy podany obiekt jest obecnym kontrolerem przepływu
    /// </summary>
    /// <param name="controlable">Kontroler przepływu</param>
    /// <returns></returns>
    public bool IsFlowMaster(IFlowControlable controlable)
    {
        return flowMaster == controlable;
    }

    /// <summary>
    /// Sprawdza, czy podany obiekt znajduje już się na liście do uzyskania kontroli nad przepływem
    /// </summary>
    public bool InQueue(IFlowControlable controlable)
    {
        foreach(Tuple<IFlowControlable, object[]> controller in controllerQueue)
        {
            if (controller == controlable)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Sprawdza, czy podany obiekt jest obecnym kontrolerem. Jeżeli tak nie jest, dodaje go do kolejki
    /// </summary>
    /// <param name="controlable">Kontroler przepływu</param>
    /// <param name="args">Argumenty, które służą do inicjalizacji kontrolera</param>
    /// <returns>True - jeżeli obiekt jest obecnym kontrolerem, False - jeżeli nie jest</returns>
    public bool CheckAndEnqueue(IFlowControlable controlable, object[] args = null)
    {
        if (!IsFlowMaster(controlable))
        {
            Enqueue(controlable, args);
            return false;
        }

        return true;
    }

    #endregion Sterowanie przepływem

    #region Sterowanie rozgrywką

    private void End()
    {
        ResetSettings();
        /*
         Zamykanie popupów z oznaczeniem do zamknięcia na koniec tury gracza
         Zmienianie obecnie aktywnego gracza
         Ukrycie przycisku zakończenia tury/Wyłączenie go
         */
        DefaultEnding();
        NextTurn();
    }

    /// <summary>
    /// Przywraca domyślne ustawienia przepływu
    /// </summary>
    private void ResetSettings()
    {
        flowMaster = null;
        controllerQueue.Clear();
        showTime = Keys.Flow.SHOW_TIME;
        endTime = Keys.Flow.END_TIME;
        countingTime = Keys.Flow.COUNTING_TIME;
        beginTime = Time.time;

        GameplayController.instance.menu.SetActiveNextTurnButton(false);
        GameplayController.instance.menu.SetActiveNextTurnButtonTimer(false);
    }

    /// <summary>
    /// Próbuje zakończyć obecną turę. Jeżeli gracz zbankrutował, daje mu możliwość ocalenia się.
    /// Sprawdza, czy któryś z graczy nie wygrał.
    /// </summary>
    private void EndTurn()
    {
        if (CurrentPlayer.Money < 0f)
        {
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
            Popup.Functionality.Destroy().Invoke(source);
        };
        Popup.PopupAction noAction = delegate (Popup source)
        {
            gameplayController.LosePlayer(CurrentPlayer);
            CheckWin();
            Popup.Functionality.Destroy().Invoke(source);
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
            

            QuestionPopup startTurn = new QuestionPopup(SettingsController.instance.languageController.GetWord("TURN_STARTED"));
            startTurn.AddButton("Ok", Popup.Functionality.Destroy(startTurn));
            startTurn.onClose += delegate { PopupSystem.instance.ShowDice(RollResult()); };

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
            string message = SettingsController.instance.languageController.GetWord("YOU_GOT") + firstThrow + SettingsController.instance.languageController.GetWord("AND") + secondThrow;
            QuestionPopup showRoll = QuestionPopup.CreateOkDialog(message, delegate { board.MovePlayer(CurrentPlayer, firstThrow + secondThrow); });
            IconPopup rollResult = new IconPopup(IconPopupType.RollResult, showRoll);
            PopupSystem.instance.AddPopup(rollResult);
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
            QuestionPopup endTurn = new QuestionPopup(SettingsController.instance.languageController.GetWord("TURN_ENDED"));
            endTurn.AddButton("Ok", Popup.Functionality.Destroy(endTurn));
            PopupSystem.instance.AddPopup(endTurn);
        }
    }

    #endregion Sterowanie rozgrywką
}
