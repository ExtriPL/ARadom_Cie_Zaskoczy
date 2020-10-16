using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginPanel : MonoBehaviourPunCallbacks, IInitiable<MainMenuController>
{
    public Toggle rememberMe;
    public TextMeshProUGUI nickInputText;
    public TMP_InputField nickInput;
    public GameObject connectingText;
    public Button loginButton;
    private MainMenuController mainMenuController;
    private void Start()
    {
    }

    #region Przyciski
    public void OnLogin() 
    {
        if (rememberMe.isOn)
        {
            SettingsController.instance.settings.playerNickname = nickInput.text.Trim();
            SettingsController.instance.SaveSettingsToFile();
        }
        PhotonNetwork.LocalPlayer.NickName = nickInput.text.Trim();
    }

    public void OnNickEnter(string text) 
    {
        loginButton.interactable = ((text.Trim().Length >= Keys.Menu.USERNAME_MIN_LENGTH) && PhotonNetwork.IsConnectedAndReady);
    }

    #endregion Przyciski

    #region Photon
    public override void OnConnectedToMaster()
    {
        loginButton.interactable = (nickInputText.text.Trim().Length >= Keys.Menu.USERNAME_MIN_LENGTH);
        connectingText.SetActive(false);
        if (SettingsController.instance.settings.playerNickname != Keys.Menu.DEFAULT_USERNAME)
        {
            OnLogin();
            mainMenuController.OpenPanel(1);
        }
    }
    #endregion Photon

    #region Inicjalizacja
    public void Init(MainMenuController mainMenuController)
    {
        this.mainMenuController = mainMenuController;
        loginButton.interactable = false;
        if (SettingsController.instance.settings.playerNickname != Keys.Menu.DEFAULT_USERNAME) rememberMe.isOn = true;
        else rememberMe.isOn = false;
        nickInput.text = SettingsController.instance.settings.playerNickname;
        if(!PhotonNetwork.IsConnectedAndReady)connectingText.SetActive(true);
    }

    public void PreInit() {}

    public void DeInit() {}
    #endregion Inicjalizacja
}
