using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardController : MonoBehaviour
{
    public List<Building> buildings = new List<Building>();
    public Dictionary<int, int> places = new Dictionary<int, int>();

    public int placesCount;

    public void Start()
    {
        List<int> usedIndexes = new List<int>();

        //Wsadzanie losowych budynków na kolejne pola
        for(int i = 0; i < placesCount; i++)
        {
            int index = 0;
            do
            {
                index = Random.Range(0, buildings.Count - 1);
            }
            while (usedIndexes.Contains(index));

            usedIndexes.Add(index);

            places.Add(i, index);
        }
    }
}
