using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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
    public int placedId;
    /// <summary>
    /// Lista pól, które posiada gracz
    /// </summary>
    public List<int> fieldList;
    /// <summary>
    /// Kolor jasny gracza
    /// </summary>
    public float[] blinkColorComponents;
    /// <summary>
    /// Kolor ciemny gracza?
    /// </summary>
    public float[] mainColorComponents;
    /// <summary>
    /// Określa, czy gracz przegrał grę
    /// </summary>
    public bool isLoser;
    /// <summary>
    /// Czy gracz wziął pożyczkę wciągu gry
    /// </summary>
    public bool tookLoan;
    /// <summary>
    /// Kwota pożyczki, która pozostała do zapłaty
    /// </summary>
    public float outstandingAmount;
    /// <summary>
    /// Czy gracz jest obecnie w więzieniu
    /// </summary>
    public bool imprisoned;
}
