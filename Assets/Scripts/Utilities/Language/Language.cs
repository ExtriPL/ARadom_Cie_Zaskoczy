using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Language", menuName = "ARadom/Translation/Language")]

[System.Serializable]
public class Language : ScriptableObject
{
    public Languages language;
    public List<Word> words = new List<Word>();
}
