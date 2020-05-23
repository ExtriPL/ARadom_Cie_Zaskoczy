using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class IconBox : PopupBox
{
    [SerializeField, Tooltip("Lista sprite-ów, któe mogą zostać wyświetlone jako ikona IconBox-a")]
    private List<Sprite> sprites = new List<Sprite>();

    public override void Init(Popup pattern)
    {
        base.Init(pattern);
        IconPopup popup = pattern as IconPopup;
        gameObject.GetComponent<Image>().sprite = popup.iconType == IconPopupType.FromSprite ? popup.icon : MatchSprite(popup.iconType); //Jeżeli ikona jest określona, ustawia ją. Jeżeli nie, próbuje określić ją na podstawie typu
    }

    /// <summary>
    /// Określa jaki Sprite odpowiada podanemu typowi
    /// </summary>
    /// <param name="iconType">Typ ikony</param>
    /// <returns>Sprite odpowiadający podanemu typowi</returns>
    private Sprite MatchSprite(IconPopupType iconType)
    {
        switch(iconType)
        {
            default: 
                return null;
        }
    }
}
