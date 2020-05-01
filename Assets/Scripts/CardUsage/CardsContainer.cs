using System.Collections.Generic;
using UnityEngine;

public class CardsContainer : MonoBehaviour
{
    //Lista oczywiście będzie pobierana z serwera
    [SerializeField] private List<Card> cards;
    [SerializeField] private CardDisplay cardPrefab;
    [SerializeField] private Transform cardsContainer;

    public int cardSpacing = 150;

    private void Start()
    {
        float cardWidth = cardPrefab.GetComponent<RectTransform>().rect.width;
        float allCardsWidth = cardSpacing * (cards.Count - 1) + cardWidth;

        float firstCardPos = (allCardsWidth /2) - (cardWidth / 2);

        displayCards(firstCardPos);
    }

    private void displayCards(float firstCardPos)
    {
        for (int i = 0; i < cards.Count; i++)
        {
            cardPrefab.card = cards[i];
            GameObject tmpObj = Instantiate(cardPrefab, cardsContainer).gameObject;
            tmpObj.transform.localPosition = new Vector3(-firstCardPos + (cardSpacing * i), 0, 0); 
        }
    }
}
