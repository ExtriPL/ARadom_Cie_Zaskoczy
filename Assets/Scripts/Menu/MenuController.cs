using Photon.Pun;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject playerNamePanel;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject joiningLobbyPanel;
    [SerializeField] private GameObject AuthorsPanel;
    [SerializeField] private GameObject savePanel;

    [SerializeField] private Button menuJoinButton;

    [SerializeField] private InputField playerNameInput;

    [SerializeField] private TextMeshProUGUI playerName;


    private void Start()
    {
        if(PhotonNetwork.NetworkClientState == Photon.Realtime.ClientState.Leaving)
        {
            JoinMenuOnClick();
        }
    }

    #region Funkcje PHOTON
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to master");

        PhotonNetwork.AutomaticallySyncScene = true;

        menuJoinButton.GetComponentInChildren<Text>().text = "Graj";

        //sprawdza czy nazwa gracza zapisana w player prefs
        if (PlayerPrefs.HasKey("NickName"))
        {
            if (PlayerPrefs.GetString("NickName") == "")
            {
                PhotonNetwork.NickName = "Player#" + Random.Range(0, 9999);
            }
            else
            {
                PhotonNetwork.NickName = PlayerPrefs.GetString("NickName");
            }
        }
        else
        {
            PhotonNetwork.NickName = "Player#" + Random.Range(0, 9999);
        }
        playerNameInput.text = PhotonNetwork.NickName; //aktualizuj input
        playerName.text = PhotonNetwork.NickName;
    }
    #endregion

    #region Inne Funkcje
    public void PlayerNameUpdate(string nameInput)
    {
        PhotonNetwork.NickName = nameInput;
        PlayerPrefs.SetString("NickName", nameInput);
        if (PhotonNetwork.NickName.Length > 1)
        {
            menuJoinButton.GetComponentInChildren<Text>().text = "Graj";
            menuJoinButton.interactable = true;
        }
        else
        {
            menuJoinButton.GetComponentInChildren<Text>().text = "Wprowadź nazwę";
            menuJoinButton.interactable = false;
        }
    }
    #endregion

    #region Przyciski
    public void JoinMenuOnClick()
    {
        if (!PhotonNetwork.IsConnected)
            PhotonNetwork.ConnectUsingSettings();

        playerNamePanel.SetActive(false);
        menuPanel.SetActive(true);
    }

    public void JoinLobbyOnClick()
    {
        PhotonNetwork.JoinLobby();

        menuPanel.SetActive(false);
        joiningLobbyPanel.SetActive(true);
    }

    public void EnterOptionMenuOnClick()
    {
        menuPanel.SetActive(false);
        optionsPanel.SetActive(true);
    }

    public void QuitOptionMenuOnClick()
    {
        menuPanel.SetActive(true);
        optionsPanel.SetActive(false);
    }

    public void EnterAuthorsOnClick()
    {
        menuPanel.SetActive(false);
        AuthorsPanel.SetActive(true);
    }

    public void QuitAuthorsOnClick()
    {
        menuPanel.SetActive(true);
        AuthorsPanel.SetActive(false);
    }

    public void MenuExitOnClick()
    {
        menuPanel.SetActive(false);
        playerNamePanel.SetActive(true);
    }

    public void EnterSaveMenu()
    {
        SaveController.instance.ListItems();
        menuPanel.SetActive(false);
        savePanel.SetActive(true);
    }

    #endregion
}
