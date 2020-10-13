using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIPanels : MonoBehaviour
{
    private bool isOpen = false;
    public BottomPanel bottomPanel;
    public LeftPanel leftPanel;
    public RightPanel rightPanel;
    public GameObject button;
    public TextMeshProUGUI money;

    private void Start()
    {
        if (GameplayController.instance.gameInitialized)
        {
            ActivateButton();
        }
        else 
        {
            GameplayController.instance.invoker.onExecutionFinished += ActivateButton;
        }
    }

    private void Update()
    {
        if (GameplayController.instance.gameInitialized) money.text = GameplayController.instance.session.FindPlayer(PhotonNetwork.LocalPlayer.NickName).Money.ToString() + " GR";
    }

    private void ActivateButton()
    {
        button.SetActive(true);
    }

    public void OpenBottomPanel() 
    {
        bottomPanel.GetComponent<Animation>().Play("BottomToMiddle");
        bottomPanel.Init(gameObject.GetComponent<UIPanels>(), PhotonNetwork.LocalPlayer);
    }
    public void OpenBottomPanel(Photon.Realtime.Player player)
    { 
        bottomPanel.Init(gameObject.GetComponent<UIPanels>(), player);
    }
    public void OpenLeftPanel()
    {
        leftPanel.Init(gameObject.GetComponent<UIPanels>());
    }

    public void CloseBottomPanel() 
    {
        bottomPanel.Deinit();
        bottomPanel.GetComponent<Animation>().Play("MiddleToBottom");
    }

    public void OpenRightPanel() 
    {
        rightPanel.Init(gameObject.GetComponent<UIPanels>());
    }

}
