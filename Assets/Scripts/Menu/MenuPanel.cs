using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MenuPanel : MonoBehaviourPunCallbacks, IInitiable<MainMenuController>
{
    private MainMenuController mainMenuController;
    public TextMeshProUGUI welcomePlayer;


    #region Inicjalizacja
    public void PreInit(MainMenuController mainMenuController) 
    {
        this.mainMenuController = mainMenuController;
    }

    public void Init()
    {
        welcomePlayer.text = SettingsController.instance.languageController.GetWord("WELCOME") +" "+ PhotonNetwork.LocalPlayer.NickName + "!";

        mainMenuController.loadingScreen.EndLoading();
    }

    public void DeInit() {}
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
    #endregion Przyciski
}
