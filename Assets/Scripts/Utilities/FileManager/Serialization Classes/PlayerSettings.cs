using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[System.Serializable]
public struct PlayerSettings
{
    /// <summary>
    /// Nick gracza
    /// </summary>
    public string nick;
    /// <summary>
    /// Liczba tur, które ominą gracz
    /// </summary>
    public int turnsToSkip;
    /// <summary>
    /// Ilość pieniędzy posiadana przez gracza
    /// </summary>
    public float money;
    /// <summary>
    /// Numer pola, na którym stoi gracz
    /// </summary>
    public int fieldId;
    /// <summary>
    /// Lista pól, które posiada gracz
    /// </summary>
    public List<int> fieldList;
    //Dodać implementacje GUID
}
