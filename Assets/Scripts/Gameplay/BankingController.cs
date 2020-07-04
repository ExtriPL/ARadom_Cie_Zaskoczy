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
    /// Informacje odebrane w ostatnim evencie OnAuction
    /// </summary>
    private Tuple<string, int, string, float, string> auctionData;

    #region Inicjalizacja

    public void InitBanking()
    {
        auctionPopup = null;
        language = SettingsController.instance.languageController;
        for (int i = 0; i < GameplayController.instance.session.playerCount; i++) bidders.Add(GameplayController.instance.session.FindPlayer(i).GetName()); //Dodawanie wszystkich graczy do licytacji
    }

    public void SubscribeEvents()
    {
        EventManager.instance.onAquiredBuilding += OnAquiredBuilding;
        EventManager.instance.onAuction += OnAuction;
        EventManager.instance.onPlayerQuited += OnPlayerQuit;
        EventManager.instance.onPay += OnPay;
    }

    public void UnsubscribeEvents()
    {
        EventManager.instance.onAquiredBuilding -= OnAquiredBuilding;
        EventManager.instance.onAuction -= OnAuction;
        EventManager.instance.onPlayerQuited -= OnPlayerQuit;
        EventManager.instance.onPay -= OnPay;
    }

    #endregion Inicjalizacja

    #region Funkcje bankowości

    /// <summary>
    /// Funkcja nadająca graczowi budynek
    /// </summary>
    /// <param name="player">Gracz, który dostaje pole</param>
    public void AquireBuilding(Player player, int fieldId)
    {
        if (GameplayController.instance.board.GetField(fieldId) is BuildingField)
        {
            player.AddOwnership(fieldId);
            player.DecreaseMoney(((BuildingField)GameplayController.instance.board.GetField(fieldId)).GetInitialPrice());
            EventManager.instance.SendOnPlayerAquiredBuiding(player.GetName(), fieldId);
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
    /// <param name="price">Kwota sprzedarzy pola</param>
    public void TradeBuilding(Player seller, Player buyer, int placeId, float price)  
    {
        if (GameplayController.instance.board.GetField(placeId) is BuildingField)
        {
            if (seller.HasField(placeId))
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
        if (upgradeBuilding.HasNextTier(GameplayController.instance.board.GetTier(placeId)))
        {
            GameplayController.instance.board.NextTier(placeId);
            player.DecreaseMoney(upgradeBuilding.GetTier(GameplayController.instance.board.GetTier(placeId)).buyPrice);
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
        return !player.TookLoan && (player.Money + Keys.Gameplay.LOAN_AMOUNT >= 0);
    }

    #endregion Funkcje bankowości

    #region Obsługa eventów

    public void OnAquiredBuilding(string playerName, int placeId) 
    {
        if (GameplayController.instance.session.FindPlayer(playerName).NetworkPlayer.IsLocal)
        {
            //popup kupującemu
            string message = language.GetWord("YOU_AQUIRED_BUILDING") + GameplayController.instance.board.GetField(placeId).GetFieldName() + ".";
            InfoPopup popup = new InfoPopup(message, 1.5f);
            PopupSystem.instance.AddPopup(popup);
        }
        else 
        {
            //popup reszcie
            string message = language.GetWord("PLAYER") + playerName + language.GetWord("AQUIRED_BUILDING") + GameplayController.instance.board.GetField(placeId).GetFieldName() + ".";
            InfoPopup popup = new InfoPopup(message, 1.5f);
            PopupSystem.instance.AddPopup(popup);
        }
    }

    private void OnAuction(string playerName, int placeId, string bidder, float bid, string passPlayerName)
    {
        auctionData = Tuple.Create(playerName, placeId, bidder, bid, passPlayerName);
        CloseEarlierAuction();

        if (bidders.Contains(passPlayerName)) bidders.Remove(passPlayerName); //Usuwanie gracza, który pasuje

        if (bidders.Count > 1)
        {
            AuctionFlow(playerName, placeId, bidder, bid, passPlayerName);
        }
        else
        {
            //Po licytacji
            EndAuction(placeId, bid);
        }
    }

    private void OnPlayerQuit(string playerName) 
    {
        if(bidders.Contains(playerName))bidders.Remove(playerName);
        if(raisers.Contains(playerName))raisers.Remove(playerName);
        if (auctionPopup != null && GameplayController.instance.session.roomOwner.IsLocal) EventManager.instance.SendOnAuction(auctionData.Item1, auctionData.Item2, "", auctionData.Item4, playerName);
    }

    private void OnPay(string payerName, string receiverName, float amount)
    {
        if(GameplayController.instance.session.FindPlayer(receiverName).NetworkPlayer.IsLocal)
        {
            string message = language.GetWord("PLAYER") + payerName + language.GetWord("PAID") + amount + language.GetWord("FOR_STAND");

            InfoPopup infoPopup = new InfoPopup(message, 1.5f);

            PopupSystem.instance.AddPopup(infoPopup);
        }
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
    /// </summary>
    private void EndAuction(int placeId, float bid)
    {
        if (auctionPopup != null) auctionPopup.onClose = null;
        if (bidders.Count > 0)
        {
            if (GameplayController.instance.session.FindPlayer(bidders[0]).NetworkPlayer.IsLocal)
            {
                if (raisers.Contains(bidders[0]))
                {
                    GameplayController.instance.arController.GetVisualiser(placeId).onAnimationEnd += delegate { GameplayController.instance.EndTurn(); };
                    AquireBuilding(GameplayController.instance.session.FindPlayer(bidders[0]), placeId);
                }
                else
                {
                    if (GameplayController.instance.session.FindPlayer(bidders[0]).Money >= bid)
                    {
                        string message = SettingsController.instance.languageController.GetWord("DO_YOU_WANT_TO_BUY") + GameplayController.instance.board.GetField(placeId).GetFieldName() + "\n" + SettingsController.instance.languageController.GetWord("PRICE") + ":" + bid + "?";
                        QuestionPopup buyQuestion = new QuestionPopup(message);
                        Popup.PopupAction noAction = delegate (Popup source)
                        {
                            GameplayController.instance.EndTurn();
                            Popup.Functionality.Destroy(buyQuestion).Invoke(buyQuestion);
                        };
                        Popup.PopupAction yesAction = delegate (Popup source)
                        {
                            GameplayController.instance.arController.GetVisualiser(placeId).onAnimationEnd += delegate { GameplayController.instance.EndTurn(); };
                            AquireBuilding(GameplayController.instance.session.localPlayer, placeId);
                            Popup.Functionality.Destroy(buyQuestion).Invoke(buyQuestion);
                        };
                        buyQuestion.AddButton(SettingsController.instance.languageController.GetWord("NO"), noAction);
                        buyQuestion.AddButton(SettingsController.instance.languageController.GetWord("YES"), yesAction);

                        PopupSystem.instance.AddPopup(buyQuestion);
                    }
                }
            }
            else
            {
                InfoPopup auctionEnded = new InfoPopup(SettingsController.instance.languageController.GetWord("AUCTION_ENDED"), 2.5f);
                PopupSystem.instance.AddPopup(auctionEnded);
            }
        }
        else GameplayController.instance.EndTurn();

        PopupSystem.instance.ClosePopup(auctionPopup);
        auctionPopup = null;
        bidders.Clear();
        for (int i = 0; i < GameplayController.instance.session.playerCount; i++) bidders.Add(GameplayController.instance.session.FindPlayer(i).GetName()); //Dodawanie wszystkich graczy do licytacji
    }

    /// <summary>
    /// Funckja sterująca przepływem aukcji
    /// </summary>
    private void AuctionFlow(string playerName, int placeId, string bidder, float bid, string passPlayerName)
    {
        LanguageController lang = SettingsController.instance.languageController;

        //1 linijka:  Licytacja
        //2 linijka: nazwa budynku
        string auctionString = lang.GetWord("AUCTION") + "\n" + GameplayController.instance.board.GetField(placeId).GetFieldName();

        if (bidders.Contains(bidder)) //Jeżeli bidders posiada biddera aka czy ktoś pobił
        {
            // Ostatni gracz jest lokalny: 3 linijka: Ty podbiłeś
            if (GameplayController.instance.session.FindPlayer(bidder).NetworkPlayer.IsLocal) auctionString += lang.GetWord("YOU_RAISED");
            // Ostatni gracz nie jest lokalny 3 linijka: Gracz <nick> podbił
            else auctionString += lang.GetWord("PLAYER") + bidder + lang.GetWord("RAISED");
        }
        else if (playerName != passPlayerName && passPlayerName != "") //Jeżeli ktoś spasował, ponieważ sprawdza czy osoba pasująca nie jest startującym aukcji
        {
            //Jeżeli pasujący gracz jest lokalny 3 linijka: Ty spasowałeś
            if (GameplayController.instance.session.FindPlayer(passPlayerName).NetworkPlayer.IsLocal) auctionString += lang.GetWord("YOU_PASSED");
            //Jeżeli pasujący gracz nie jest lokalny 3 linijka: Gracz <nick> spasował
            else auctionString += lang.GetWord("PLAYER") + passPlayerName + lang.GetWord("PASSED");
        }
        else if (playerName == passPlayerName) //Jeżeli gracz który spasował, jest graczem który zaczął akcję, aka pierwsze wywołanie eventu licytacji
        {
            //Jeżeli gracz jest lokalny 3 linijka : Zacząłeś aukcję
            if (GameplayController.instance.session.FindPlayer(playerName).NetworkPlayer.IsLocal) auctionString += lang.GetWord("YOU_STARTED_AUCTION");
            //Jeżeli gracz nie jest lokalny 3 linijka: gracz <nick> rozpoczął aukcję
            else auctionString += lang.GetWord("PLAYER") + playerName + lang.GetWord("STARTED_AUCTION");
        }
        //4 linijka: Obecna stawka: <bid>
        auctionString += lang.GetWord("AUCTION_CURRENT_BID") + bid;

        auctionPopup = new QuestionPopup(auctionString, 30f);

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

    #endregion Aukcja
}
