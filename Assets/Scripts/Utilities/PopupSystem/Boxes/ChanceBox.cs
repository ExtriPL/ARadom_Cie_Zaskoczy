using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;

public class ChanceBox : PopupBox
{
    public TextMeshProUGUI title, cardContent;

    public override void Init(Popup source)
    {
        base.Init(source);
        LanguageController lang = SettingsController.instance.languageController;

        ChancePopup popup = source as ChancePopup;
        title.text = lang.GetWord(popup.card.GetCardNameKey());
        cardContent.text = lang.GetWord(popup.card.GetCardContentKey());

        boxAnimator.SetTrigger("Show");
    }
}