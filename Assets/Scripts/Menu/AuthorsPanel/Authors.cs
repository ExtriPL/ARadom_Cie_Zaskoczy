using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Authors", menuName = "ARadom/Authors")]

public class Authors : ScriptableObject
{
    [System.Serializable] 
    public enum Profession
    {
        programmer,
        graphic_designer,
        game_designer,
        teacher
    }

    [System.Serializable] 
    public struct Author 
    {
        public string name;
        public string surname;
        public Profession proffesion;
        public string website;
    }

    public List<Author> authors = new List<Author>();
}

