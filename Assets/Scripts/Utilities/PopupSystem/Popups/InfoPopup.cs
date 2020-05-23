using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Mathf = UnityEngine.Mathf;

[System.Serializable]
public class InfoPopup : Popup
{
    /// <summary>
    /// Wiadomość, która jest wyświetlana przez InfoBox
    /// </summary>
    public string message { get; private set; }

    /// <summary>
    /// Inicjowanie popup-u typu Info
    /// </summary>
    /// <param name="message">Wiadomość, wyświetlana przez InfoBox</param>
    /// <param name="lifeSpan">Czas wyświetlania InfoBox-u</param>
    /// <param name="onOpen">Akcje wywoływane po wyświetleniu InfoBox-u</param>
    /// <param name="onClick">Akcje wywoływane po naciśnięciu na InfoBox</param>
    /// <param name="onClose">Akcje wywoływane po zniszczeniu InfoBox-u</param>
    public InfoPopup(string message, float lifeSpan = Keys.Popups.MAX_EXISTING_TIME, PopupAction onOpen = null, PopupAction onClose = null, PopupAction onClick = null)
        : base(lifeSpan, onOpen, onClick, onClose)
    {
        this.message = message;
    }
}
