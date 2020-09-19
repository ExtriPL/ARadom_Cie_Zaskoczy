using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayPanel : MonoBehaviourPunCallbacks, IPanelInitable
{
    private BasePool basePool;
    public GameObject content;
    public GameObject template;
    public List<RoomInfo> roomList = new List<RoomInfo>();
    MainMenuController mainMenuController;


    public void PreInit()
    {
        basePool = new BasePool(content, template, 3);
        basePool.Init();
    }
    public void Init(MainMenuController mainMenuController)
    {
        this.mainMenuController = mainMenuController;

        for (int i = 0; i < roomList.Count; i++) 
        {
            if (roomList[i].PlayerCount == 0) roomList.RemoveAt(i);
        }
        PhotonNetwork.JoinLobby();
    }
    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        PhotonNetwork.JoinLobby();
    }
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log("OnRoomListUpdate");
        foreach (RoomInfo roomInfo in roomList.ToArray())
        {
            if (!this.roomList.Contains(roomInfo))
            {
                if (roomInfo.PlayerCount != 0)
                {
                    basePool.TakeObject().GetComponent<RoomListing>().Init(roomInfo, this, basePool, mainMenuController);
                    this.roomList.Add(roomInfo);
                }
            }
        }
    }
}
