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

            QuestionPopup startTurn = QuestionPopup.CreateOkDialog(language.GetWord("YOU_ARE_IN_PRISON"));

            Popup.PopupAction diceRoll = delegate (Popup source)
            {
                if(freeingThrows.Contains(board.dice.rollResult))
                {
                    QuestionPopup free = QuestionPopup.CreateOkDialog(language.GetWord("YOU_ARE_FREE"));
                    free.onClose += GameplayController.instance.flow.RollResult();
                    PopupSystem.instance.AddPopup(free);
                }
                else
                {
                    QuestionPopup noFree = QuestionPopup.CreateOkDialog(language.GetWord("NOT_THIS_TIME"));

                    PopupSystem.instance.AddPopup(noFree);
                }
            };

            startTurn.onClose += delegate { PopupSystem.instance.ShowDice(diceRoll); };

            PopupSystem.instance.AddPopup(startTurn);
        }
        else
            base.OnAwake(player, visualiser);
    }
}
