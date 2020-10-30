using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIPanels : MonoBehaviour, IEventSubscribable
{
    public BottomPanel bottomPanel;
    public LeftPanel leftPanel;
    public RightPanel rightPanel;
    public GameObject button;
    public TextMeshProUGUI money;
    public GameObject openMenuButton;
    public GameObject loadingScreen;

    #region Inicjalizacja

    private void Start()
    {
        StartLoadingScreen();
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

        //Przygotowywanie paneli
        bottomPanel.PreInit();
        leftPanel.PreInit();

        //Włączanie przycisku otwierania panelu dolnego
        button.SetActive(true);
        openMenuButton.SetActive(true);

        EndLoadingScreen();

        //Ustawienie wyświetlanej ilości pieniędzy na wartość początkową
        OnPlayerMoneyChanged(GameplayController.instance.session.localPlayer.GetName());
    }

    #endregion

    #region Panel dolny
    /// <summary>
    /// Otwarcie dolnego panelu za pomocą przycisku na dole ekranu
    /// </summary>
    public void OpenBottomPanel() 
    {
        bottomPanel.DeInit(); // Czyszczenie panelu
        bottomPanel.Init(this, GameplayController.instance.session.localPlayer); //Inicjalizacja panelu z danymi lokalnego gracza
        bottomPanel.GetComponent<Animation>().Play("BottomToMiddle"); //animacja
    }

    /// <summary>
    /// Otwarcie dolnego panelu za pomocą funkcji wywołanej z kodu
    /// </summary>
    /// <param name="player">Gracz, którego informacje chcemy wyświetlić</param>
    public void OpenBottomPanel(Player player)
    {
        bottomPanel.DeInit(); //Czyszczenie
        bottomPanel.Init(this, player); //Inicjalizacja panelu z podanym graczem
    }

    /// <summary>
    /// Zamknięcie dolnego panelu
    /// </summary>
    public void CloseBottomPanel()
    {
        bottomPanel.GetComponent<Animation>().Play("MiddleToBottom");
    }

    #endregion

    #region Lewy panel
    /// <summary>
    /// Otwarcie lewego panelu za pomocą przycisku
    /// </summary>
    public void OpenLeftPanel()
    {
        leftPanel.DeInit(); //Czyszczenie
        leftPanel.Init(this); //Inicjalizacja
        leftPanel.GetComponent<Animation>().Play("LeftToMiddle"); //Animacja otwierania lewego panelu
        bottomPanel.GetComponent<Animation>().Play("MiddleToRight"); //Animacja zamykania dolnego panelu
    }
    /// <summary>
    /// Zamykanie lewego panelu za pomocą przycisku
    /// </summary>
    public void CloseLeftPanel()
    {
        OpenBottomPanel(GameplayController.instance.session.localPlayer); //Inicjalizacja panelu dolnego z danymi lokalnego gracza
        leftPanel.GetComponent<Animation>().Play("MiddleToLeft"); //Animacja zamknięcia leweggo panelu
        bottomPanel.GetComponent<Animation>().Play("RightToMiddle"); //Animacja otwarcia dolnego panelu
    }

    /// <summary>
    /// Zamykanie lewego panelu z kodu
    /// </summary>
    /// <param name="player">Gracz, którego informacje chcemy wyświetlić po otwarciu panelu dolnego</param>
    public void CloseLeftPanel(Player player)
    {
        OpenBottomPanel(player); //Inicjalizacja panelu dolnego z danymi przekazanego gracza
        leftPanel.GetComponent<Animation>().Play("MiddleToLeft"); //Animacja zamknięcia lewego panelu
        bottomPanel.GetComponent<Animation>().Play("RightToMiddle"); //Animacja otwarcia dolnego panelu
    }


    #endregion

    #region Prawy panel

    public void OpenRightPanel(Player player) 
    {
        rightPanel.DeInit();
        rightPanel.Init(this, player); //Inicjalizacja panelu prawego z danymi przekazanego gracza
        leftPanel.GetComponent<Animation>().Play("MiddleToLeft"); //Animacja zamknięcia lewego panelu
        rightPanel.GetComponent<Animation>().Play("RightToMiddle"); //Animacja otwarcia prawego panelu
    }


    public void CloseRightPanel() 
    {
        rightPanel.GetComponent<Animation>().Play("MiddleToRight");
        leftPanel.GetComponent<Animation>().Play("LeftToMiddle");
    }

    public void StartLoadingScreen()
    {
        if (!loadingScreen.activeInHierarchy)
        {
            loadingScreen.SetActive(true);
            RectTransform rt = loadingScreen.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, rt.anchorMin.y);
            rt.anchorMax = new Vector2(1, rt.anchorMax.y);
        }
    }

    public void EndLoadingScreen()
    {
        if (loadingScreen.activeInHierarchy)
        {
            loadingScreen.GetComponent<Animation>().Play("MiddleToRight");
            loadingScreen.SetActive(false);
        }
    }


    #endregion

    #region Eventy

    private void OnPlayerMoneyChanged(string playerName)
    {
        GameSession session = GameplayController.instance.session;

        if(session.localPlayer.GetName().Equals(playerName))
            money.text = session.localPlayer.Money.ToString() + " GR";
    }

    #endregion Eventy
}
