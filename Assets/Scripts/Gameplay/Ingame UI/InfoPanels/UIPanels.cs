using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIPanels : MonoBehaviour, IEventSubscribable
{
    private bool isOpen = false;
    public BottomPanel bottomPanel;
    public LeftPanel leftPanel;
    public RightPanel rightPanel;
    public GameObject button;
    public TextMeshProUGUI money;

    private void Start()
    {
        if (GameplayController.instance.GameInitialized)
            StartPanels();
        else
            GameplayController.instance.invoker.onExecutionFinished += StartPanels;
    }

    private void OnDisable()
    {
        UnsubscribeEvents();
    }

    public void SubscribeEvents()
    {
        EventManager.instance.onPlayerMoneyChanged += OnPlayerMoneyChanged;
    }

    public void UnsubscribeEvents()
    {
        EventManager.instance.onPlayerMoneyChanged -= OnPlayerMoneyChanged;
    }

    private void StartPanels()
    {
        SubscribeEvents();
        button.SetActive(true);
        bottomPanel.PreInit();
        leftPanel.PreInit();

        OnPlayerMoneyChanged(GameplayController.instance.session.localPlayer.GetName());
    }

    public void OpenBottomPanel() 
    {
        bottomPanel.DeInit();
        bottomPanel.GetComponent<Animation>().Play("BottomToMiddle");
        bottomPanel.Init(this, GameplayController.instance.session.localPlayer);
    }

    public void OpenBottomPanel(Player player)
    {
        bottomPanel.DeInit();
        bottomPanel.Init(this, player);
        CloseLeftPanel();
    }

    public void OpenLeftPanel()
    {
        leftPanel.DeInit();
        leftPanel.Init(this);
        leftPanel.GetComponent<Animation>().Play("LeftToMiddle");
        bottomPanel.GetComponent<Animation>().Play("MiddleToRight");
    }

    public void CloseBottomPanel() 
    {
        bottomPanel.GetComponent<Animation>().Play("MiddleToBottom");
    }

    public void CloseLeftPanel()
    {
        leftPanel.GetComponent<Animation>().Play("MiddleToLeft");
        bottomPanel.GetComponent<Animation>().Play("RightToMiddle");
    }

    public void OpenRightPanel() 
    {
        rightPanel.Init(this);
    }

    #region Eventy

    private void OnPlayerMoneyChanged(string playerName)
    {
        GameSession session = GameplayController.instance.session;

        if(session.localPlayer.GetName().Equals(playerName))
            money.text = session.localPlayer.Money.ToString() + " GR";
    }

    #endregion Eventy
}
