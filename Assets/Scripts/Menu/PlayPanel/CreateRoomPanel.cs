using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateRoomPanel : MonoBehaviourPunCallbacks, IPanelInitable
{
    MainMenuController mainMenuController;
    public TMP_InputField nameInputField;
    public TMP_InputField playerCountInputField;
    public TMP_InputField passwordInputField;
    public GameObject createButton;
    public void Init(MainMenuController mainMenuController)
    {
        this.mainMenuController = mainMenuController;
        nameInputField.text = PhotonNetwork.NickName + "'s Room";
        playerCountInputField.text = Keys.Menu.MIN_PLAYERS_COUNT.ToString();
    }

    public void OnPlayersCountChanged(string playerCountText)
    {
        int.TryParse(playerCountText.Replace("\u200B", ""), out int playerCount);
        CheckReady(nameInputField.text.Replace("\u200B", "").Trim(), playerCount);
    }

    public void OnRoomNameChanged(string roomName)
    {
        int.TryParse(playerCountInputField.text.Replace("\u200B", ""), out int playerCount);
        CheckReady(roomName, playerCount);
    }

    /// <summary>
    /// Sprawdza, czy nazwa pokoju i ilość graczy jest ustawiona prawidłowo
    /// </summary>
    private void CheckReady(string roomName, int playerCount)
    {
        if(roomName.Length >= Keys.Menu.ROOM_NAME_MIN_LENGTH && playerCount.IsBetween(Keys.Menu.MIN_PLAYERS_COUNT, Keys.Menu.MAX_PLAYERS_COUNT))
        {
            createButton.GetComponent<Button>().interactable = true;
        }
        else createButton.GetComponent<Button>().interactable = false;
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.CurrentRoom.PlayerTtl = Keys.Session.PLAYER_TTL;
        mainMenuController.OpenPanel(8);
    }

    public void Create()
    {
        string roomSize = playerCountInputField.text.Replace("\u200B", "");
        RoomOptions roomOptions = new RoomOptions() { IsVisible = true, IsOpen = true, MaxPlayers = (byte)int.Parse(roomSize)};
        PhotonNetwork.CreateRoom(nameInputField.text, roomOptions, null);
    }

    public void PreInit()
    {
    }
}
