using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[System.Serializable]
public class BankingController : IEventSubscribable
{
    private LanguageController language;

    /// <summary>
    /// Popup przechowujący panel licytacyjny
    /// </summary>
    public QuestionPopup auctionPopup { get; private set; }

    /// <summary>
    /// Lista graczy, którzy aktualnie są w licytacji
    /// </summary>
    private List<string> bidders = new List<string>();
    /// <summary>
    /// Lista graczy, którzy podbili licytacje chociaż raz
    /// </summary>
    private List<string> raisers = new List<string>();
    /// <summary>
    /// Lista graczy, którzy mogą brać udział w aukcji
    /// </summary>
    private List<string> auctionPlayers = new List<string>();
    /// <summary>
    /// Informacje odebrane w ostatnim evencie OnAuction
    /// </summary>
    private Tuple<string, int, string, float, string> auctionData;

    #region Inicjalizacja

    public void InitBanking()
    {
        auctionPopup = null;
        language = SettingsController.instance.languageController;
        //Dodawanie wszystkich graczy do licytacji
        for (int i = 0; i < GameplayController.instance.session.playerCount; i++)
        {
            auctionPlayers.Add(GameplayController.instance.session.FindPlayer(i).GetName());
            bidders.Add(GameplayController.instance.session.FindPlayer(i).GetName());
        }
    }

    public void SubscribeEvents()
    {
        EventManager.instance.onAquiredBuilding += OnAquiredBuilding;
        EventManager.instance.onAuction += OnAuction;
        EventManager.instance.onPlayerQuited += OnPlayerQuit;
        EventManager.instance.onPlayerLostGame += OnPlayerLostGame;
        EventManager.instance.onTurnChanged += OnTurnChanged;
    }

    public void UnsubscribeEvents()
    {
        EventManager.instance.onAquiredBuilding -= OnAquiredBuilding;
        EventManager.instance.onAuction -= OnAuction;
        EventManager.instance.onPlayerQuited -= OnPlayerQuit;
        EventManager.instance.onPlayerLostGame -= OnPlayerLostGame;
        EventManager.instance.onTurnChanged -= OnTurnChanged;
    }

    #endregion Inicjalizacja

    #region Funkcje bankowości

    /// <summary>
    /// Funkcja nadająca graczowi budynek
    /// </summary>
    /// <param name="player">Gracz, który dostaje pole</param>
    public void AquireBuilding(Player player, int placeId)
    {
        if (GameplayController.instance.board.GetField(placeId) is BuildingField)
        {
            player.AddOwnership(placeId);
            player.DecreaseMoney(((BuildingField)GameplayController.instance.board.GetField(placeId)).GetInitialPrice());
            EventManager.instance.SendOnPlayerAquiredBuiding(player.GetName(), placeId);
        }
        else 
        {
            Debug.LogError("Nieprawidłowy typ pola!");
        }
    }

    /// <summary>
    /// Przekazanie pola należącego do gracza seller graczowi buyer
    /// </summary>
    /// <param name="seller">Osoba, która sprzedaje pole</param>
    /// <param name="buyer">Osoba kupująca pole</param>
    /// <param name="placeId">Pole, które jest sprzedawane</param>
    /// <param name="price">Kwota sprzedazy pola</param>
    public void TradeBuilding(Player seller, Player buyer, int placeId, float price)  
    {
        if (GameplayController.instance.board.GetField(placeId) is BuildingField)
        {
            if (seller.HasPlace(placeId))
            {
                if (buyer.Money >= price)
                {
                    buyer.AddOwnership(placeId);
                    buyer.DecreaseMoney(price);
                    seller.RemoveOwnership(placeId);
                    seller.IncreaseMoney(price);
                }
            }
        }
        else
        {
            Debug.LogError("Nieprawidłowy typ pola!");
        }
    }

    /// <summary>
    /// Ulepsza budynek należący do danego gracza i pobiera za to odpowiednią należność
    /// </summary>
    /// <param name="player">Gracz, który ulepsza budynek</param>
    /// <param name="placeId">Numer pola, które jest ulepszane</param>
    public void UpgradeBuilding(Player player, int placeId)
    {
        NormalBuilding upgradeBuilding = GameplayController.instance.board.GetField(placeId) as NormalBuilding;

        //Sprawdzanie, czy budynek zawiera następny tier
        if (upgradeBuilding.HasNextTier(placeId))
        {
            player.DecreaseMoney(GetUpgradePrice(placeId));
            GameplayController.instance.board.NextTier(placeId);
            EventManager.instance.SendOnPlayerUpgradedBuilding(player.GetName(), placeId);
        }
        else Debug.LogError("Budynek nie zawiera następnego tieru");
    }

    /// <summary>
    /// Przelewa pieniądze gracza payer graczowi receiver
    /// </summary>
    /// <param name="payer">Gracz, który przelewa pieniądze</param>
    /// <param name="receiver">Gracz, któremu przelewane są pieniądze</param>
    /// <param name="amount">Ilośc przelewanych pieniędzy</param>
    public void Pay(Player payer, Player receiver, float amount)
    {
        if (payer != null && receiver != null)
        {
            payer.DecreaseMoney(amount);
            receiver.IncreaseMoney(amount);

            EventManager.instance.SendPayEvent(payer.GetName(), receiver.GetName(), amount);

            LanguageController language = SettingsController.instance.languageController;
            string[] message = new string[] { language.PackKey("PLAYER"), payer.GetName(), language.PackKey("PAID"), amount.ToString(), language.PackKey("RADOM_PENNIES") };
            EventManager.instance.SendPopupMessage(message, IconPopupType.Money, receiver);
        }
        else Debug.Log("Płatnik ani płątobiorca nie mogą być nullem");
    }

    /// <summary>
    /// Sprawdza, czy podany gracz może wziąć pożyczkę.
    /// Pożyczki nie da się wziąć, jeżeli jej kwota nie zapewni pozytywnego bilansu na koncie
    /// </summary>
    /// <param name="player">Gracz, o którym chcemy wiedzieć, czy może wziąć pożyczkę</param>
    /// <returns>Możliwość wzięcia pożyczki przez gracza</returns>
    public bool CanTakeLoan(Player player)
    {
        return !player.TookLoan;
    }

    /// <summary>
    /// Zaciąga pożyczkę dla podanego gracza. Pożyczkę można wziąć tylko wtedy, gdy gracz zbankrutował
    /// </summary>
    /// <param name="player">Gracz, który bierze pożyczkę</param>
    public void TakeLoan(Player player)
    {
        if (player != null)
        {
            player.TookLoan = true;
            player.OutstandingAmount = -player.Money;
            player.SetMoney(Keys.Gameplay.LOAN_BUFFER);
        }
        else Debug.LogError("Gracz, który bierze pożyczkę, nie może być nullem");
    }

    /// <summary>
    /// Zwraca koszt ulepszenia pola, które znajduje się pod podanym numerem
    /// </summary>
    /// <param name="placeId">Numer pola, które jest ulepszane</param>
    /// <returns>Cena ulepszenia pola</returns>
    public float GetUpgradePrice(int placeId)
    {
        Field field = GameplayController.instance.board.GetField(placeId);
        if (field is NormalBuilding)
            return (field as NormalBuilding).GetTier(GameplayController.instance.board.GetTier(placeId) + 1).buyPrice;
        else
        {
            Debug.LogError("Pole o id " + placeId + " nie jest polem, które można ulepszyć.");
            return 0;
        }
    }

    #endregion Funkcje bankowości

    #region Obsługa eventów

    public void OnAquiredBuilding(string playerName, int placeId) 
    {
        string message;

        if (GameplayController.instance.session.FindPlayer(playerName).NetworkPlayer.IsLocal)
        {
            //popup kupującemu
            message = language.GetWord("YOU_AQUIRED_BUILDING") + GameplayController.instance.board.GetField(placeId).GetFieldName() + ".";
        }
        else 
        {
            //popup reszcie
            message = language.GetWord("PLAYER") + playerName + language.GetWord("AQUIRED_BUILDING") + GameplayController.instance.board.GetField(placeId).GetFieldName() + ".";
        }

        IconPopup popup = new IconPopup(IconPopupType.NewPlace, message);
        PopupSystem.instance.AddPopup(popup);
    }

    private void OnAuction(string playerName, int placeId, string bidder, float bid, string passPlayerName)
    {
        auctionData = Tuple.Create(playerName, placeId, bidder, bid, passPlayerName);
        CloseEarlierAuction();

        if (bidders.Contains(passPlayerName)) 
            bidders.Remove(passPlayerName); //Usuwanie gracza, który pasuje

        if (bidders.Count > 1)
            AuctionFlow(playerName, placeId, bidder, bid, passPlayerName);
        else
            EndAuction(placeId, bid);
    }

    private void OnPlayerQuit(string playerName) 
    {
        RemoveFromAuction(playerName);
    }

    private void OnPlayerLostGame(string playerName)
    {
        RemoveFromAuction(playerName);
    }

    private void OnTurnChanged(string previousPlayerName, string currentPlayerName)
    {
        if (auctionPopup != null)
            EndAuction(auctionData.Item2, auctionData.Item4, false);
    }

    #endregion Obsługa eventów

    #region Aukcja

    /// <summary>
    /// Zamyka okno ostatniej akcji
    /// </summary>
    private void CloseEarlierAuction()
    {
        //Czyszczenie poprzednio wyświetlonego popup-u
        if (auctionPopup != null)
        {
            auctionPopup.onClose = null;
            PopupSystem.instance.ClosePopup(auctionPopup);
        }
    }

    /// <summary>
    /// Funckja wywoływana, gdy aukcja dobiegnie końca
    /// <param name="offerForLastBidder">Czy zaoferować kuono budynku ostatniemu graczowi, który został w licytacji, a nigdy nie podbijał</param>
    /// </summary>
    private void EndAuction(int placeId, float bid, bool offerForLastBidder = true)
    {
        if (auctionPopup != null) auctionPopup.onClose = null;
        if (bidders.Count > 0)
        {
            string auctionEndedMessage = SettingsController.instance.languageController.GetWord("AUCTION_ENDED");
            IconPopup auctionEnded = new IconPopup(IconPopupType.Auction, auctionEndedMessage);

            if (GameplayController.instance.session.FindPlayer(bidders[0]).NetworkPlayer.IsLocal)
            {
                if (raisers.Contains(bidders[0]))
                    AquireBuilding(GameplayController.instance.session.FindPlayer(bidders[0]), placeId);
                else if (offerForLastBidder && GameplayController.instance.session.FindPlayer(bidders[0]).Money >= bid)
                {
                    string message = SettingsController.instance.languageController.GetWord("DO_YOU_WANT_TO_BUY") + " " + GameplayController.instance.board.GetField(placeId).GetFieldName() + "\n" + SettingsController.instance.languageController.GetWord("PRICE") + ":" + bid + "?";
                    
                    Popup.PopupAction yesAction = delegate (Popup source)
                    {
                        AquireBuilding(GameplayController.instance.session.localPlayer, placeId);
                    };

                    QuestionPopup buyQuestion = QuestionPopup.CreateYesNoDialog(message, yesAction);
                    PopupSystem.instance.AddPopup(buyQuestion);
                }

                PopupSystem.instance.AddPopup(auctionEnded);
            }
            else if(GameplayController.instance.session.playerCount > 2)
                PopupSystem.instance.AddPopup(auctionEnded);
        }

        PopupSystem.instance.ClosePopup(auctionPopup);
        ClearAuction();
    }

    public void ClearAuction()
    {
        auctionPopup = null;
        raisers.Clear();
        bidders = new List<string>(auctionPlayers); //Dodawanie wszystkich graczy do licytacji
    }

    /// <summary>
    /// Funckja sterująca przepływem aukcji
    /// </summary>
    private void AuctionFlow(string playerName, int placeId, string bidder, float bid, string passPlayerName)
    {
        LanguageController lang = SettingsController.instance.languageController;

        //1 linijka:  Licytacja
        //2 linijka: nazwa budynku
        string auctionString = lang.GetWord("AUCTION") + "\n" + GameplayController.instance.board.GetField(placeId).GetFieldName() + "\n";

        if (bidders.Contains(bidder)) //Jeżeli bidders posiada biddera aka czy ktoś pobił
        {
            // Ostatni gracz jest lokalny: 3 linijka: Ty podbiłeś
            if (GameplayController.instance.session.FindPlayer(bidder).NetworkPlayer.IsLocal) auctionString += lang.GetWord("YOU_RAISED");
            // Ostatni gracz nie jest lokalny 3 linijka: Gracz <nick> podbił
            else auctionString += lang.GetWord("PLAYER") + " " + bidder + " "+ lang.GetWord("RAISED");
        }
        else if (playerName != passPlayerName && passPlayerName != "") //Jeżeli ktoś spasował, ponieważ sprawdza czy osoba pasująca nie jest startującym aukcji
        {
            //Jeżeli pasujący gracz jest lokalny 3 linijka: Ty spasowałeś
            if (GameplayController.instance.session.FindPlayer(passPlayerName).NetworkPlayer.IsLocal) auctionString += lang.GetWord("YOU_PASSED");
            //Jeżeli pasujący gracz nie jest lokalny 3 linijka: Gracz <nick> spasował
            else auctionString += lang.GetWord("PLAYER") + " " + passPlayerName + " " + lang.GetWord("PASSED");
        }
        else if (playerName == passPlayerName) //Jeżeli gracz który spasował, jest graczem który zaczął akcję, aka pierwsze wywołanie eventu licytacji
        {
            //Jeżeli gracz jest lokalny 3 linijka : Zacząłeś aukcję
            if (GameplayController.instance.session.FindPlayer(playerName).NetworkPlayer.IsLocal) auctionString += lang.GetWord("YOU_STARTED_AUCTION");
            //Jeżeli gracz nie jest lokalny 3 linijka: gracz <nick> rozpoczął aukcję
            else auctionString += lang.GetWord("PLAYER") + " " + playerName + " " + lang.GetWord("STARTED_AUCTION");
        }
        //4 linijka: Obecna stawka: <bid>
        auctionString += "\n" + lang.GetWord("AUCTION_CURRENT_BID") + bid;

        auctionPopup = new QuestionPopup(auctionString);

        if (!GameplayController.instance.session.FindPlayer(playerName).NetworkPlayer.IsLocal && bidders.Contains(GameplayController.instance.session.localPlayer.GetName()))
        {
            //Wersja dla osób, które mogą licytować
            string passString = lang.GetWord("PASS");
            string raiseString = lang.GetWord("RAISE");

            Popup.PopupAction passAction = delegate (Popup source)
            {
                source.onClose = null;
                EventManager.instance.SendOnAuction(playerName, placeId, "", bid, GameplayController.instance.session.localPlayer.GetName());
            };

            Popup.PopupAction raiseAction = delegate (Popup source)
            {
                source.onClose = null;
                if (!raisers.Contains(GameplayController.instance.session.localPlayer.GetName())) raisers.Add(GameplayController.instance.session.localPlayer.GetName());
                EventManager.instance.SendOnAuction(playerName, placeId, GameplayController.instance.session.localPlayer.GetName(), bid + Keys.Gameplay.RAISE_BID_VALUE, "");
            };

            auctionPopup.AddButton(passString, passAction);
            if (GameplayController.instance.session.localPlayer.Money >= bid + Keys.Gameplay.RAISE_BID_VALUE) auctionPopup.AddButton(raiseString, raiseAction);

            auctionPopup.onClose = passAction;

        }
        PopupSystem.instance.AddPopup(auctionPopup);
    }

    /// <summary>
    /// Usuwa gracza z możliwości udziału w aukcji
    /// </summary>
    /// <param name="playerName">Gracz, którego chcemy usunąć z aukcji</param>
    private void RemoveFromAuction(string playerName)
    {
        if (auctionPlayers.Contains(playerName)) auctionPlayers.Remove(playerName);
        if (bidders.Contains(playerName)) bidders.Remove(playerName);
        if (raisers.Contains(playerName)) raisers.Remove(playerName);
        if (auctionPopup != null && GameplayController.instance.session.roomOwner.IsLocal) 
            EventManager.instance.SendOnAuction(auctionData.Item1, auctionData.Item2, "", auctionData.Item4, playerName);
    }

    #endregion Aukcja
}
