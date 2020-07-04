using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MenuPanel : MonoBehaviourPunCallbacks, IPanelInitable
{
    private MainMenuController mainMenuController;
    public TextMeshProUGUI welcomePlayer;


    #region Inicjalizacja
    public void Init(MainMenuController mainMenuController)
    {
        this.mainMenuController = mainMenuController;
        welcomePlayer.text = SettingsController.instance.languageController.GetWord("WELCOME") +" "+ PhotonNetwork.LocalPlayer.NickName + "!";
    }
    #endregion Inicjalizacja

    #region Przyciski
    public void Logout() 
    {
        SettingsController.instance.settings.playerNickname = Keys.Menu.DEFAULT_USERNAME;
        SettingsController.instance.SaveSettingsToFile();
        mainMenuController.OpenPanel(0);
    }

    public void Exit() 
    {
    #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
    #else
          Application.Quit();
    #endif
    }

    public void PreInit()
    {
    }
    #endregion Przyciski
}
