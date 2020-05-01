using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class SaveController : MonoBehaviourPunCallbacks
{

    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject savePanel;
    [SerializeField] private GameObject creatingRoomPanel;
    [SerializeField] private GameObject contentList;
    [SerializeField] private GameObject loadButton;
    public static SaveController instance;

    private string roomName;

    private void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

    }

    public void ListItems()
    {
        foreach (RectTransform t in contentList.GetComponentInChildren<RectTransform>()) Destroy(t.gameObject);
        for (int i = 0; i < FileManager.GetSavesName().Count; i++)
        {

            GameObject g = Instantiate(loadButton, contentList.GetComponent<Transform>());
            g.GetComponentInChildren<TextMeshProUGUI>().text = FileManager.GetSavesName()[i];
            g.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -g.GetComponent<RectTransform>().sizeDelta.y * i);
        }
    }

    public void ExitSaveMenu()
    {
        savePanel.SetActive(false);
        menuPanel.SetActive(true);
    }

    /// <summary>
    /// Wczytywanie ustawień gry z pliku
    /// </summary>
    public void LoadGame(TextMeshProUGUI text)
    {
        if (FileManager.DoesSaveFileExist(text.text))
        {
            instance.roomName = text.text;
            PhotonNetwork.JoinLobby(); 

            instance.StartCoroutine(WaitUntilJoinedLobby());

            instance.savePanel.SetActive(false);
            instance.creatingRoomPanel.SetActive(true);
        }
        else
        {
            MessageSystem.instance.AddMessage("<color=red>Nie było pliku gry, który można wczytać</color>", MessageType.MediumMessage);
        }
    }

    IEnumerator WaitUntilJoinedLobby()
    {
        GameSave save = new GameSave();
        FileManager.LoadGame(ref save, instance.roomName);
        RoomOptions roomOptions = new RoomOptions() { IsVisible = true, IsOpen = true, MaxPlayers = (byte)save.players.Count };

        yield return new WaitUntil(() => PhotonNetwork.NetworkClientState == ClientState.JoinedLobby);

        PhotonNetwork.CreateRoom(instance.roomName, roomOptions);

        yield return new WaitUntil(() => PhotonNetwork.NetworkClientState == ClientState.Joined);

        Hashtable table = new Hashtable
        {
            { "loadSavedGame", true },
            { "saveFileName", instance.roomName }
        };

        PhotonNetwork.CurrentRoom.SetCustomProperties(table);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("Błąd podczas tworzenia pokoju");
    }
}
