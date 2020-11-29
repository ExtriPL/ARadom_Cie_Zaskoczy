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
    public TextMeshProUGUI infoText;

    private GameSave save;
    private string saveFileName;
    private MainMenuController mainMenuController;
    private BasePool pool;
    public void Init(string name, BasePool pool, MainMenuController mainMenuController)
    {
        save = new GameSave();
        saveFileName = name;
        FileManager.LoadGame(ref save, name);
        LanguageController lang = SettingsController.instance.languageController;
        string infoString = lang.GetWord("SAVE_LISTING_ROUND") + save.dice.round + " | " + save.date.ToString();

        nameText.text = name;
        infoText.text = infoString;
        this.pool = pool;
        this.mainMenuController = mainMenuController;
    }
    public void LoadGame()
    {
        if (FileManager.DoesSaveFileExist(saveFileName))
        {
            StartCoroutine(WaitUntilJoinedLobby());
        }
    }

    public void RemoveSave()
    {
        FileManager.RemoveGame(saveFileName);
        gameObject.SetActive(false);
    }

    IEnumerator WaitUntilJoinedLobby()
    {
        RoomOptions roomOptions = new RoomOptions() { IsVisible = true, IsOpen = true, MaxPlayers = (byte)save.players.Count };

        if (PhotonNetwork.NetworkClientState != ClientState.JoinedLobby) PhotonNetwork.JoinLobby();

        yield return new WaitUntil(() => PhotonNetwork.NetworkClientState == ClientState.JoinedLobby);

        PhotonNetwork.CreateRoom(save.roomName, roomOptions);

        yield return new WaitUntil(() => PhotonNetwork.NetworkClientState == ClientState.Joined);


        Hashtable table = new Hashtable
        {
            { "loadSavedGame", true },
            { "saveFileName", saveFileName }
        };

        PhotonNetwork.CurrentRoom.SetCustomProperties(table);
        mainMenuController.OpenPanel(Panel.RoomPanel);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("Błąd podczas tworzenia pokoju");
    }

    public override void OnDisable()
    {
        saveFileName = "";
        if(pool != null) pool.ReturnObject(gameObject);
        base.OnDisable();
    }
}
