using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Start", menuName = "ARadom/Field/Special/Start")]
public class StartSpecial : SpecialField
{
    public override void OnPlayerPassby(Player player, PlaceVisualiser visualiser)
    {
        base.OnPlayerPassby(player, visualiser);
        if(player.NetworkPlayer.IsLocal)
            GivePassMoney(player);
    }

    private void GivePassMoney(Player player)
    {
        LanguageController language = SettingsController.instance.languageController;
        string message;

        if (player.OutstandingAmount > 0)
        {
            player.OutstandingAmount -= Keys.Gameplay.PASS_START_MONEY;

            if(player.OutstandingAmount <= 0)
            {
                //Pożyczka została spłacona
                player.IncreaseMoney(-player.OutstandingAmount);
                player.OutstandingAmount = 0;

                message = language.GetWord("YOU_PAID_OFF_YOUR_LOAN");
            }
            else
            {
                //Zostały pieniądze do spłacenia
                message = language.GetWord("START_MONEY_FOR_LOAN_PAYOFF");
            }
        }
        else
        {
            player.IncreaseMoney(Keys.Gameplay.PASS_START_MONEY);
            message = language.GetWord("YOU_RECEIVED") + Keys.Gameplay.PASS_START_MONEY + language.GetWord("FOR_PASSING_START");
        }

        InfoPopup gainMoney = new InfoPopup(message, 1.5f);

        PopupSystem.instance.AddPopup(gainMoney);
    }
}
