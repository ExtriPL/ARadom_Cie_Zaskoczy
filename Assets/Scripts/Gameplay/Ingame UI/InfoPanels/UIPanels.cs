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
            StartPanels();
        else
            GameplayController.instance.invoker.onExecutionFinished += StartPanels;
    }

    private void Update()
    {
        if (GameplayController.instance.gameInitialized) money.text = GameplayController.instance.session.FindPlayer(PhotonNetwork.LocalPlayer.NickName).Money.ToString() + " GR";
    }

    private void StartPanels()
    {
        button.SetActive(true);
        bottomPanel.PreInit();
    }

    public void OpenBottomPanel() 
    {
        bottomPanel.GetComponent<Animation>().Play("BottomToMiddle");
        bottomPanel.Init(this, GameplayController.instance.session.localPlayer);
    }
    public void OpenBottomPanel(Player player)
    { 
        bottomPanel.Init(this, player);
    }
    public void OpenLeftPanel()
    {
        leftPanel.Init(this);
    }

    public void CloseBottomPanel() 
    {
        bottomPanel.DeInit();
        bottomPanel.GetComponent<Animation>().Play("MiddleToBottom");
    }

    public void OpenRightPanel() 
    {
        rightPanel.Init(this);
    }

}
