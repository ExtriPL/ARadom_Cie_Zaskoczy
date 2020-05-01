using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour, IOnEventCallback
{
    public static EventManager instance;

    #region Eventy

    /*public delegate void RoomOvnerQuit();
    /// <summary>
    /// Event jest wywoływany, gdy właściciel pokoju wyjdzie z niego
    /// </summary>
    public event RoomOvnerQuit onRoomOvnerQuit;*/

    public delegate void GameStateChanged(GameState previousState, GameState newState);
    /// <summary>
    /// Event jest wywoływany, gdy zmieni się stan gry
    /// </summary>
    public event GameStateChanged onGameStateChanged;

    public delegate void PlayerQuit(string playerName);
    /// <summary>
    /// Event jest wywoływany, gdy gracz nie będący właścicielem wyjdzie z pokoju
    /// </summary>
    public event PlayerQuit onPlayerQuit;

    public delegate void PlayerMove(string playerName, int fromPlaceIndex, int toPlaceINdex);
    /// <summary>
    /// Event jest wywoływany, gdy gracz jest przesuwany na planszy.
    /// </summary>
    public event PlayerMove onPlayerMove;

    public delegate void TurnChange(string previousPlayerName, string currentPlayerName);
    public event TurnChange onTurnChange;

    #endregion

    private void OnEnable()
    {
        if (!instance) instance = this;
        else Destroy(this);
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
        
    }

    public void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;

        switch(eventCode)
        {
            case (byte)EventsId.GameStateChange:
                {
                    object[] data = (object[])photonEvent.CustomData;
                    GameState previousState = (GameState)data[0];
                    GameState newState = (GameState)data[1];
                    onGameStateChanged?.Invoke(previousState, newState);
                }
                break;
            case (byte)EventsId.PlayerQuit:
                {
                    object[] data = (object[])photonEvent.CustomData;
                    string playerName = (string)data[0];
                    onPlayerQuit?.Invoke(playerName);
                }
                break;
            case (byte)EventsId.PlayerMove:
                {
                    object[] data = (object[])photonEvent.CustomData;
                    string playerName = (string)data[0];
                    int fromFieldIndex = (int)data[1];
                    int toFieldIndex = (int)data[2];
                    onPlayerMove?.Invoke(playerName, fromFieldIndex, toFieldIndex);
                }
                break;
            case (byte)EventsId.TurnChange:
                {
                    object[] data = (object[])photonEvent.CustomData;
                    string previousPlayerName = (string)data[0];
                    string currentPlayerName = (string)data[1];
                    onTurnChange?.Invoke(previousPlayerName, currentPlayerName);
                }
                break;
        }
    }

    /// <summary>
    /// Wysyła event o zmianie stanu gry w klasie GameplayController
    /// </summary>
    /// <param name="previousState">Stan gry przed zmianą</param>
    /// <param name="newState">Stan gry po zmianie, obecny</param>
    public void SendOnRoomStateChanged(GameState previousState, GameState newState)
    {
        object[] data = { previousState, newState };
        RaiseEventOptions raiseOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        SendOptions sendOptions = new SendOptions { Reliability = true };
        PhotonNetwork.RaiseEvent((byte)EventsId.GameStateChange, data, raiseOptions, sendOptions);
    }

    /// <summary>
    /// Wysyła event o wyjściu gracza, który nie jest właścicielem, z pokoju
    /// </summary>
    /// <param name="player">Obiekt gracza na liście graczy w GamePlayController</param>
    public void SendOnPlayerQuit(string playerName)
    {
        object[] data =  { playerName };
        RaiseEventOptions raiseOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
        SendOptions sendOptions = new SendOptions { Reliability = true };
        PhotonNetwork.RaiseEvent((byte)EventsId.PlayerQuit, data, raiseOptions, sendOptions);
    }

    /// <summary>
    /// Wysyła event o poruszeniu się gracza
    /// </summary>
    /// <param name="playerName">Nazwa gracza</param>
    /// <param name="fromPlaceIndex">Numer pola, z którego poruszył się gracz</param>
    /// <param name="toPlaceIndex">Numer pola, na które poruszył się gracz</param>
    public void SendOnPlayerMove(string playerName, int fromPlaceIndex, int toPlaceIndex)
    {
        object[] data = { playerName, fromPlaceIndex, toPlaceIndex };
        RaiseEventOptions raiseOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        SendOptions sendOptions = new SendOptions { Reliability = true };
        PhotonNetwork.RaiseEvent((byte)EventsId.PlayerMove, data, raiseOptions, sendOptions);
    }

    /// <summary>
    /// Wysyła przez sieć event zmiany tury
    /// </summary>
    /// <param name="previousPlayerName">Nazwa gracza, którego tura właśnie się zakończyła</param>
    /// <param name="currentPlayerName">Nazwa gracza, którego tura właśnie się zaczyna</param>
    public void SendOnTurnChange(string previousPlayerName, string currentPlayerName)
    {
        object[] data = { previousPlayerName, currentPlayerName };
        RaiseEventOptions raiseOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        SendOptions sendOptions = new SendOptions { Reliability = true };
        PhotonNetwork.RaiseEvent((byte)EventsId.TurnChange, data, raiseOptions, sendOptions);
    }
}
