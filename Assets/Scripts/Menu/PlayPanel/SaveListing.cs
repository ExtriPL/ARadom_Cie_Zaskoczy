using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class SaveListing : MonoBehaviourPunCallbacks
{
    public TextMeshProUGUI nameText;
    private string roomName;
    private MainMenuController mainMenuController;
    private BasePool pool;
    public void Init(string name, BasePool pool, MainMenuController mainMenuController)
    {
        roomName = name;
        nameText.text = name;
        this.pool = pool;
        this.mainMenuController = mainMenuController;
    }
    public void LoadGame()
    {
        if (FileManager.DoesSaveFileExist(roomName))
        {
            StartCoroutine(WaitUntilJoinedLobby());
        }
    }

    IEnumerator WaitUntilJoinedLobby()
    {
        GameSave save = new GameSave();
        FileManager.LoadGame(ref save, roomName);
        RoomOptions roomOptions = new RoomOptions() { IsVisible = true, IsOpen = true, MaxPlayers = (byte)save.players.Count };

        if (PhotonNetwork.NetworkClientState != ClientState.JoinedLobby) PhotonNetwork.JoinLobby();

        yield return new WaitUntil(() => PhotonNetwork.NetworkClientState == ClientState.JoinedLobby);

        PhotonNetwork.CreateRoom(roomName, roomOptions);

        yield return new WaitUntil(() => PhotonNetwork.NetworkClientState == ClientState.Joined);


        Hashtable table = new Hashtable
        {
            { "loadSavedGame", true },
            { "saveFileName", roomName }
        };

        PhotonNetwork.CurrentRoom.SetCustomProperties(table);
        mainMenuController.OpenPanel(8);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("Błąd podczas tworzenia pokoju");
    }

    public override void OnDisable()
    {
        roomName = null;
        if(pool != null) pool.ReturnObject(gameObject);
        base.OnDisable();
    }
}
