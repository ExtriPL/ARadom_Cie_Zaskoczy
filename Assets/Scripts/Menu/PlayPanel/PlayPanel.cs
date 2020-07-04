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
    private List<RoomInfo> roomList = new List<RoomInfo>();
    MainMenuController mainMenuController;


    public void PreInit()
    {
        basePool = new BasePool(content, template, 3);
        basePool.Init();
    }
    public void Init(MainMenuController mainMenuController)
    {
        this.mainMenuController = mainMenuController;
        PhotonNetwork.JoinLobby();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (RoomInfo roomInfo in roomList.ToArray())
        {
            if (!this.roomList.Contains(roomInfo))
            {
                basePool.TakeObject().GetComponent<RoomListing>().Init(roomInfo, basePool, mainMenuController);
                this.roomList.Add(roomInfo);
            }
        }
    }
}
