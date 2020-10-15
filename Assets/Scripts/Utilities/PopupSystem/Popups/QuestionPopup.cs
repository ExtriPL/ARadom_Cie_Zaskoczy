using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[System.Serializable]
public class QuestionPopup : Popup
{
    /// <summary>
    /// Wiadomość, która jest wyświetlana przez QuestionBox-a
    /// </summary>
    public string message;
    public List<Tuple<string, PopupAction>> buttons { get; private set; }

    /// <summary>
    /// Inicjowanie popup-u typu Question
    /// </summary>
    /// <param name="message">Wiadomość, wyświetlana przez InfoBox</param>
    /// <param name="lifeSpan">Czas wyświetlania InfoBox-u</param>
    /// <param name="onOpen">Akcje wywoływane po wyświetleniu InfoBox-u</param>
    /// <param name="onClick">Akcje wywoływane po naciśnięciu na InfoBox</param>
    /// <param name="onClose">Akcje wywoływane po zniszczeniu InfoBox-u</param>
    public QuestionPopup(string message, PopupAction onOpen = null, PopupAction onClose = null, PopupAction onClick = null)
        : base(AutoCloseMode.NewAppears, onOpen, onClose, onClick)
    {
        this.message = message;
        buttons = new List<Tuple<string, PopupAction>>();
    }

    /// <summary>
    /// Dodaje przycisk, który zostanie wyświetlony przez QuestionBox-a
    /// </summary>
    /// <param name="name"></param>
    /// <param name="clickAction"></param>
    public void AddButton(string name, PopupAction clickAction = null)
    {
        if(buttons.Count >= Keys.Popups.QUESTIONBOX_BUTTONS_AMOUNT)
        {
            Debug.LogError("Nie można dodać więcej przycisków dla QuestionPopup-u o treści: " + message);
            return;
        }

        buttons.Add(Tuple.Create(name, clickAction));
    }

    /// <summary>
    /// Tworzy QuestionPopup z dwoma odpowiedziami: "Tak" i "Nie"
    /// </summary>
    /// <param name="question">Pytanie, które ma zostać wyświetlone użytkownikowi</param>
    /// <param name="yesAction">Akcja wywoływana po wciśnięciu przycisku "Tak"</param>
    /// <param name="noAction">Akcja wywoływana po wciśnięciu przycisku "Nie"</param>
    /// <returns>QuestionPopup z odpowiedziami "Tak" "Nie"</returns>
    public static QuestionPopup CreateYesNoDialog(string question, PopupAction yesAction = null, PopupAction noAction = null)
    {
        QuestionPopup popup = new QuestionPopup(question);
        popup.AddButton(SettingsController.instance.languageController.GetWord("NO"), noAction);
        popup.AddButton(SettingsController.instance.languageController.GetWord("YES"), yesAction);

        return popup;
    }

    /// <summary>
    /// Tworzy QuestionPopup z odpowiedzią "OK"
    /// </summary>
    /// <param name="question">Pytanie, które ma zostać wyświetlone użytkownikowi</param>
    /// <param name="okAction">Akcja wywoływana po naciśnięciu przycisku "Ok"</param>
    /// <returns>>QuestionPopup z odpowiedzią "Ok"</returns>
    public static QuestionPopup CreateOkDialog(string question, PopupAction okAction = null)
    {
        QuestionPopup popup = new QuestionPopup(question);
        if (okAction == null)
            okAction = Functionality.Destroy();
        else
            okAction += Functionality.Destroy();

        popup.AddButton("OK", okAction);

        return popup;
    }
}
