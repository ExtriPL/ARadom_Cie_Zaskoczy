using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "ChanceSpecial", menuName = "ARadom/Field/Special/Chance")]
public class ChanceSpecial : SpecialField
{
    [SerializeField, Tooltip("Lista możliwych do wylosowania kart")]
    private List<ChanceCard> chanceCards = new List<ChanceCard>();

    public override void OnEnter(Player player, PlaceVisualiser visualiser)
    {
        base.OnEnter(player, visualiser);

        if(player.NetworkPlayer.IsLocal)
            ShowRandomCard(player);
    }

    private void ShowRandomCard(Player player)
    {
        List<ChanceCard> cards = new List<ChanceCard>(chanceCards);
        while(cards.Count > 0)
        {
            int index = Random.Range(0, cards.Count - 1);
            ChanceCard card = cards[index];
            cards.RemoveAt(index);

            if(card.IsActive())
            {
                card.OpenCard(player);
                break;
            }
        }
    }
}