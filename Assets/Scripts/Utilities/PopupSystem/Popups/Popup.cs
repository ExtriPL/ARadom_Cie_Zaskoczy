using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class Popup
{
    public delegate void PopupAction(Popup source);

    /// <summary>
    /// Akcje wywoływane, gdy popup zostanie wciśnięty
    /// </summary>
    public PopupAction onClick;
    /// <summary>
    /// Akcje wywoływane, gdy okno przestanie się wyświetlać lub zostanie zamknięte
    /// </summary>
    public PopupAction onClose;
    /// <summary>
    /// Akcje wywoływane, gdy okno zacznie być wyświetlane
    /// </summary>
    public PopupAction onOpen;

    /// <summary>
    /// Czas wyświetlania popup-u
    /// </summary>
    public float lifeSpan { get; private set; }
    /// <summary>
    /// Priorytet wyświetlania Popup-u
    /// </summary>
    public PopupPriority priority = PopupPriority.Normal;

    /// <summary>
    /// Inicjowanie podstawowych właściwości Popup-u
    /// </summary>
    /// <param name="lifeSpan">Czas, przez jaki wyświetlany jest popup-u</param> 
    /// <param name="onOpen">Akcje wywoływane po wyświetleniu popup-u</param>
    /// <param name="onClick">Akcje wywoływane po naciśnięciu na popup</param>
    /// <param name="onClose">Akcje wywoływane po zniszczeniu popup-u</param>
    protected Popup(float lifeSpan = Keys.Popups.MAX_EXISTING_TIME, PopupAction onOpen = null, PopupAction onClose = null, PopupAction onClick = null)
    {
        this.lifeSpan = lifeSpan;
        this.onOpen = onOpen;
        this.onClose = onClose;
        this.onClick = onClick;
    }

    /// <summary>
    /// Funkcja czyszcząca delegaty. Trzeba ją wywołać przed zniszczeniem obiektu
    /// </summary>
    public virtual void ClearDelegates()
    {
        onClick = null;
        onOpen = null;
        onClose = null;
    }

    /// <summary>
    /// Gotowe zestawy funkcjonalności, które można przypisać pod popup-y
    /// </summary>
    public static class Functionality
    {
        /// <summary>
        /// Dodaje przekazany popup do kolejki wyświetlania
        /// </summary>
        /// <param name="popup">Popup, który ma zostać dodany do kolejki wyświetlania</param>
        /// <returns>Akcja dodająca nowy popup do kolejki wyświetlania</returns>
        public static PopupAction Show(Popup popup)
        {
            PopupAction show = delegate (Popup source)
            {
                PopupSystem.instance.AddPopup(popup);
            };

            return show;
        }

        /// <summary>
        /// Zamyka box-a zawierającego przekazany popup
        /// </summary>
        /// <param name="popup">Popup służący do znalezienia box-a, który ma zostać zamknięty</param>
        /// <returns>Akcja zamykająca box-a z podanym popup-em</returns>
        public static PopupAction Destroy(Popup popup)
        {
            PopupAction destroy = delegate (Popup source)
            {
                PopupSystem.instance.ClosePopup(popup);
            };

            return destroy;
        }

        /// <summary>
        /// Dodaje wskazany popup do kolejki wyświetlania i zamyka box-a z popup-em źródłowym
        /// </summary>
        /// <param name="popup">Popup, który ma zostać dodany do kolejki wyświetlania</param>
        /// <returns>Akcja dodająca nowy popup do kolejki wyświetlania i zamykająca popup źródłowy/returns>
        public static PopupAction ShowAndDestroy(Popup popup)
        {
            PopupAction showAndDestroy = delegate (Popup source)
            {
                PopupSystem.instance.AddPopup(popup);
                PopupSystem.instance.ClosePopup(source);
            };

            return showAndDestroy;
        }

        /// <summary>
        /// Wyświetla wiadomość debugowania w konsoli
        /// </summary>
        /// <param name="message">Wiadomość do wyświetlenia przez akcje</param>
        /// <returns>Akcja wyświetlająca wiadomość debugowania w konsoli</returns>
        public static PopupAction DebugMeesage(string message)
        {
            PopupAction debugMessage = delegate (Popup source)
            {
                Debug.Log("Źródło: " + source + "; Wiadomość: " + message);
            };

            return debugMessage;
        }
    }
}
