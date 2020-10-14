using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Klasa przechowująca wszystkie informacje o rozgrywce. Służy do serializacji
/// </summary>
[System.Serializable]
public struct GameSave
{
    /// <summary>
    /// Wersja aplikacji, w której został utworzony plik zapisu
    /// </summary>
    public string applicationVersion;
    /// <summary>
    /// Stan rozgrywki
    /// </summary>
    public GameState gameState;
    /// <summary>
    /// Czas, przez jaki toczyła się gra
    /// </summary>
    public float gameTime;
    /// <summary>
    /// Lista ustawień graczy
    /// </summary>
    public List<PlayerSettings> players;
    /// <summary>
    /// Właściwości kostki
    /// </summary>
    public DiceSettings dice;
    /// <summary>
    /// Rozmieszczenie budynków na planszy. Klucz to pozycja na planszy a wartość to para(int + string) określająca co to za pole (int - pozycja na liście pól, string - nazwa pola)
    /// </summary>
    public Dictionary<int, Tuple<int, string>> places;
    /// <summary>
    /// Przypisanie pól na planszy do tierów ich budynków
    /// </summary>
    public Dictionary<int, int> tiers;

    public bool IsCompatible()
    {
        return Application.version == applicationVersion;
    }
}
