using Photon.Pun;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class RightPanel : MonoBehaviour, IEventSubscribable
{
    private BasePool myPool;
    private BasePool theirPool;
    private GameplayController gC;

    public GameObject myContent;
    public GameObject theirContent;

    public GameObject template;

    private UIPanels uIPanels;

    public List<TradeListing> myListings { get; private set; }
    public List<TradeListing> theirListings { get; private set; }

    private LanguageController lC;


    public TextMeshProUGUI mainTitle;
    public TextMeshProUGUI myTitle;
    public TextMeshProUGUI theirTitle;

    public TextMeshProUGUI myCurrentMoney;
    public TextMeshProUGUI theirCurrentMoney;

    public TMP_InputField myMoney;
    public TMP_InputField theirMoney;

    public GameObject acceptDeclineGroup;
    public GameObject sendButton;
    public GameObject closeButton;

    public GameObject addMyBuildingsButton;
    public GameObject addTheirBuildingsButton;

    /// <summary>
    /// Gracz z którym handlujemy
    /// </summary>
    public Player tradingPlayer;

    #region Inicjalizacja
    public void PreInit(UIPanels controller)
    {
        myListings = new List<TradeListing>();
        theirListings = new List<TradeListing>();

        myPool = new BasePool(myContent, template, Keys.Menu.MAX_PLAYERS_COUNT);
        theirPool = new BasePool(theirContent, template, Keys.Menu.MAX_PLAYERS_COUNT);

        myPool.Init();
        theirPool.Init();

        uIPanels = controller;
        gC = GameplayController.instance;
        lC = SettingsController.instance.languageController;
    }
    public void Init(UIPanels uiPanels, Player player)
    {
        tradingPlayer = player;
        OnPlayerMoneyChanged(GameplayController.instance.session.localPlayer.GetName());
        OnPlayerMoneyChanged(tradingPlayer.GetName());
        FillSender(player);
    }

    public void Init(Player player, List<Field> myBuildings, float myMoney, List<Field> theirBuildings, float theirMoney)
    {
        tradingPlayer = player;
        OnPlayerMoneyChanged(GameplayController.instance.session.localPlayer.GetName());
        OnPlayerMoneyChanged(tradingPlayer.GetName());
        FillReceiver(player, myBuildings, myMoney, theirBuildings, theirMoney);
    }

    public void DeInit()
    {
        tradingPlayer = null;

        myListings.ForEach((listing) => {
            listing.DeInit();
        });
        myListings.Clear();

        theirListings.ForEach((listing) => {
            listing.DeInit();
        });
        theirListings.Clear();

        myMoney.text = "0";
        theirMoney.text = "0";
        mainTitle.text = "";
        theirTitle.text = "";
        myTitle.text = "";

    }


    private void FillSender(Player player)
    {
        myMoney.text = "0";
        theirMoney.text = "0";
        mainTitle.text = lC.GetWord("MAKE_AN_OFFER_TO") + player.GetName();
        theirTitle.text = lC.GetWord("PLAYERS_BUILDINGS") + player.GetName();
        myTitle.text = lC.GetWord("MY_BUILDINGS");
        closeButton.SetActive(true);
        sendButton.SetActive(true);
        acceptDeclineGroup.SetActive(false);
        this.myMoney.interactable = true;
        this.theirMoney.interactable = true;
    }

    private void FillReceiver(Player sender, List<Field> myBuildings, float myMoney, List<Field> theirBuildings, float theirMoney)
    {
        mainTitle.text = lC.GetWord("OFFER_FROM") + sender.GetName(); 
        theirTitle.text = lC.GetWord("PLAYERS_BUILDINGS") + sender.GetName();
        myTitle.text = lC.GetWord("MY_BUILDINGS");
        this.myMoney.text = myMoney.ToString();
        this.theirMoney.text = theirMoney.ToString();

        myBuildings.ForEach((building) =>
        {
            TradeListing t = myPool.TakeObject().GetComponent<TradeListing>();
            t.Init(this, myPool, building, true);
            myListings.Add(t);
        });

        theirBuildings.ForEach((building) =>
        {
            TradeListing t = theirPool.TakeObject().GetComponent<TradeListing>();
            t.Init(this, theirPool, building, true);
            theirListings.Add(t);
        });


        this.myMoney.interactable = false;
        this.theirMoney.interactable = false;
        closeButton.SetActive(false);
        addMyBuildingsButton.SetActive(false);
        addTheirBuildingsButton.SetActive(false);
        sendButton.SetActive(false);
        acceptDeclineGroup.SetActive(true);
        acceptDeclineGroup.GetComponentsInChildren<Button>()[0].interactable = true;
        acceptDeclineGroup.GetComponentsInChildren<Button>()[1].interactable = true;
    }

    #endregion

    #region Kontrola formularza

    public void OpenMyBuildings()
    {
        uIPanels.currentOpenPanel = UIPanels.InGameUIPanels.None;
        uIPanels.OpenBottomPanel(GameplayController.instance.session.localPlayer, true);
    }

    public void OpenTheirBuildings()
    {
        uIPanels.currentOpenPanel = UIPanels.InGameUIPanels.None;
        uIPanels.OpenBottomPanel(tradingPlayer, true);
    }

    public void AddToTradeList(Player player, Field field)
    {
        if (player.NetworkPlayer == PhotonNetwork.LocalPlayer && myListings.Find(listing => listing.tradeField == field) == null)
        {
            TradeListing listing = myPool.TakeObject().GetComponent<TradeListing>();
            listing.Init(this, myPool, field);
            myListings.Add(listing);
        }
        else if(theirListings.Find(listing => listing.tradeField == field) == null)
        {
            TradeListing listing = theirPool.TakeObject().GetComponent<TradeListing>();
            listing.Init(this, theirPool, field);
            theirListings.Add(listing);
        }
    }


    public void RemoveFromTradeList(Player player, Field field)
    {
        if (player.NetworkPlayer == PhotonNetwork.LocalPlayer)
        {
            TradeListing tListing = myListings.Find(listing => listing.tradeField == field);
            tListing?.DeInit();
            if (tListing != null) myListings.Remove(tListing);
        }
        else
        {
            TradeListing tListing = theirListings.Find(listing => listing.tradeField == field);
            tListing?.DeInit();
            if(tListing != null) theirListings.Remove(tListing);
        }
    }

    public void SendOffer() 
    {
        //wyslac eventa sieciowego
        string senderNickName = PhotonNetwork.LocalPlayer.NickName;
        string receiverNickName = tradingPlayer.GetName();

        string[] senderBuildingNames = myListings.Select(x => x.tradeField.GetFieldName()).ToArray();
        string[] receiverBuildingNames = theirListings.Select(x => x.tradeField.GetFieldName()).ToArray();

        float senderMoney = int.Parse(myMoney.text);
        float receiverMoney = int.Parse(theirMoney.text);

        if (senderBuildingNames.Length != 0 || senderMoney != 0 || receiverBuildingNames.Length != 0 || receiverMoney != 0)
        {
            sendButton.GetComponent<Button>().interactable = false;
            gameObject.GetComponent<Animation>().Play("MiddleToBottom");
            uIPanels.bottomPanel.GetComponent<Animation>().Play("MiddleToBottom");

            EventManager.instance.SendOnTradeOffer(senderNickName, senderBuildingNames, senderMoney, receiverNickName, receiverBuildingNames, receiverMoney);
            DeInit();
        } 
    }

    public void AcceptOffer() 
    {
        EventManager.instance.SendOnTradeOfferResponse(true, GameplayController.instance.session.localPlayer.GetName(), tradingPlayer.GetName());

        //oddanie moich budynkow graczowi
        myListings.ForEach((listing) => {
            gC.banking.TradeBuilding(gC.session.localPlayer, tradingPlayer, gC.board.GetPlaceIndex(listing.tradeField), 0);
        });

        //przekazanie budynków gracza mi
        theirListings.ForEach((listing) => {
            gC.banking.TradeBuilding(tradingPlayer, gC.session.localPlayer, gC.board.GetPlaceIndex(listing.tradeField), 0);
        });

        //przekazanie pieniedzy gracza do mnie
        tradingPlayer.DecreaseMoney(int.Parse(theirMoney.text));
        gC.session.localPlayer.IncreaseMoney(int.Parse(theirMoney.text));

        //przekazanie moich pieniędzy graczowi
        gC.session.localPlayer.DecreaseMoney(int.Parse(myMoney.text));
        tradingPlayer.IncreaseMoney(int.Parse(myMoney.text));

        ClosePanel();
    }

    public void DeclineOffer() 
    {
        EventManager.instance.SendOnTradeOfferResponse(false, GameplayController.instance.session.localPlayer.GetName(), tradingPlayer.GetName());
        ClosePanel();
    }

    //Zamykanie po zaakceptowaniu albo odrzuceniu oferty
    private void ClosePanel() 
    {
        acceptDeclineGroup.GetComponentsInChildren<Button>()[0].interactable = false;
        acceptDeclineGroup.GetComponentsInChildren<Button>()[1].interactable = false;

        gameObject.GetComponent<Animation>().Play("MiddleToBottom");
        uIPanels.bottomPanel.GetComponent<Animation>().Play("MiddleToBottom");
        DeInit();
    }


    public void OnMyMoneyInput(string input) 
    {
        if (float.Parse(theirMoney.text) >= GameplayController.instance.session.localPlayer.Money)
        {
            sendButton.GetComponent<Button>().interactable = false;
        }
        else
        {
            sendButton.GetComponent<Button>().interactable = true;
        }
    }

    public void OnTheirMoneyInput(string input)
    {
        if (tradingPlayer != null)
        {
            if (int.Parse(theirMoney.text) >= tradingPlayer.Money)
            {
                sendButton.GetComponent<Button>().interactable = false;
            }
            else
            {
                sendButton.GetComponent<Button>().interactable = true;
            }
        }
    }


    #endregion


    #region Eventy
    public void SubscribeEvents()
    {
        EventManager.instance.onPlayerMoneyChanged += OnPlayerMoneyChanged;
    }

    public void UnsubscribeEvents()
    {
        EventManager.instance.onPlayerMoneyChanged -= OnPlayerMoneyChanged;
    }



    private void OnPlayerMoneyChanged(string playerName)
    {
        GameSession session = GameplayController.instance.session;

        if (session.localPlayer.GetName().Equals(playerName))
        {
            myCurrentMoney.text = "/ " + session.localPlayer.Money.ToString() + " GR";
            OnMyMoneyInput(session.localPlayer.Money.ToString());
        }
        if (tradingPlayer.GetName() == playerName)
        {
            theirCurrentMoney.text = "/ " + tradingPlayer.Money.ToString() + " GR";
            OnTheirMoneyInput(tradingPlayer.Money.ToString());
        }
    }

    #endregion
}

