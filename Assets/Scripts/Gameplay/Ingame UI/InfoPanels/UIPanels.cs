using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIPanels : MonoBehaviour, IEventSubscribable
{
    ///Panele
    public BottomPanel bottomPanel;
    public LeftPanel leftPanel;
    public RightPanel rightPanel;
    public BuildingInfoPanel buildingInfo;

    public GameObject yourTurnNotification;
    public GameObject button;
    public TextMeshProUGUI money;
    public GameObject openMenuButton;
    public GameObject loadingScreen;
    private GameplayController gC;
    private LanguageController lC;

    public InGameUIPanels currentOpenPanel;

    public enum InGameUIPanels
    {
        BottomPanel,
        BottomPanelBuildingChoice,
        LeftPanel,
        RightPanel,
        BuildingInfoPanel,
        None
    }

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
        EventManager.instance.onTradeOffer += OnTradeOfferReceived;
        EventManager.instance.onTradeOfferResponse += OnTradeResponseReceived;
        EventManager.instance.onTurnChanged += OnTurnChanged;
    }

    public void UnsubscribeEvents()
    {
        EventManager.instance.onPlayerMoneyChanged -= OnPlayerMoneyChanged;
        EventManager.instance.onTradeOffer -= OnTradeOfferReceived;
        EventManager.instance.onTradeOfferResponse -= OnTradeResponseReceived;
        EventManager.instance.onTurnChanged -= OnTurnChanged;
    }

    private void StartPanels()
    {
        gC = GameplayController.instance;
        lC = SettingsController.instance.languageController;
        currentOpenPanel = InGameUIPanels.None;
        SubscribeEvents();

        //Przygotowywanie paneli
        bottomPanel.PreInit();
        leftPanel.PreInit();
        rightPanel.PreInit(this);
        buildingInfo.PreInit(this);

        //Włączanie przycisku otwierania panelu dolnego
        openMenuButton.SetActive(true);
        button.SetActive(true);

        //Ustawienie wyświetlanej ilości pieniędzy na wartość początkową
        OnPlayerMoneyChanged(GameplayController.instance.session.localPlayer.GetName());
        EndLoadingScreen();
    }



    #endregion

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) 
        {
            switch (currentOpenPanel)
            {
                case InGameUIPanels.BottomPanel:
                    CloseBottomPanel();
                    break;
                case InGameUIPanels.BottomPanelBuildingChoice:
                    bottomPanel.ConfirmBuildingSelection();
                    break;
                case InGameUIPanels.LeftPanel:
                    CloseLeftPanel();
                    break;
                case InGameUIPanels.RightPanel:
                    CloseRightPanel();
                    break;
                case InGameUIPanels.BuildingInfoPanel:
                    buildingInfo.Close();
                    break;
                case InGameUIPanels.None:
                    break;
                default:
                    break;
            }
        }
    }

    public void CloseAll() 
    {
        yourTurnNotification.GetComponent<Animation>().Play("NotificationHide");
        while (currentOpenPanel != InGameUIPanels.None) 
        {
            switch (currentOpenPanel)
            {
                case InGameUIPanels.BottomPanel:
                    CloseBottomPanel();
                    break;
                case InGameUIPanels.BottomPanelBuildingChoice:
                    bottomPanel.ConfirmBuildingSelection();
                    break;
                case InGameUIPanels.LeftPanel:
                    CloseLeftPanel();
                    break;
                case InGameUIPanels.RightPanel:
                    CloseRightPanel();
                    break;
                case InGameUIPanels.BuildingInfoPanel:
                    buildingInfo.Close();
                    break;
                case InGameUIPanels.None:
                    break;
                default:
                    break;
            }
        }
        yourTurnNotification.SetActive(false);
    }

    #region Panel dolny
    /// <summary>
    /// Otwarcie dolnego panelu za pomocą przycisku na dole ekranu
    /// </summary>
    public void OpenBottomPanel()
    {
        currentOpenPanel = InGameUIPanels.BottomPanel;
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
        currentOpenPanel = InGameUIPanels.BottomPanel;
        bottomPanel.DeInit(); //Czyszczenie
        bottomPanel.Init(this, player); //Inicjalizacja panelu z podanym graczem
    }

    /// <summary>
    /// Otwarcie dolnego panelu do celów handlu
    /// </summary>
    public void OpenBottomPanel(Player player, bool trading)
    {
        currentOpenPanel = InGameUIPanels.BottomPanelBuildingChoice;
        bottomPanel.DeInit(); //Czyszczenie
        bottomPanel.Init(this, player, trading); //Inicjalizacja panelu z podanym graczem
        rightPanel.GetComponent<Animation>().Play("MiddleToRight");
        bottomPanel.GetComponent<Animation>().Play("LeftToMiddle");
    }


    /// <summary>
    /// Zamknięcie dolnego panelu
    /// </summary>
    public void CloseBottomPanel()
    {
        bottomPanel.GetComponent<Animation>().Play("MiddleToBottom");
        currentOpenPanel = InGameUIPanels.None;
    }

    #endregion

    #region Lewy panel
    /// <summary>
    /// Otwarcie lewego panelu za pomocą przycisku
    /// </summary>
    public void OpenLeftPanel()
    {
        currentOpenPanel = InGameUIPanels.LeftPanel;
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
        currentOpenPanel = InGameUIPanels.RightPanel;
        rightPanel.DeInit();
        rightPanel.Init(this, player); //Inicjalizacja panelu prawego z danymi przekazanego gracza
        leftPanel.GetComponent<Animation>().Play("MiddleToLeft"); //Animacja zamknięcia lewego panelu
        rightPanel.GetComponent<Animation>().Play("RightToMiddle"); //Animacja otwarcia prawego panelu
    }

    public void OpenRightPanel(Player sender, List<Field> myBuildings, float myMoney, List<Field> theirBuildings, float theirMoney) 
    {
        currentOpenPanel = InGameUIPanels.None;
        rightPanel.DeInit();
        rightPanel.Init(sender, myBuildings, myMoney, theirBuildings, theirMoney); //Inicjalizacja panelu prawego z danymi przekazanego gracza
        rightPanel.GetComponent<Animation>().Play("BottomToMiddle");
    }


    public void CloseRightPanel()
    {
        currentOpenPanel = InGameUIPanels.LeftPanel;
        rightPanel.GetComponent<Animation>().Play("MiddleToRight");
        leftPanel.GetComponent<Animation>().Play("LeftToMiddle");
    }

    public void StartLoadingScreen()
    {
        loadingScreen.SetActive(true);
        RectTransform rt = loadingScreen.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, rt.anchorMin.y);
        rt.anchorMax = new Vector2(1, rt.anchorMax.y);
    }

    public void EndLoadingScreen()
    {
        if (loadingScreen.activeInHierarchy)
        {
            loadingScreen.GetComponent<Animation>().Play("MiddleToRight");
            //loadingScreen.SetActive(false);
        }
    }


    #endregion

    #region Eventy

    private void OnPlayerMoneyChanged(string playerName)
    {
        GameSession session = GameplayController.instance.session;

        if (session.localPlayer.GetName().Equals(playerName))
        {
            money.text = session.localPlayer.Money.ToString() + " GR";
            bottomPanel.moneyText.text = session.localPlayer.Money.ToString() + " GR";
        }
    }

    private void OnTradeOfferReceived(string senderNickName, string[] senderBuildingNames, float senderMoney, string receiverNickName, string[] receiverBuildingNames, float receiverMoney) 
    {
        if (receiverNickName == PhotonNetwork.LocalPlayer.NickName) 
        {
            Popup.PopupAction yesAction = delegate
            {
                Player sender = gC.session.FindPlayer(senderNickName);

                List<Field> myBuildings = new List<Field>();
                foreach (string name in receiverBuildingNames) 
                {
                    myBuildings.Add(gC.board.GetField(name));
                }

                Player receiver = gC.session.FindPlayer(receiverNickName);

                List<Field> theirBuildings = new List<Field>();
                foreach (string name in senderBuildingNames)
                {
                    theirBuildings.Add(gC.board.GetField(name));
                }

                OpenRightPanel(sender, myBuildings, receiverMoney, theirBuildings, senderMoney);
            };
            
            IconPopup tradeOfferReceivedPopup = new IconPopup(IconPopupType.Trade, QuestionPopup.CreateYesNoDialog(lC.GetWord("YOU_GOT_AN_OFFER_FROM") + senderNickName + " <br>" + lC.GetWord("DO_YOU_WANT_TO_SEE"), yesAction));
            PopupSystem.instance.AddPopup(tradeOfferReceivedPopup);
        }
    }

    private void OnTradeResponseReceived(bool accepted, string senderNickName, string receiverNickName) 
    {
        string response = accepted ? lC.GetWord("PLAYER") + " " + senderNickName + " " + lC.GetWord("ACCEPTED_THE_OFFER") : lC.GetWord("PLAYER") + " " + senderNickName + " " + lC.GetWord("REJECTED_THE_OFFER");
        IconPopup tradeOfferResponseReceivedPopup = new IconPopup(IconPopupType.Trade, response);

        PopupSystem.instance.AddPopup(tradeOfferResponseReceivedPopup);
    }

    private void OnTurnChanged(string previousPlayerName, string currentPlayerName) 
    {
        if (currentPlayerName == GameplayController.instance.session.localPlayer.GetName() && currentOpenPanel != InGameUIPanels.None) 
        {
            yourTurnNotification.SetActive(true);
            yourTurnNotification.GetComponent<Animation>().Play("NotificationShow");
        }
    }

    #endregion Eventy
}
