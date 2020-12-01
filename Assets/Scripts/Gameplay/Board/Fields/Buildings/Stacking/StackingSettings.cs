using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "StackingSettings", menuName = "ARadom/Field/Building/Stacking/StackingSettings", order = 0)]
public class StackingSettings : ScriptableObject
{
    [SerializeField, Tooltip("Cena zakupu pola")]
    private float buyPrice;
    [SerializeField, Tooltip("Ilość pieniędzy, jaką zapłaci gracz za wejście na pole przy posiadaniu przez właściciela określonej ilości budynków")]
    private List<float> enterCost = new List<float>();  
    [SerializeField, Tooltip("Grupa budynków specjalnych, do których należy pole")]
    private StackingBuildingType stackingType;
    [SerializeField, Tooltip("Klucz służący do przetłumaczenia nazwy grupy")]
    private string translateTypeName;
    public string TranslateTypeName => translateTypeName;
    
    /// <summary>
    /// Cena zakupu pola
    /// </summary>
    public float BuyPrice { get => buyPrice; }

    /// <summary>
    /// Grupa budynków specjalnych, do których należy pole
    /// </summary>
    public StackingBuildingType StackingType { get => stackingType; }

    /// <summary>
    /// Ilość pieniędzy, jaką zapłaci gracz za wejście na pole
    /// </summary>
    /// <param name="buildingCount">Ilość budynków danego typu, które posiada gracz</param>
    /// <returns></returns>
    public float GetEnterCost(int buildingCount)
    {
        int index = Mathf.Clamp(buildingCount - 1, 0, enterCost.Count - 1);
        return enterCost[index];
    }
}

public enum StackingBuildingType
{
    Church,
    Apartment
}