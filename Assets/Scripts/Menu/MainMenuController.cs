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
            panel.GetComponent<IPanelInitable>().PreInit();
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
            panel.SetActive(false);
        }
        //przejscie
        panels[panelId].SetActive(true);
        panels[panelId].GetComponent<IPanelInitable>().Init(this);
        //Animacja
    }
    #endregion Kontrola Paneli

}
