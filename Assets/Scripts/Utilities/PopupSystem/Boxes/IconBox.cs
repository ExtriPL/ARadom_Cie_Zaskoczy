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

    private readonly Vector2 defaultPosition = new Vector2(-60f, 60f);
    private readonly Vector2 defaultAnchorMin = new Vector2(1f, 0f);
    private readonly Vector2 defaultAnchorMax = new Vector2(1f, 0f);
    private readonly Vector2 defaultSize = new Vector2(100f, 100f);
    private Button button;

    private Animator boxAnimator;

    protected override Action CloseAnimationTrigger => delegate { button.interactable = false; boxAnimator.SetTrigger("Hide"); };

    protected override void Start()
    {
        base.Start();
        button = GetComponent<Button>();
        boxAnimator = GetComponent<Animator>();
    }

    public override void Init(Popup pattern)
    {
        base.Init(pattern);

        IconPopup popup = pattern as IconPopup;
        gameObject.GetComponent<Image>().sprite = MatchSprite(popup.iconType);

        int currentAmount = pSystem.CountShowedPopups(typeof(IconPopup)); //Ilość popupów tego typu przed wyświetleniem
        RectTransform rect = gameObject.transform as RectTransform;
        rect.anchoredPosition = defaultPosition;
        rect.anchorMin = defaultAnchorMin;
        rect.anchorMax = defaultAnchorMax;
        rect.sizeDelta = defaultSize;

        rect.localScale = Vector3.zero;

        button.interactable = false;
        CurrentPosition = currentAmount;
        boxAnimator.SetInteger("currentPosition", currentAmount);
        boxAnimator.SetInteger("targetPosition", currentAmount);
        boxAnimator.SetTrigger("Show");
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
            case IconPopupType.Buy:
                return sprites[1];
            case IconPopupType.Auction:
                return sprites[2]; 
            default: 
                return sprites[0];
        }
    }

    public override void OnShowAnimationEnd()
    {
        base.OnShowAnimationEnd();
        button.interactable = true;
    }

    public override void Reposition()
    {
        base.Reposition();
        List<int> positions = pSystem.GetShowedPositions(GetType());
        int smaller = 0;

        foreach(int position in positions)
        {
            if (position < CurrentPosition)
                smaller++;
        }

        //Jeżeli liczba mniejszych indeksów jest mniejsza, od obecnego indeksu oznacza to, że niżej zrobiło sie miejsce
        int target = smaller < CurrentPosition ? CurrentPosition - 1 : CurrentPosition;
        boxAnimator.SetInteger("targetPosition", target);
    }
}
