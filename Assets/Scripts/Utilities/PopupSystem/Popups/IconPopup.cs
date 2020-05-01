using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class IconPopup : Popup
{
    /// <summary>
    /// Typ ikony wyświetlanej na IconBox-ie
    /// </summary>
    public IconPopupType iconType { get; private set; }
    /// <summary>
    /// Popup, który zostanie wyświetlony po naciśnięciu IconBox-a
    /// </summary>
    public Popup storedPopup;

    /// <summary>
    /// Inicjowanie popup-u typu Icon
    /// </summary>
    /// <param name="iconType">Typ ikony, jaka jest wyświetlana przez IconBox/param>
    /// <param name="lifeSpan">Czas wyświetlania IconBox-u</param>
    /// <param name="onClick">Akcje wywoływane po naciśnięciu na IconBox</param>
    /// <param name="onClose">Akcje wywoływane po zniszczeniu IconBox-u</param>
    public IconPopup(IconPopupType iconType, float lifeSpan = Keys.Popups.POPUP_MAX_EXISTING_TIME, PopupAction onClose = null, PopupAction onClick = null)
        : base(lifeSpan, onClick, onClose)
    {
        this.iconType = iconType;
    }
}
