using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public abstract class BuildingField : Field
{
    [SerializeField, Tooltip("Historia budynku"), TextArea]
    protected string fieldHistory;

    /// <summary>
    /// Zdarzenie wywoływane przy zakupie pola
    /// </summary>
    /// <param name="player">Gracz, który kupił pole</param>
    /// <param name="visualiser">Pole, na którym zostało wywołane zdarzenie</param>
    public virtual void OnBuy(Player player, PlaceVisualiser visualiser) { }

    /// <summary>
    /// Zdarzenie wywoływane przy sprzedaniu pola
    /// </summary>
    /// <param name="player">Gracz, który sprzedał pole</param>
    /// <param name="visualiser">Pole, na którym zostało wywołane zdarzenie</param>
    public virtual void OnSell(Player player, PlaceVisualiser visualiser) { }

    /// <summary>
    /// Zwraca cene najniższej wersji budynku
    /// </summary>
    /// <returns></returns>
    public abstract float GetInitialPrice();

    public override void OnEnter(Player player, PlaceVisualiser visualiser)
    {
        base.OnEnter(player, visualiser);

        if(player.NetworkPlayer.IsLocal)
        { 
            if(GameplayController.instance.board.GetOwner(visualiser.placeIndex) == null)
            {
                //Pole nie ma właściciela
                ShowBuyPopup(player, visualiser);
            }
        }
    }

    /// <summary>
    /// Pokazuje wskazanemu graczowi poppup z pytaniem, czy chce on kupić dane pole
    /// </summary>
    /// <param name="player">Gracz, któremu pokazujemy popup</param>
    /// <param name="visualiser">Instancja visualisera, który przechowuje dane pole</param>
    private void ShowBuyPopup(Player player, PlaceVisualiser visualiser)
    {
        if (player.Money >= GetInitialPrice())
        {
            LanguageController language = SettingsController.instance.languageController;
            //popup o mozliwosci kupienia pola od banku
            string message = language.GetWord("DO_YOU_WANT_TO_BUY") + GetFieldName() + "\n" + language.GetWord("PRICE") + ":" + GetInitialPrice() + "?";

            QuestionPopup wantToBuy = new QuestionPopup(message);
            Popup.PopupAction yesAction = delegate (Popup source)
            {
                //gracz chce kupic pole wiec jest mu przydzielane z banku
                GameplayController.instance.banking.AquireBuilding(player, player.PlaceId);
                source.onClose = null;
                Popup.Functionality.Destroy(wantToBuy).Invoke(source);
            };
            Popup.PopupAction noAction = delegate (Popup source)
            {
                //gracz nie chce kupic pola od banku
                //event wywołujący panel licytacji
                Popup.Functionality.Destroy(wantToBuy).Invoke(source);
            };
            Popup.PopupAction auctionAction = delegate
            {
                EventManager.instance.SendOnAuction(player.GetName(), player.PlaceId, "", GetInitialPrice(), player.GetName());
            };

            string yes = language.GetWord("YES");
            string no = language.GetWord("NO");
            wantToBuy.AddButton(no, noAction);
            wantToBuy.AddButton(yes, yesAction);
            wantToBuy.onClose += auctionAction;
            PopupSystem.instance.AddPopup(wantToBuy);
        }
        else
        {
            //Jeżeli nie stać nas na kupienie budynku, rozpoczyna się licytacja
            EventManager.instance.SendOnAuction(player.GetName(), visualiser.placeIndex, "", GetInitialPrice(), player.GetName());
        }
    }

    protected void ShowPayPopup(Player player, PlaceVisualiser visualiser, float cost)
    {
        LanguageController language = SettingsController.instance.languageController;
        Player owner = GameplayController.instance.board.GetOwner(visualiser.placeIndex);
        //Wiadomość o konieczności zapłaty
        string message = language.GetWord("YOU_MUST_PAY") + owner.GetName() + language.GetWord("FOR_STAY_ON_PLACE") + "\n" + language.GetWord("COST") + cost;

        Popup.PopupAction onClose = delegate (Popup source)
        {
            GameplayController.instance.banking.Pay(player, owner, cost);
        };

        QuestionPopup payPopup = QuestionPopup.CreateOkDialog(message, Popup.Functionality.Destroy());

        payPopup.onClose += onClose;

        PopupSystem.instance.AddPopup(payPopup);
    }

    /// <summary>
    /// Historia budynku, który stoi na danym polu
    /// </summary>
    /// <returns>Historia budynku</returns>
    public string FieldHistory { get => fieldHistory; }
}