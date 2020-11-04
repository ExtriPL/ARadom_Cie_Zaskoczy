using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayPanel : MonoBehaviourPunCallbacks, IInitiable<MainMenuController>
{
    private BasePool basePool;
    public GameObject content;
    public GameObject template;
    public List<RoomListing> roomListings = new List<RoomListing>();
    MainMenuController mainMenuController;


    public void PreInit()
    {
        basePool = new BasePool(content, template, 3);
        basePool.Init();
    }
    public void Init(MainMenuController mainMenuController)
    {
        this.mainMenuController = mainMenuController;

        roomListings.RemoveAll(x => x.roomInfo.RemovedFromList);
        roomListings.RemoveAll(x => x.roomInfo.PlayerCount == x.roomInfo.MaxPlayers);
        roomListings.ForEach((roomListing) =>
        {
            roomListing.Refresh();
        });

        PhotonNetwork.JoinLobby();
    }

    public void DeInit() {}

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) mainMenuController.OpenPanel(1);
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        PhotonNetwork.JoinLobby();
    }
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (RoomInfo roomInfo in roomList)
        {
            if (roomListings.FirstOrDefault(x => x.roomInfo.Equals(roomInfo)) == null)
            {
                if (!roomInfo.RemovedFromList && roomInfo.PlayerCount != roomInfo.MaxPlayers)
                {
                    RoomListing rL = basePool.TakeObject().GetComponent<RoomListing>();
                    rL.Init(roomInfo, this, basePool, mainMenuController);
                    roomListings.Add(rL);
                }
            }
        }
    }
}
