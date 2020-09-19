using ExitGames.Client.Photon;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Player
{
    #region Właściwości gracza

    /// <summary>
    /// Sieciowa instancja gracza w photonie.
    /// </summary>
    public Photon.Realtime.Player NetworkPlayer { get; private set; }

    /// <summary>
    /// Ilość tur do pominięcia.
    /// </summary>
    public int TurnsToSkip
    {
        get => (int)NetworkPlayer.CustomProperties["turnsToSkip"];
        private set
        {
            Hashtable table = new Hashtable();
            table.Add("turnsToSkip", value);
            NetworkPlayer.SetCustomProperties(table);
        }
    }

    /// <summary>
    /// Ilość pieniędzy, które posiada gracz.
    /// </summary>
    public float Money
    {
        get => (float)NetworkPlayer.CustomProperties["money"];
        private set
        {
            Hashtable table = new Hashtable();
            table.Add("money", value);
            NetworkPlayer.SetCustomProperties(table);
        }
    }

    /// <summary>
    /// Pole na którym stoi obecnie gracz.
    /// </summary>
    public int PlaceId
    {
        get => (int)NetworkPlayer.CustomProperties["fieldId"];
        set
        {
            Hashtable table = new Hashtable();
            table.Add("fieldId", value);
            NetworkPlayer.SetCustomProperties(table);
        }
    }

    /// <summary>
    /// Lista pól, które posiada gracz. 
    /// </summary>
    private List<int> FieldList
    {
        get
        {
            List<int> list = new List<int>();
            list.AddRange((int[])NetworkPlayer.CustomProperties["fieldList"]);
            return list;
        }
        set
        {
            Hashtable table = new Hashtable();
            table.Add("fieldList", value.ToArray());
            NetworkPlayer.SetCustomProperties(table);
        }
    }

    /// <summary>
    ///  Kolor przydzielony graczowi
    /// </summary>
    public Color MainColor
    {
        get
        {
            //Pobieranie koloru z odpowiednich zmiennych zapisanych na serwerze
            float r = (float)NetworkPlayer.CustomProperties["mainColor_r"];
            float g = (float)NetworkPlayer.CustomProperties["mainColor_g"];
            float b = (float)NetworkPlayer.CustomProperties["mainColor_b"];
            float a = (float)NetworkPlayer.CustomProperties["mainColor_a"];

            return new Color(r, g, b, a);
        }
        set
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

            NetworkPlayer.SetCustomProperties(table);
        }
    }

    /// <summary>
    ///  Kolor przydzielony graczowi służący do podświetlenia aktywnego pola
    /// </summary>
    public Color BlinkColor
    {
        get
        {
            //Pobieranie koloru z odpowiednich zmiennych zapisanych na serwerze
            float r = (float)NetworkPlayer.CustomProperties["blinkColor_r"];
            float g = (float)NetworkPlayer.CustomProperties["blinkColor_g"];
            float b = (float)NetworkPlayer.CustomProperties["blinkColor_b"];
            float a = (float)NetworkPlayer.CustomProperties["blinkColor_a"];

            return new Color(r, g, b, a);
        }
        set
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

            NetworkPlayer.SetCustomProperties(table);
        }
    }

    /// <summary>
    /// Określa, czy gracz przegrał grę.
    /// </summary>
    public bool IsLoser
    {
        get => (bool)NetworkPlayer.CustomProperties["isLoser"];
        set
        {
            Hashtable table = new Hashtable();
            table.Add("isLoser", value);
            NetworkPlayer.SetCustomProperties(table);
        }
    }

    /// <summary>
    /// Określa, czy gracz w ciągu całej rozgrywki wziął kiedykolwiek pożyczkę
    /// </summary>
    public bool TookLoan
    {
        get => (bool)NetworkPlayer.CustomProperties["tookLoan"];
        set
        {
            Hashtable table = new Hashtable();
            table.Add("tookLoan", value);
            NetworkPlayer.SetCustomProperties(table);
        }
    }

    /// <summary>
    /// Kwota pożyczki pozostała do zapłaty
    /// </summary>
    public float OutstandingAmount
    {
        get => (float)NetworkPlayer.CustomProperties["outstandingAmount"];
        set
        {
            Hashtable table = new Hashtable();
            table.Add("outstandingAmount", value);
            NetworkPlayer.SetCustomProperties(table);
        }
    }

    #endregion Właściwości gracza

    /// <summary>
    /// Funckja do inicjalizacji gracza przez osoby, nie będące właścicielem pokoju
    /// </summary>
    /// <param name="networkPlayer">Sieciowa instancja gracza</param>
    public Player(Photon.Realtime.Player networkPlayer)
    {
        this.NetworkPlayer = networkPlayer;
    }

    /// <summary>
    /// Funkcja do inicjalizacja gracza przez właściciela pokoju
    /// </summary>
    /// <param name="networkPlayer">Sieciowa instancja gracza</param>
    /// <param name="mainColor">Główny kolor gracza</param>
    /// <param name="blinkColor">Kolor mrygania gracza</param>
    public Player(Photon.Realtime.Player networkPlayer, Color mainColor, Color blinkColor)
    {
        NetworkPlayer = networkPlayer;
        Money = Keys.Gameplay.START_MONEY;
        TurnsToSkip = 0;
        PlaceId = 0;
        FieldList = new List<int>();
        MainColor = mainColor;
        BlinkColor = blinkColor;
        IsLoser = false;
        TookLoan = false;
        OutstandingAmount = 0;
    }

    /// <summary>
    /// Zwraca nazwę gracza.
    /// </summary>
    /// <returns></returns>
    public string GetName()
    {
        return NetworkPlayer.NickName;
    }

    #region Zarządzanie pieniędzmi

    /// <summary>
    /// Zwięszka kwote pieniędzy o wartość.
    /// </summary>
    /// <param name="value">Wartość zmiany</param>
    public void IncreaseMoney(float value)
    {
        Money += Mathf.Abs(value);
    }

    /// <summary>
    /// Zmniejsza kwote pieniędzy o wartość.
    /// </summary>
    /// <param name="value">Wartość zmiany</param>
    public void DecreaseMoney(float value)
    {
        Money -= Mathf.Abs(value);
    }

    /// <summary>
    /// Przypisuje wartości money podaną wartość.
    /// </summary>
    /// <param name="value">Przypisywana wartość</param>
    public void SetMoney(float value)
    {
        Money = value;
    }

    #endregion Zarządzanie pieniędzmi

    #region Zarządzanie polami

    /// <summary>
    /// Dodaje graczowi pole o danym ID.
    /// </summary>
    /// <param name="fieldId">ID pola</param>
    public void AddOwnership(int fieldId)
    {
        if (!FieldList.Contains(fieldId))
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
        if (FieldList.Contains(fieldId))
        {
            RemoveFromFieldList(fieldId);
        }
    }

    public void ClearOwnership()
    {
        FieldList = new List<int>();
    }

    /// <summary>
    /// Dodanie elementów na listę sieciową.
    /// </summary>
    /// <param name="fieldId"></param>
    private void AddToFieldList(int fieldId)
    {
        int[] array = FieldList.ToArray();
        List<int> list = new List<int>();
        list.AddRange(array);
        list.Add(fieldId);
        FieldList = list;
    }

    /// <summary>
    /// Usuwanie elementów z listy sieciowej.
    /// </summary>
    /// <param name="fieldId"></param>
    private void RemoveFromFieldList(int fieldId)
    {
        int[] array = FieldList.ToArray();
        List<int> list = new List<int>();
        list.AddRange(array);
        list.Remove(fieldId);
        FieldList = list;
    }

    /// <summary>
    /// Sprawdza czy gracz ma pole o danym id.
    /// </summary>
    /// <param name="fieldId"></param>
    /// <returns></returns>
    public bool HasField(int fieldId) => FieldList.Contains(fieldId);

    /// <summary>
    /// Zwraca liste posiadanych przez gracza miejsc na planszy
    /// </summary>
    /// <returns>Lista miejsc na planszy, które posiada gracz</returns>
    public List<int> GetOwnedFields() => FieldList;

    #endregion Zarządzanie polami

    #region Zarządzanie turami

    /// <summary>
    /// Odejmuje jedną turę od ilości tur do pominięcia.
    /// </summary>
    public void SubstractTurnToSkip()
    {
        if (TurnsToSkip > 0)
        {
            TurnsToSkip--;

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
        TurnsToSkip = amountOfTurnsToSkip;
    }

    /// <summary>
    /// Dodaje podaną ilość tur do tur do pominięcia
    /// </summary>
    /// <param name="amountofTurnsToSkip">Ilość tur do pominięcia</param>
    public void AddTurnsToSkip(int amountofTurnsToSkip)
    {
        TurnsToSkip +=amountofTurnsToSkip;
    }

    /// <summary>
    /// Zeruje ilość tur do pominięcia.
    /// </summary>
    public void ResetTurnsToSkip()
    {
        TurnsToSkip = 0;
    }

    #endregion Zarządzanie turami
}