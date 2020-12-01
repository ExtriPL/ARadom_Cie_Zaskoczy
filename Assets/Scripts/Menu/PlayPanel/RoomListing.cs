using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class RoomListing: MonoBehaviourPunCallbacks
{
    public RoomInfo roomInfo;
    private BasePool pool;
    private MainMenuController mainMenuController;
    private PlayPanel playPanel;
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
        if (roomList.Contains(roomInfo))
        {
            roomInfo = roomList.Find(x => x.Equals(roomInfo));

            if (roomInfo.RemovedFromList) Deinit();
            else Refresh();
        }
    }

    public void Deinit() 
    {
        playPanel.roomListings.Remove(this);
        roomInfo = null;
        pool.ReturnObject(gameObject);
    }

    public void Refresh() 
    {
        gameObject.GetComponentInChildren<TextMeshProUGUI>().text = roomInfo.Name;
        gameObject.GetComponentInChildren<TextMeshProUGUI>().text += " ";
        gameObject.GetComponentInChildren<TextMeshProUGUI>().text += roomInfo.PlayerCount;
        gameObject.GetComponentInChildren<TextMeshProUGUI>().text += "/";
        gameObject.GetComponentInChildren<TextMeshProUGUI>().text += roomInfo.MaxPlayers;
        //Dałem tutaj >= bo boniu powiedzial: a co jeżeli będzie więcej osób niż się da?
        if (roomInfo.PlayerCount >= roomInfo.MaxPlayers) Deinit();
    }

    public void JoinRoom() 
    {
        mainMenuController.loadingScreen.onLoadingInMiddle += delegate { StartCoroutine(WaitForLobby()); };
        mainMenuController.loadingScreen.StartLoading();
    }

    private IEnumerator WaitForLobby()
    {
        yield return new WaitUntil(() => PhotonNetwork.IsConnectedAndReady && PhotonNetwork.InLobby);
        Hashtable table = new Hashtable();
        table.Add("Room_PlayerReady", false);
        PhotonNetwork.LocalPlayer.SetCustomProperties(table);
        PhotonNetwork.JoinRoom(roomInfo.Name);
    }

    public override void OnJoinedRoom()
    {
        if (!PhotonNetwork.CurrentRoom.Name.Equals(roomInfo.Name))
            return;

        int sameNamePlayers = PhotonNetwork.CurrentRoom.Players.Where(player => player.Value.NickName.Equals(PhotonNetwork.LocalPlayer.NickName)).Count();
        if (sameNamePlayers > 1)
            PhotonNetwork.LocalPlayer.NickName += (sameNamePlayers - 1).ToString();

        mainMenuController.OpenPanelWithoutLoading(Panel.RoomPanel);  
        mainMenuController.loadingScreen.EndLoading();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
        mainMenuController.OpenPanelWithoutLoading(Panel.PlayPanel);
        mainMenuController.loadingScreen.EndLoading();
    }
}
