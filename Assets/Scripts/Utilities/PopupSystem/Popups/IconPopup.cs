using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class IconPopup : Popup
{
    /// <summary>
    /// Typ ikony wyświetlanej na IconBox-ie
    /// </summary>
    public IconPopupType iconType { get; private set; }
    /// <summary>
    /// Ikona, która zostanie wyświetlona na popup-ie gdy iconType == IconPopupType.FromSprite
    /// </summary>
    public Sprite icon;

    /// <summary>
    /// Inicjowanie popup-u typu Icon
    /// </summary>
    /// <param name="iconType">Typ ikony, jaka jest wyświetlana przez IconBox/param>
    /// <param name="lifeSpan">Czas wyświetlania IconBox-u</param>
    /// <param name="onOpen">Akcje wywoływane po wyświetleniu IconBox-u</param>
    /// <param name="onClick">Akcje wywoływane po naciśnięciu na IconBox</param>
    /// <param name="onClose">Akcje wywoływane po zniszczeniu IconBox-u</param>
    public IconPopup(IconPopupType iconType, float lifeSpan = Keys.Popups.MAX_EXISTING_TIME, PopupAction onOpen = null, PopupAction onClose = null, PopupAction onClick = null)
        : base(lifeSpan, onOpen, onClick, onClose)
    {
        this.iconType = iconType;
    }
}
