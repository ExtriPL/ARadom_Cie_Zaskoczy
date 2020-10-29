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
    /// Inicjowanie popup-u typu Icon
    /// </summary>
    /// <param name="iconType">Typ ikony, jaka jest wyświetlana przez IconBox/param>
    /// <param name="lifeSpan">Czas wyświetlania IconBox-u</param>
    /// <param name="onOpen">Akcje wywoływane po wyświetleniu IconBox-u</param>
    /// <param name="onClick">Akcje wywoływane po naciśnięciu na IconBox</param>
    /// <param name="onClose">Akcje wywoływane po zniszczeniu IconBox-u</param>
    public IconPopup(IconPopupType iconType, PopupAction onClose = null, AutoCloseMode closeMode = AutoCloseMode.NewAppears)
        : base(closeMode)
    {
        this.iconType = iconType;
        this.onClose = onClose;
        onClick = Functionality.Destroy(); //Icon popup po kliknięciu jest zamykany
    }

    /// <summary>
    /// Inicjowanie popup-u typu Icon
    /// </summary>
    /// <param name="iconType">Typ ikony do wyświetlania</param>
    /// <param name="closeMode">Tryb automatycznego zamknięcia popupu</param>
    /// <param name="toShow">Popup, który ma zostać wyświetlony po kliknięciu ikon boxu</param>
    public IconPopup(IconPopupType iconType, Popup toShow = null, AutoCloseMode closeMode = AutoCloseMode.NewAppears)
        : base(closeMode)
    {
        this.iconType = iconType;
        onClick = Functionality.Destroy();

        if(toShow != null)
            onClose += Functionality.Show(toShow);
    }

    /// <summary>
    /// Inicjowanie popup-u typu Icon
    /// </summary>
    /// <param name="iconType">Typ ikony do wyświetlania</param>
    /// <param name="closeMode">Tryb automatycznego zamknięcia popupu</param>
    /// <param name="message">Wiadomość do wyświetlenia w QuestionPopupie po kliknięciu IconPopup-u</param>
    public IconPopup(IconPopupType iconType, string message, AutoCloseMode closeMode = AutoCloseMode.NewAppears)
        : base(closeMode)
    {
        this.iconType = iconType;
        onClick = Functionality.Destroy();

        QuestionPopup toShow = QuestionPopup.CreateOkDialog(message);
        onClose += Functionality.Show(toShow);
    }
}

public enum IconPopupType
{
    /// <summary>
    /// Brak jakiejkolwiek ikony
    /// </summary>
    None,
    Prison,
    NewPlace,
    Money,
    Trade,
    Key1,
    Key2,
    Auction,
    Mail,
    Medal,
    PlayerLost,
    ChestBrown,
    ChestGold,
    Message,
    Win,
    CastleSilver,
    CastleGolden,
    QuestionMarkGold,
    QuestionMark,
    Dice,
    PlayerLeft
}