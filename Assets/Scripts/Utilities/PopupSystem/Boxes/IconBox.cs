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

    private Vector2 defaultPosition;
    private Vector2 defaultAnchorMin;
    private Vector2 defaultAnchorMax;
    private Button button;

    protected override Action CloseAnimationTrigger => delegate { button.interactable = false; animations.Play("IconBoxHide"); };

    protected override void Start()
    {
        base.Start();
        RectTransform rect = gameObject.transform as RectTransform;
        defaultPosition = rect.anchoredPosition;
        defaultAnchorMin = rect.anchorMin;
        defaultAnchorMax = rect.anchorMax;
        button = GetComponent<Button>();
    }

    public override void Init(Popup pattern)
    {
        base.Init(pattern);

        IconPopup popup = pattern as IconPopup;
        gameObject.GetComponent<Image>().sprite = MatchSprite(popup.iconType);

        int currentAmount = pSystem.CountShowedPopups(typeof(IconPopup)); //Ilość popupów tego typu przed wyświetleniem
        RectTransform rect = gameObject.transform as RectTransform;
        rect.anchoredPosition = defaultPosition;
        rect.anchorMax = defaultAnchorMin;
        rect.anchorMax = defaultAnchorMax;

        rect.anchoredPosition = GetPosition(currentAmount);
        rect.localScale = Vector3.zero;

        button.interactable = false;
        animations.Play("IconBoxShow");
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

    private Vector2 GetPosition(int currentAmount)
    {
        RectTransform rect = gameObject.transform as RectTransform;
        Vector2 currentPosition = rect.anchoredPosition;
        Vector2 size = rect.sizeDelta;

        return currentPosition + new Vector2(0f, (currentPosition.y + size.y / 2.0f) * currentAmount);
    }

    public override void OnShowAnimationEnd()
    {
        base.OnShowAnimationEnd();
        button.interactable = true;
    }
}
