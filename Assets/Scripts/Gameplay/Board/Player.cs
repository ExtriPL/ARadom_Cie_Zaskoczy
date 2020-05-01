using ExitGames.Client.Photon;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Player
{
    /// <summary>
    /// Sieciowa instancja gracza w photonie.
    /// </summary>
    public Photon.Realtime.Player networkPlayer { get; private set; }

    /// <summary>
    /// Ilość tur do pominięcia.
    /// </summary>
    public int turnsToSkip
    {
        get 
        {
            return (int)networkPlayer.CustomProperties["turnsToSkip"];
        } 

        private set
        {
            Hashtable table = new Hashtable();
            table.Add("turnsToSkip", value);
            networkPlayer.SetCustomProperties(table);
        }
    }

    /// <summary>
    /// Ilość pieniędzy, które posiada gracz.
    /// </summary>
    public float money 
    { 
        get
        {
            return (float)networkPlayer.CustomProperties["money"];
        }

        private set
        {
            Hashtable table = new Hashtable();
            table.Add("money", value);
            networkPlayer.SetCustomProperties(table);
        }
    
    }
       
    /// <summary>
    /// Pole na którym stoi obecnie gracz.
    /// </summary>
    public int fieldId
    {
        get
        {
            return (int)networkPlayer.CustomProperties["fieldId"];
        }

        set
        {
            Hashtable table = new Hashtable();
            table.Add("fieldId", value);
            networkPlayer.SetCustomProperties(table);
        }

    }

    /// <summary>
    ///  Kolor przydzielony graczowi
    /// </summary>
    public Color mainColor
    {
        get
        {
            //Pobieranie koloru z odpowiednich zmiennych zapisanych na serwerze
            float r = (float)networkPlayer.CustomProperties["mainColor_r"];
            float g = (float)networkPlayer.CustomProperties["mainColor_g"];
            float b = (float)networkPlayer.CustomProperties["mainColor_b"];
            float a = (float)networkPlayer.CustomProperties["mainColor_a"];

            return new Color(r, g, b, a);
        }
        private set
        {
            float r = value.r;
            float g = value.g;
            float b = value.b;
            float a = value.a;

            Hashtable table = new Hashtable()
            {
                { "mainColor_r", r },
                { "mainColor_g", g },
                { "mainColor_b", b },
                { "mainColor_a", a }
            };

            networkPlayer.SetCustomProperties(table);
        }
    }

    /// <summary>
    ///  Kolor przydzielony graczowi służący do podświetlenia aktywnego pola
    /// </summary>
    public Color blinkColor
    {
        get
        {
            //Pobieranie koloru z odpowiednich zmiennych zapisanych na serwerze
            float r = (float)networkPlayer.CustomProperties["blinkColor_r"];
            float g = (float)networkPlayer.CustomProperties["blinkColor_g"];
            float b = (float)networkPlayer.CustomProperties["blinkColor_b"];
            float a = (float)networkPlayer.CustomProperties["blinkColor_a"];

            return new Color(r, g, b, a);
        }
        private set
        {
            float r = value.r;
            float g = value.g;
            float b = value.b;
            float a = value.a;

            Hashtable table = new Hashtable()
            {
                { "blinkColor_r", r },
                { "blinkColor_g", g },
                { "blinkColor_b", b },
                { "blinkColor_a", a }
            };

            networkPlayer.SetCustomProperties(table);
        }
    }

    /// <summary>
    /// Lista pól, które posiada gracz. 
    /// </summary>
    private List<int> fieldList
    {
        get
        {
            List<int> list = new List<int>();
            list.AddRange((int[])networkPlayer.CustomProperties["fieldList"]);
            return list;
        }

        set
        {
            Hashtable table = new Hashtable();
            table.Add("fieldList", value.ToArray());
            networkPlayer.SetCustomProperties(table);
        }

    }
    
    public Player(Photon.Realtime.Player networkPlayer)
    {
        this.networkPlayer = networkPlayer;
        money = Keys.Gameplay.START_MONEY;
        turnsToSkip = 0;
        fieldId = 0;
        fieldList = new List<int>();
        mainColor = Color.white;
        blinkColor = Color.black;
    }

    public Player(Photon.Realtime.Player networkPlayer, Color mainColor, Color blinkColor)
    {
        this.networkPlayer = networkPlayer;
        money = Keys.Gameplay.START_MONEY;
        turnsToSkip = 0;
        fieldId = 0;
        fieldList = new List<int>();
        this.mainColor = mainColor;
        this.blinkColor = blinkColor;
    }

    /// <summary>
    /// Zwięszka kwote pieniędzy o wartość.
    /// </summary>
    /// <param name="value">Wartość zmiany</param>
    public void IncreaseMoney(float value)
    {
        money += value;
    }

    /// <summary>
    /// Zmniejsza kwote pieniędzy o wartość.
    /// </summary>
    /// <param name="value">Wartość zmiany</param>
    public void DecreaseMoney(float value)
    {
        money -= value;
    }

    /// <summary>
    /// Przypisuje wartości money podaną wartość.
    /// </summary>
    /// <param name="value">Przypisywana wartość</param>
    public void SetMoney(float value)
    {
        money = value;
    }

    /// <summary>
    /// Zwraca nazwę gracza.
    /// </summary>
    /// <returns></returns>
    public string GetName()
    {
        return networkPlayer.NickName;
    }

    /// <summary>
    /// Dodaje graczowi pole o danym ID.
    /// </summary>
    /// <param name="fieldId">ID pola</param>
    public void AddOwnership(int fieldId)
    {
        if (!fieldList.Contains(fieldId))
        {
            AddToFieldList(fieldId);
        }
    }

    /// <summary>
    /// Odbiera graczowi pole o danym ID.
    /// </summary>
    /// <param name="fieldId">ID pola</param>
    public void RemoveOwnership(int fieldId)
    {
        if (fieldList.Contains(fieldId))
        {
            RemoveFromFieldList(fieldId);
        }
    }

    /// <summary>
    /// Dodanie elementów na listę sieciową.
    /// </summary>
    /// <param name="fieldId"></param>
    private void AddToFieldList(int fieldId)
    {
        int[] array = fieldList.ToArray();
        List<int> list = new List<int>();
        list.AddRange(array);
        list.Add(fieldId);
        fieldList = list;
    }

    /// <summary>
    /// Usuwanie elementów z listy sieciowej.
    /// </summary>
    /// <param name="fieldId"></param>
    private void RemoveFromFieldList(int fieldId)
    {
        int[] array = fieldList.ToArray();
        List<int> list = new List<int>();
        list.AddRange(array);
        list.Remove(fieldId);
        fieldList = list;
    }

    /// <summary>
    /// Sprawdza czy gracz ma pole o danym id.
    /// </summary>
    /// <param name="fieldId"></param>
    /// <returns></returns>
    public bool HasField(int fieldId) => fieldList.Contains(fieldId);

    /// <summary>
    /// Odejmuje jedną turę od ilości tur do pominięcia.
    /// </summary>
    public void SubstractTurnToSkip()
    {
        if (turnsToSkip > 0)
        {
            turnsToSkip--;

        }
        else 
        {
            Debug.LogError("Nastąpiła zmniejszenia liczby kolejek do pominięcia dla gracza " + GetName() + " poniżej zera.");
        }
    }

    /// <summary>
    /// Ustawia daną ilość tur do pominięcia.
    /// </summary>
    /// <param name="amountOfTurnsToSkip">Ilość tur do pominięcia.</param>
    public void SetTurnsToSkip(int amountOfTurnsToSkip)
    {
        turnsToSkip = amountOfTurnsToSkip;
    }

    /// <summary>
    /// Dodaje podaną ilość tur do tur do pominięcia
    /// </summary>
    /// <param name="amountofTurnsToSkip">Ilość tur do pominięcia</param>
    public void AddTurnsToSkip(int amountofTurnsToSkip)
    {
        turnsToSkip +=amountofTurnsToSkip;
    }

    /// <summary>
    /// Zeruje ilość tur do pominięcia.
    /// </summary>
    public void ResetTurnsToSkip()
    {
        turnsToSkip = 0;
    }

    public List<int> GetOwnedFields() => fieldList;
}