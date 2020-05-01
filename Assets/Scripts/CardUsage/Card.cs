using UnityEngine;

[CreateAssetMenu(fileName = "New Card", menuName = "ARadom/Card")]
public class Card : ScriptableObject
{
    public Sprite logo;
    public new string name;
    public string descripition;
}
