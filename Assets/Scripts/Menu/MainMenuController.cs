using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MainMenuController : MonoBehaviour
{

    public List<GameObject> panels = new List<GameObject>();
    public void Start()
    {
        Connect();
        SettingsController.instance.Init();
        PreInitPanels();
        OpenPanel(0);
    }

    private void Update()
    {
    }

    #region Inicjalizacja

    private void PreInitPanels() 
    {
        foreach (GameObject panel in panels) 
        {
            panel.GetComponent<IInitiable<MainMenuController>>().PreInit();
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
    public void OpenPanel(int panelId)
    {
        foreach (GameObject panel in panels) 
        {
            if (panel.GetComponent<IInitiable<MainMenuController>>() is IEventSubscribable) panel.GetComponent<IEventSubscribable>().UnsubscribeEvents();
            panel.SetActive(false);
        }
        //przejscie
        panels[panelId].SetActive(true);
        if (panels[panelId].GetComponent<IInitiable<MainMenuController>>() is IEventSubscribable) panels[panelId].GetComponent<IEventSubscribable>().SubscribeEvents();
        panels[panelId].GetComponent<IInitiable<MainMenuController>>().Init(this);
        //Animacja
    }
    #endregion Kontrola Paneli

}
