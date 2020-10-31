using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine;
using UnityEngine.UI;

public class RoomListing: MonoBehaviourPunCallbacks
{
    private RoomInfo roomInfo;
    private BasePool pool;
    private MainMenuController mainMenuController;
    PlayPanel playPanel;
    public void Init(RoomInfo roomInfo, PlayPanel playPanel, BasePool pool, MainMenuController mainMenuController) 
    {
        this.mainMenuController = mainMenuController;
        this.roomInfo = roomInfo;
        this.pool = pool;
        this.playPanel = playPanel;
        Refresh();
    }
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        if (!(roomList.Contains(roomInfo)) || (roomList.Find(x => x.Name == roomInfo.Name).RemovedFromList)) Deinit();
        else
        {
            Refresh();
        }
    }

    public void Deinit() 
    {
        playPanel.roomList.Remove(roomInfo);
        roomInfo = null;
        pool.ReturnObject(gameObject);
    }

    private void Refresh() 
    {
        gameObject.GetComponentInChildren<TextMeshProUGUI>().text = roomInfo.Name;
        gameObject.GetComponentInChildren<TextMeshProUGUI>().text += " ";
        gameObject.GetComponentInChildren<TextMeshProUGUI>().text += roomInfo.PlayerCount;
        gameObject.GetComponentInChildren<TextMeshProUGUI>().text += "/";
        gameObject.GetComponentInChildren<TextMeshProUGUI>().text += roomInfo.MaxPlayers;
    }

    public void JoinRoom() 
    {
        StartCoroutine(WaitForLobby());
    }

    private IEnumerator WaitForLobby()
    {
        yield return new WaitUntil(() => PhotonNetwork.IsConnectedAndReady);

        Hashtable table = new Hashtable();
        table.Add("Room_PlayerReady", false);
        PhotonNetwork.LocalPlayer.SetCustomProperties(table);
        PhotonNetwork.JoinRoom(roomInfo.Name);
    }

    public override void OnJoinedRoom()
    {
        mainMenuController.OpenPanel(8);
    }

    public void StartLoadingScreen() 
    {
        mainMenuController.StartLoadingScreen();
    }
}
