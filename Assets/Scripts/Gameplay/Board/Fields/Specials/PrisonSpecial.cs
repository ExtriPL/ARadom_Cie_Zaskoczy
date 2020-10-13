using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Prison", menuName = "ARadom/Field/Special/Prison")]
public class PrisonSpecial : SpecialField
{
    [SerializeField, Tooltip("Lista rzutów, które uwalniają z więzienia")]
    private List<RollResult> freeingThrows = new List<RollResult>();

    public override void OnAwake(Player player, PlaceVisualiser visualiser)
    {
        if(player.NetworkPlayer.IsLocal && player.Imprisoned)
        {
            Board board = GameplayController.instance.board;
            LanguageController language = SettingsController.instance.languageController;

            QuestionPopup startTurn = QuestionPopup.CreateOkDialog(language.GetWord("YOU_ARE_IN_PRISON"), Popup.Functionality.Destroy());

            IconPopup dice = new IconPopup(IconPopupType.None);
            startTurn.onClose += Popup.Functionality.Show(dice);
            dice.onClick += Popup.Functionality.Destroy();
            dice.onClose += delegate (Popup source)
            {
                if(freeingThrows.Contains(board.dice.rollResult))
                {
                    QuestionPopup free = QuestionPopup.CreateOkDialog(language.GetWord("YOU_ARE_FREE"), Popup.Functionality.Destroy());
                    free.onClose += delegate (Popup source2)
                    {
                        int firstThrow = board.dice.last1;
                        int secondThrow = board.dice.last2;
                        InfoPopup rollResult = new InfoPopup(SettingsController.instance.languageController.GetWord("YOU_GOT") + firstThrow + SettingsController.instance.languageController.GetWord("AND") + secondThrow, 1.5f);
                        PopupSystem.instance.AddPopup(rollResult);
                        board.MovePlayer(player, firstThrow + secondThrow);
                    };//Być może trzeba tego delegata uogólnić i wstawić w jedno miejsce zamiast pisać ten sam kod tu i w Field?

                    PopupSystem.instance.AddPopup(free);
                }
                else
                {
                    QuestionPopup noFree = QuestionPopup.CreateOkDialog(language.GetWord("NOT_THIS_TIME"), Popup.Functionality.Destroy());

                    PopupSystem.instance.AddPopup(noFree);
                }
            };

            PopupSystem.instance.AddPopup(startTurn);
        }
        else
            base.OnAwake(player, visualiser);
    }
}
