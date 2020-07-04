using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using Hashtable = ExitGames.Client.Photon.Hashtable;

public class RoomController : MonoBehaviourPunCallbacks
{
    

    #region Panele
    [SerializeField] private GameObject roomPanel;
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private GameObject creatingRoomPanel;
    [SerializeField] private GameObject joiningLobbyPanel;
    [SerializeField] private GameObject joiningRoomPanel;
    #endregion

    [SerializeField] private GameObject loadButton;
    [SerializeField] private GameObject startButton;
    [SerializeField] private GameObject readyButton;

    [SerializeField] private Transform playersContainer;
    [SerializeField] private GameObject playerListingPrefab;

    [SerializeField] private Text roomNameDisplay;  //wyświetla nazwe pokoju

    public override void OnJoinedRoom()
    { 
        roomNameDisplay.text = PhotonNetwork.CurrentRoom.Name;
        if (PhotonNetwork.IsMasterClient)
        {
            ExitGames.Client.Photon.Hashtable table = new ExitGames.Client.Photon.Hashtable();
            table.Add("LevelLoader_startMasterName", PhotonNetwork.MasterClient.NickName);
            PhotonNetwork.CurrentRoom.SetCustomProperties(table);

            PhotonNetwork.CurrentRoom.PlayerTtl = Keys.Session.PLAYER_TTL;
            startButton.SetActive(true);
            startButton.GetComponent<Button>().interactable = false;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            startButton.GetComponent<Button>().interactable = true;
#endif
        }
        else
        {
            readyButton.SetActive(true);
        }
        ClearPlayerListings();
        ListPlayers();

        StartCoroutine(JoinAfterSecond());
    }

    IEnumerator JoinAfterSecond()
    {
        yield return new WaitForSeconds(0.6f);

        lobbyPanel.SetActive(false);

        if (PhotonNetwork.IsMasterClient)
            creatingRoomPanel.SetActive(false);
        else
            joiningRoomPanel.SetActive(false);

        roomPanel.SetActive(true);
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        ClearPlayerListings();
        ListPlayers();

        if (PhotonNetwork.CurrentRoom.PlayerCount > 1)
            startButton.GetComponent<Button>().interactable = true;
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        ClearPlayerListings();
        ListPlayers();

        if (PhotonNetwork.IsMasterClient)
        {
            ExitGames.Client.Photon.Hashtable table = new ExitGames.Client.Photon.Hashtable();
            table.Add("LevelLoader_startMasterName", PhotonNetwork.MasterClient.NickName);
            PhotonNetwork.CurrentRoom.SetCustomProperties(table);

            startButton.SetActive(true);
            readyButton.SetActive(false);
        }

        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
            startButton.GetComponent<Button>().interactable = false;
    }

    

    IEnumerator RejoinLobby()
    {
        yield return new WaitForSeconds(1);
        PhotonNetwork.JoinLobby();
    }

    public void BackOnClick()
    {
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LeaveLobby();

        roomPanel.SetActive(false);
        joiningLobbyPanel.SetActive(true);
        StartCoroutine(RejoinLobby());
    }

    public void ReadyOnClick()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            string playerName = PhotonNetwork.LocalPlayer.NickName;
            playersContainer.GetChild(0).GetComponent<Image>().color = Color.green;
        }
    }

    void ClearPlayerListings()
    {
        for (int i = playersContainer.childCount -1; i >= 0; i--)
        {
            Destroy(playersContainer.GetChild(i).gameObject);
        }
    }

    void ListPlayers()
    {
        foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
        {
            GameObject tempList = Instantiate(playerListingPrefab, playersContainer);
            Text tempText = tempList.transform.GetChild(0).GetComponent<Text>();
            tempText.text = player.NickName;
        }
    }
}