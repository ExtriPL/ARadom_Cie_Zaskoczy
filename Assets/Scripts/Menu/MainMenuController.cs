using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MainMenuController : MonoBehaviour
{

    public List<GameObject> panels = new List<GameObject>();

    public LoadingPanel loadingScreen;

    public void Start()
    {
        SettingsController.instance.Init();
        PreInitPanels();
        OpenPanelWithoutLoading(Panel.LoginPanel);
        Connect();
    }

    private void Update()
    {
    }

    #region Inicjalizacja

    private void PreInitPanels() 
    {
        foreach (GameObject panel in panels) 
        {
            panel.GetComponent<IInitiable<MainMenuController>>().PreInit(this);
        }
    }
    private void Connect()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.GameVersion = Application.version;
        }
    }
    #endregion Inicjalizacja

    #region Input

    public void OnSelectInput(GameObject gameObject) 
    {
        gameObject.SetActive(false);
    }

    public void OnDeselectInput(GameObject gameObject)
    {
        gameObject.SetActive(true);
    }
    #endregion Input

    #region Kontrola Paneli

    private void OpenPanelWithoutLoading(int panelId)
    {
        foreach (GameObject panel in panels)
        {
            if (panel.GetComponent<IInitiable<MainMenuController>>() is IEventSubscribable)
                panel.GetComponent<IEventSubscribable>().UnsubscribeEvents();
            panel.SetActive(false);
        }
        //przejscie
        panels[panelId].SetActive(true);
        if (panels[panelId].GetComponent<IInitiable<MainMenuController>>() is IEventSubscribable)
            panels[panelId].GetComponent<IEventSubscribable>().SubscribeEvents();
        panels[panelId].GetComponent<IInitiable<MainMenuController>>().Init();
    }

    public void OpenPanelWithoutLoading(Panel panelId)
    {
        OpenPanelWithoutLoading((int)panelId);
    }

    public void OpenPanel(int panelId)
    {
        loadingScreen.onLoadingInMiddle += delegate { OpenPanelWithoutLoading(panelId); };
        loadingScreen.StartLoading();
    }

    public void OpenPanel(Panel panelId)
    {
        OpenPanel((int)panelId);
    }

    #endregion Kontrola Paneli
}

[System.Serializable]
public enum Panel
{
    LoginPanel,
    MenuPanel,
    SettingsPanel,
    AuthorsPanel,
    PlayPanel,
    CreateRoomPanel,
    SavePanel,
    PasswordPanel,
    RoomPanel
}
