using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class Popup
{
    public delegate void PopupAction(Popup popup);

    /// <summary>
    /// Akcje wywoływane, gdy popup zostanie wciśnięty
    /// </summary>
    public PopupAction onClick;
    /// <summary>
    /// Akcje wywoływane, gdy okno przestanie się wyświetlać lub zostanie zamknięte
    /// </summary>
    public PopupAction onClose;

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
    protected Popup(float lifeSpan = Keys.Popups.POPUP_MAX_EXISTING_TIME, PopupAction onClose = null, PopupAction onClick = null)
    {
        this.lifeSpan = lifeSpan;
        this.onClose = onClose;
        this.onClick = onClick;
    }

    /// <summary>
    /// Funkcja czyszcząca delegaty. Trzeba ją wywołać przed zniszczeniem obiektu
    /// </summary>
    public virtual void ClearDelegates()
    {
        onClick = null;
        onClose = null;
    }
}
