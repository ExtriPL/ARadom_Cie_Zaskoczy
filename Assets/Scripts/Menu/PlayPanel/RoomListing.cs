using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomListing: MonoBehaviourPunCallbacks
{
    private RoomInfo roomInfo;
    private BasePool pool;
    private MainMenuController mainMenuController;
    public void Init(RoomInfo roomInfo, BasePool pool, MainMenuController mainMenuController) 
    {
        this.mainMenuController = mainMenuController;
        this.roomInfo = roomInfo;
        this.pool = pool;
        Refresh();
    }
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        if ((roomList.Find(x => x.Name == roomInfo.Name).RemovedFromList)) Deinit(); else Refresh();
    }

    public void Deinit() 
    {
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
        PhotonNetwork.JoinRoom(roomInfo.Name);
        
    }

    public override void OnJoinedRoom()
    {
        mainMenuController.OpenPanel(8);
    }
}
