using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Start", menuName = "ARadom/Field/Special/Start")]
public class StartSpecial : SpecialField
{
    public override List<string> GetFieldInfo()
    {
        return new List<string>();
    }

    public override void OnPlayerEnter(Player player, PlaceVisualiser visualiser)
    {

    }

    public override void OnPlayerLeave(Player player, PlaceVisualiser visualiser)
    {
        
    }

    public override void OnPlayerPassby(Player player, PlaceVisualiser visualiser)
    {
        player.IncreaseMoney(Keys.Gameplay.PASS_START_MONEY);

        LanguageController language = SettingsController.instance.languageController;
        string message = language.GetWord("YOU_RECEIVED") + Keys.Gameplay.PASS_START_MONEY + language.GetWord("FOR_PASSING_START");
        InfoPopup gainMoney = new InfoPopup(message, 1.5f);

        PopupSystem.instance.AddPopup(gainMoney);
    }

    public override void SpecialActions()
    {
        
    }
}
