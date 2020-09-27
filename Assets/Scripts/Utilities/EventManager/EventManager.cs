using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour, IOnEventCallback
{
    public static EventManager instance;

    #region Eventy
    public delegate void GameStateChanged(GameState previousState, GameState newState);
    /// <summary>
    /// Event jest wywoływany, gdy zmieni się stan gry
    /// </summary>
    public event GameStateChanged onGameStateChanged;

    public delegate void PlayerEvent(string playerName);
    /// <summary>
    /// Event jest wywoływany, gdy gracz nie będący właścicielem wyjdzie z pokoju
    /// </summary>
    public event PlayerEvent onPlayerQuited;
    /// <summary>
    /// Event jest wywoływany, gdy jeden z graczy przegra grę
    /// </summary>
    public event PlayerEvent onPlayerLostGame;
    /// <summary>
    /// Event jest wywoływany, gdy jeden z graczy zostanie uwięziony w więzieniu
    /// </summary>
    public event PlayerEvent onPlayerImprisoned;

    public delegate void PlayerReady(string playerName, bool ready);
    /// <summary>
    /// Event służący do ustalenia gotowości gracza w pokoju
    /// </summary>
    public event PlayerReady onPlayerReady;

    public delegate void PlayerDisplacement(string playerName, int fromPlaceIndex, int toPlaceINdex);
    /// <summary>
    /// Event jest wywoływany, gdy gracz jest przesuwany na planszy.
    /// </summary>
    public event PlayerDisplacement onPlayerMoved;
    /// <summary>
    /// Event jest wywoływany, gdy gracz zostanie przeteleportowany
    /// </summary>
    public event PlayerDisplacement onPlayerTeleported;
    
    public delegate void TurnChange(string previousPlayerName, string currentPlayerName);
    /// <summary>
    ///  Event jest wywoływany, gdy zmienia się tura.
    /// </summary>
    /// <param name="previousPlayerName">Poprzedni gracz</param>
    /// <param name="currentPlayerName">Następny gracz</param>
    public event TurnChange onTurnChanged;
   
    public delegate void PlaceActing(string playerName, int placeId);
    /// <summary>
    /// Event jest wywoływany, gdy gracz otrzyma budynek od banku.
    /// </summary>
    /// <param name="playerName">Nazwa gracza</param>
    /// <param name="fieldId">ID budynku</param>
    public event PlaceActing onAquiredBuilding;
    /// <summary>
    /// Event jest wywoływany, gdy budynek jakiś budynek zostanie ulepszony
    /// </summary>
    public event PlaceActing onUpgradedBuilding;

    public delegate void Auction(string playerName, int placeId, string bidder, float bid, string passPlayerName);
    /// <summary>
    /// Event jest wywoływany, gdy trwa licytacja
    /// </summary>
    public event Auction onAuction;

    public delegate void Sync(int syncNumber, string source, string target);
    /// <summary>
    /// Event służący do synchronizacji wczytywania
    /// </summary>
    public event Sync onSync;

    public delegate void Pay(string payerName, string receiverName, float amount);
    public event Pay onPay;

    #endregion Eventy

    #region Inicjalizacja

    private void OnEnable()
    {
        instance = this;
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    #endregion Inicjalizacja

    #region Wysyłanie eventów sieciowych

    /// <summary>
    /// Wysyła event o zmianie stanu gry w klasie GameplayController
    /// </summary>
    /// <param name="previousState">Stan gry przed zmianą</param>
    /// <param name="newState">Stan gry po zmianie, obecny</param>
    public void SendOnGameStateChanged(GameState previousState, GameState newState)
    {
        object[] data = { previousState, newState };
        RaiseEventOptions raiseOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        SendOptions sendOptions = new SendOptions { Reliability = true };
        PhotonNetwork.RaiseEvent((byte)EventsId.GameStateChange, data, raiseOptions, sendOptions);
    }

    /// <summary>
    /// Wysyła event o wyjściu gracza, który nie jest właścicielem, z pokoju
    /// </summary>
    /// <param name="playerName">Nazwa gracza</param>
    public void SendOnPlayerQuited(string playerName)
    {
        object[] data = { playerName };
        RaiseEventOptions raiseOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        SendOptions sendOptions = new SendOptions { Reliability = true };
        PhotonNetwork.RaiseEvent((byte)EventsId.PlayerQuit, data, raiseOptions, sendOptions);
    }

    /// <summary>
    /// Wysyła event o zmianie gotowości gracza
    /// </summary>
    /// <param name="playerName">Nazwa gracza</param>
    public void SendOnPlayerReady(string playerName, bool ready)
    {
        Debug.Log(playerName + ":" + ready);
        object[] data = { playerName, ready };
        RaiseEventOptions raiseOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        SendOptions sendOptions = new SendOptions { Reliability = true };
        PhotonNetwork.RaiseEvent((byte)EventsId.PlayerReady, data, raiseOptions, sendOptions);
    }

    /// <summary>
    /// Wysyła event o poruszeniu się gracza
    /// </summary>
    /// <param name="playerName">Nazwa gracza</param>
    /// <param name="fromPlaceIndex">Numer pola, z którego poruszył się gracz</param>
    /// <param name="toPlaceIndex">Numer pola, na które poruszył się gracz</param>
    public void SendOnPlayerMoved(string playerName, int fromPlaceIndex, int toPlaceIndex)
    {
        object[] data = { playerName, fromPlaceIndex, toPlaceIndex };
        RaiseEventOptions raiseOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        SendOptions sendOptions = new SendOptions { Reliability = true };
        PhotonNetwork.RaiseEvent((byte)EventsId.PlayerMove, data, raiseOptions, sendOptions);
    }

    /// <summary>
    /// Wysyła event o poruszeniu się gracza
    /// </summary>
    /// <param name="playerName">Nazwa gracza</param>
    /// <param name="fromPlaceIndex">Numer pola, z którego poruszył się gracz</param>
    /// <param name="toPlaceIndex">Numer pola, na które poruszył się gracz</param>
    public void SendOnPlayerTeleported(string playerName, int fromPlaceIndex, int toPlaceIndex)
    {
        object[] data = { playerName, fromPlaceIndex, toPlaceIndex };
        RaiseEventOptions raiseOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        SendOptions sendOptions = new SendOptions { Reliability = true };
        PhotonNetwork.RaiseEvent((byte)EventsId.PlayerTeleport, data, raiseOptions, sendOptions);
    }

    /// <summary>
    /// Wysyła przez sieć event zmiany tury
    /// </summary>
    /// <param name="previousPlayerName">Nazwa gracza, którego tura właśnie się zakończyła</param>
    /// <param name="currentPlayerName">Nazwa gracza, którego tura właśnie się zaczyna</param>
    public void SendOnTurnChanged(string previousPlayerName, string currentPlayerName)
    {
        object[] data = { previousPlayerName, currentPlayerName };
        RaiseEventOptions raiseOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        SendOptions sendOptions = new SendOptions { Reliability = true };
        PhotonNetwork.RaiseEvent((byte)EventsId.TurnChange, data, raiseOptions, sendOptions);
    }

    /// <summary>
    /// Wysyła przez sieć informacje o otrzymaniu budynku przez gracza od banku
    /// </summary>
    /// <param name="playerName">Nazwa gracza, który otzrzymał budynek</param>
    /// <param name="placeId">Id budynku</param>
    public void SendOnPlayerAquiredBuiding(string playerName, int placeId)
    {
        object[] data = { playerName, placeId };
        RaiseEventOptions raiseOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        SendOptions sendOptions = new SendOptions { Reliability = true };
        PhotonNetwork.RaiseEvent((byte)EventsId.PlayerAquiredBuiding, data, raiseOptions, sendOptions);
    }

    /// <summary>
    /// Wywyła przez sieć informację o ulepszeniu budynku przez gracza
    /// </summary>
    /// <param name="playerName">Nazwa gracza, który ulepszył budynek</param>
    /// <param name="placeId">Numer pola na planszy, które zostało ulepszone</param>
    public void SendOnPlayerUpgradedBuilding(string playerName, int placeId)
    {
        object[] data = { playerName, placeId };
        RaiseEventOptions raiseOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        SendOptions sendOptions = new SendOptions { Reliability = true };
        PhotonNetwork.RaiseEvent((byte)EventsId.PlayerUpgradeBuilding, data, raiseOptions, sendOptions);
    }

    /// <summary>
    /// Wysyła przez sieć informacje o licytacji
    /// </summary>
    /// <param name="playerName">Nazwa gracza, który wywołał licytację</param>
    /// <param name="placeId">Numer pola na planszy, o które toczy się licytacja</param>
    /// <param name="bidder">Nazwa gracza, który ostatni wylicytował</param>
    /// <param name="bid">Kwota ostatniej licytacji</param>
    public void SendOnAuction(string playerName, int placeId, string bidder, float bid, string passPlayerName)
    {
        object[] data = { playerName, placeId, bidder, bid, passPlayerName };
        RaiseEventOptions raiseOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        SendOptions sendOptions = new SendOptions { Reliability = true };
        PhotonNetwork.RaiseEvent((byte)EventsId.Auction, data, raiseOptions, sendOptions);
    }

    /// <summary>
    /// Wywyła przez sieć informację służące do synchronizacji
    /// </summary>
    /// <param name="syncNumber">Numer synchronizacj. 0 - wysyłamy zapytanie o synchronizację, 1 - odpowiadamy na zapytanie o synchronizację</param>
    /// <param name="source">Nazwa gracza, który wysyła event</param>
    /// <param name="target">Nazwa gracza, do którego jest wysyłany event</param>
    public void SendSyncEvent(int syncNumber, string source, string target)
    {
        object[] data = { syncNumber, source, target };
        RaiseEventOptions raiseOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
        SendOptions sendOptions = new SendOptions { Reliability = true };
        PhotonNetwork.RaiseEvent((byte)EventsId.Sync, data, raiseOptions, sendOptions);
    }

    /// <summary>
    /// Wysyła przez sieć informację o dokonaniu tranzakcji między graczami
    /// </summary>
    /// <param name="payerName">Gracz, który przekazywał pieniądze</param>
    /// <param name="receiverName">Gracz, który otrzymał pieniądze</param>
    /// <param name="amount">Ilość wymienianych pieniędzy</param>
    public void SendPayEvent(string payerName, string receiverName, float amount)
    {
        object[] data = { payerName, receiverName, amount };
        RaiseEventOptions raiseOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        SendOptions sendOptions = new SendOptions { Reliability = true };
        PhotonNetwork.RaiseEvent((byte)EventsId.Pay, data, raiseOptions, sendOptions);
    }

    /// <summary>
    /// Wysyłą przez sieć informację o przegraniu gry przez gracza
    /// </summary>
    /// <param name="playerName">Nazwa gracza, który przegrał</param>
    public void SendOnPlayerLostGame(string playerName)
    {
        object[] data = { playerName };
        RaiseEventOptions raiseOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        SendOptions sendOptions = new SendOptions { Reliability = true };

        PhotonNetwork.RaiseEvent((byte)EventsId.PlayerLostGame, data, raiseOptions, sendOptions);
    }

    public void SendOnPlayerImprisoned(string playerName)
    {
        object[] data = { playerName };
        RaiseEventOptions raiseOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        SendOptions sendOptions = new SendOptions { Reliability = true };

        PhotonNetwork.RaiseEvent((byte)EventsId.PlayerImprison, data, raiseOptions, sendOptions);
    }

    #endregion Wysyłanie eventów sieciowych

    /// <summary>
    /// Zamiana event sieciowego na event lokalny
    /// </summary>
    /// <param name="photonEvent">Dane eventu</param>
    public void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;

        switch (eventCode)
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
                    onPlayerQuited?.Invoke(playerName);
                }
                break;
            case (byte)EventsId.PlayerMove:
                {
                    object[] data = (object[])photonEvent.CustomData;
                    string playerName = (string)data[0];
                    int fromFieldIndex = (int)data[1];
                    int toFieldIndex = (int)data[2];
                    onPlayerMoved?.Invoke(playerName, fromFieldIndex, toFieldIndex);
                }
                break;
            case (byte)EventsId.TurnChange:
                {
                    object[] data = (object[])photonEvent.CustomData;
                    string previousPlayerName = (string)data[0];
                    string currentPlayerName = (string)data[1];
                    onTurnChanged?.Invoke(previousPlayerName, currentPlayerName);
                }
                break;
            case (byte)EventsId.PlayerAquiredBuiding:
                {
                    object[] data = (object[])photonEvent.CustomData;
                    string playerName = (string)data[0];
                    int fieldId = (int)data[1];
                    onAquiredBuilding?.Invoke(playerName, fieldId);
                }
                break;
            case (byte)EventsId.Auction:
                {
                    object[] data = (object[])photonEvent.CustomData;
                    string playerName = (string)data[0];
                    int placeId = (int)data[1];
                    string bidder = (string)data[2];
                    float bid = (float)data[3];
                    string passPlayerName = (string)data[4];
                    onAuction?.Invoke(playerName, placeId, bidder, bid, passPlayerName);
                }
                break;
            case (byte)EventsId.Sync:
                {
                    object[] data = (object[])photonEvent.CustomData;
                    int syncNumber = (int)data[0];
                    string source = (string)data[1];
                    string target = (string)data[2];
                    onSync?.Invoke(syncNumber, source, target);
                }
                break;
            case (byte)EventsId.PlayerUpgradeBuilding:
                {
                    object[] data = (object[])photonEvent.CustomData;
                    string playerName = (string)data[0];
                    int placeId = (int)data[1];
                    onUpgradedBuilding?.Invoke(playerName, placeId);
                }
                break;
            case (byte)EventsId.Pay:
                {
                    object[] data = (object[])photonEvent.CustomData;
                    string payerName = (string)data[0];
                    string receiverName = (string)data[1];
                    float amount = (float)data[2];
                    onPay?.Invoke(payerName, receiverName, amount);
                }
                break;
            case (byte)EventsId.PlayerLostGame:
                {
                    object[] data = (object[])photonEvent.CustomData;
                    string playerName = (string)data[0];
                    onPlayerLostGame?.Invoke(playerName);
                }
                break;
            case (byte)EventsId.PlayerReady:
                {
                    object[] data = (object[])photonEvent.CustomData;
                    string playerName = (string)data[0];
                    bool ready = (bool)data[1];
                    onPlayerReady?.Invoke(playerName, ready);
                }
                break;
            case (byte)EventsId.PlayerTeleport:
                {
                    object[] data = (object[])photonEvent.CustomData;
                    string playerName = (string)data[0];
                    int fromFieldIndex = (int)data[1];
                    int toFieldIndex = (int)data[2];
                    onPlayerTeleported?.Invoke(playerName, fromFieldIndex, toFieldIndex);
                }
                break;
            case (byte)EventsId.PlayerImprison:
                {
                    object[] data = (object[])photonEvent.CustomData;
                    string playerName = (string)data[0];

                    onPlayerImprisoned?.Invoke(playerName);
                }
                break;
        }
    }
}
