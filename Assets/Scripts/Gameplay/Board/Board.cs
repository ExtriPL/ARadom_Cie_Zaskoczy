using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Random = UnityEngine.Random;

[System.Serializable]
public class Board : IEventSubscribable
{
    /// <summary>
    /// Lista pól na planszy.
    /// </summary>
    [Tooltip("Lista pól w na planszy.")]
    public List<Field> fields = new List<Field>();

    /// <summary>
    /// Kostka do gry.
    /// </summary>
    public RandomDice dice;

    /// <summary>
    /// Słownik przypisujący miejcu na planszy tier budynku , który na nim stoi
    /// </summary>
    private Dictionary<int, int> tiers 
    {
        get 
        {
            return (Dictionary<int, int>)PhotonNetwork.CurrentRoom.CustomProperties["board_tiers"];
        }
        set
        {
            //Debug.Log("tiers_set");
            Hashtable table = new Hashtable();
            table.Add("board_tiers", value);
            PhotonNetwork.CurrentRoom.SetCustomProperties(table);
        }
    }

    /// <summary>
    /// Słownik przypisujący każdemu miejscu na planszy pole(tj. budynek albo pole specjalne itp.).
    /// </summary>
    public Dictionary<int, int> places
    {
        get
        {
            return (Dictionary<int, int>) PhotonNetwork.CurrentRoom.CustomProperties["board_places"];
        }
        private set
        {
            Hashtable table = new Hashtable();
            table.Add("board_places", value);
            PhotonNetwork.CurrentRoom.SetCustomProperties(table);
        }
    }

    #region Inicjalizacja

    public void Update()
    {

    }

    /// <summary>
    /// Zapisuje odpowiednie ustawienia do obiektu GameSave znajdującego się w klasie GameplayController
    /// </summary>
    public void SaveProgress(ref GameSave save)
    {
        DiceSettings ds = new DiceSettings();
        ds.amountOfRolls = dice.amountOfRolls;
        ds.currentPlayer = dice.currentPlayer;
        ds.last1 = dice.last1;
        ds.last2 = dice.last2;
        ds.round = dice.round;

        save.tiers = tiers;
        save.places = new Dictionary<int, System.Tuple<int, string>>();
        for (int i = 0; i < places.Keys.Count; i++) save.places.Add(i, new System.Tuple<int, string>(places[i], GetField(i).name));

        save.dice = ds;
    }

    /// <summary>
    /// Metoda przypisująca przypadkowe budynki do pól.
    /// </summary>
    public void LoadPlaces()
    {
        if (GameplayController.instance.session.roomOwner.IsLocal)
        {
            List<int> used = new List<int>();
            Dictionary<int, int> places = new Dictionary<int, int>();

            //Start zawsze musi być polem o numerze 0
            int startIndex = GetFieldIndex("Start");
            places.Add(0, startIndex);
            used.Add(startIndex);

            //Sprawdzanie, czy istnieje wystarczająca liczba budynków do rozpoczęcia gry
            if (fields.Count >= Keys.Board.PLACE_COUNT)
            {
                for (int i = 1; i < Keys.Board.PLACE_COUNT; i++)
                {
                    int j;
                    do j = Random.Range(0, fields.Count);
                    while (used.Contains(j) || !fields[j].CanBePlaced());

                    used.Add(j);
                    places.Add(i, j);
                }

                this.places = places;
            }
            else Debug.LogError("Nie ma wystarczającej ilości budynków by zainicjować grę!");
        }
    }

    /// <summary>
    /// Metoda ustawia startowe wartości tierów dla wszystkich budynków na planszy
    /// </summary>
    public void LoadTiers()
    {
        if (GameplayController.instance.session.roomOwner.IsLocal)
        {
            Dictionary<int, int> tiers = new Dictionary<int, int>();
            for (int i = 0; i < places.Count; i++)
            {
                tiers.Add(i, 0);
            }

            this.tiers = tiers;
        }
    }

    /// <summary>
    /// Wczytywanie ustawień z pliku zapisu
    /// </summary>
    /// <param name="save">Obiekt zawierający dane pobrane z pliku zapisu</param>
    public void LoadFromSave(ref GameSave save)
    {
        dice = new RandomDice(save.dice.last1, save.dice.last2, save.dice.currentPlayer, save.dice.amountOfRolls, save.dice.round);

        Dictionary<int, int> places = new Dictionary<int, int>();

        //Przypisywanie budynków odpowiednim miejscą na planszy
        for (int i = 0; i < save.places.Keys.Count; i++)
        {
            //Jeżeli nazwa jest zgodna z oczekiwaniami
            if (fields[save.places[i].Item1].name.Equals(save.places[i].Item2))
            {
                places.Add(i, save.places[i].Item1);
            }
            //Jeżeli nazwa budynku nie zgadza się z indeksem na liście
            else if (GetFieldIndex(save.places[i].Item2) != -1)
            {
                places.Add(i, GetFieldIndex(save.places[i].Item2));
            }

        }
        tiers = save.tiers;
        this.places = places;
    }

    public void SubscribeEvents()
    {
        EventManager.instance.onPlayerQuited += OnPlayerQuit;
    }

    public void UnsubscribeEvents()
    {
        EventManager.instance.onPlayerQuited -= OnPlayerQuit;
    }

    #endregion Inicjalizacja

    #region Właściowości pól na planszy

    /// <summary>
    /// Odnajduje budynek po jego numerze na planszy.
    /// </summary>
    /// <param name="index">Numer pola na planszy</param>
    /// <returns>Pole o podanym numerze.</returns>
    public Field GetField(int index)
    {
        return fields[places[index]];
    }

    /// <summary>
    /// Odnajduje budynek po jego nazwie na liście wszystkich pól
    /// </summary>
    /// <param name="name">Nazwa pola</param>
    /// <returns></returns>
    public Field GetField(string name)
    {
        int fieldIndex = GetFieldIndex(name);
        if (fieldIndex == -1) return null;
        else return fields[fieldIndex];
    }

    /// <summary>
    /// Odnajduje indeks pole na liście wszystkich dostępnych pól
    /// </summary>
    /// <param name="name">Nazwa pola</param>
    /// <returns>Indeks pola o podanej nazwie, jeżeli nie ma takiego pola, zwraca -1</returns>
    public int GetFieldIndex(string name)
    {
        for (int i = 0; i < fields.Count; i++)
        {
            if (fields[i].name.Equals(name)) return i;
        }

        return -1;
    }

    /// <summary>
    /// Metoda wyszukująca numer pola.
    /// </summary>
    /// <param name="field">Obiekt budynku</param>
    /// <returns>Numer pola, na którym znajduje się dany budynek.</returns>
    public int GetPlaceIndex(Field field)
    {
        return places.FirstOrDefault(x => fields[x.Value] == field).Key;
    }

    /// <summary>
    /// Metoda wyszukująca gracza, który jest właścicielem danego pola.
    /// </summary>
    /// <param name="placeId">Pole.</param>
    /// <returns>Obiekt gracza, który jest właścicielem tego co znajduje się na danym polu na planszy. Jeżeli pole nie ma właściciela, zwraca null.</returns>
    public Player GetOwner(int placeId)
    {
        for (int i = 0; i < GameplayController.instance.session.playerCount; i++)
        {
            Player p = GameplayController.instance.session.FindPlayer(i);
            if (p.HasField(placeId))
            {
                return p;
            }
        }
        return null;
    }

    /// <summary>
    /// Zwraca aktualny tier podanego pola
    /// </summary>
    /// <param name="placeId">Numer pola na planszy</param>
    /// <returns>Tier pola</returns>
    public int GetTier(int placeId)
    {
        return tiers[placeId];
    }

    /// <summary>
    /// Oblicza dystans jaki jest między polami
    /// </summary>
    /// <param name="placeFrom">Numer miejsca, z którego chcemy obliczyć dystans</param>
    /// <param name="placeTo">Numer miejsca, do którego chcemy obliczyć dystans</param>
    /// <returns>Oblicza dystans między polami zgodnie z kierunkiem chodzenia po planszy. Zwraca -1 jeżeli wystąpi błąd</returns>
    public int GetPlacesDistance(int placeFrom, int placeTo)
    {
        if (placeFrom.IsBetween(0, Keys.Board.PLACE_COUNT - 1) && placeTo.IsBetween(0, Keys.Board.PLACE_COUNT - 1))
        {
            if (placeTo > placeFrom) return placeTo - placeFrom;
            else return Keys.Board.PLACE_COUNT - placeFrom + placeTo;
        }
        else Debug.LogError("Numery pól wykraczają poza zakres planszy");

        return -1;
    }

    /// <summary>
    /// Oblicza, które pola znajdują się pomiędzy podanymi
    /// </summary>
    /// <param name="placeFrom">Miejsce, z którego chcemy rozpocząć liczenie pól</param>
    /// <param name="placeTo">Miejsce, na którym chcemy zakończyć liczenie pól</param>
    /// <returns></returns>
    public List<int> GetBetweenPlaces(int placeFrom, int placeTo)
    {
        List<int> placesBetween = new List<int>();

        if (placeFrom.IsBetween(0, Keys.Board.PLACE_COUNT - 1) && placeTo.IsBetween(0, Keys.Board.PLACE_COUNT - 1))
        {
            if (GetPlacesDistance(placeTo, placeFrom) > 1)
            {
                if (placeTo > placeFrom) placesBetween = Enumerable.Range(placeFrom + 1, placeTo - placeFrom).ToList();
                else
                {
                    if (placeFrom + 1 < Keys.Board.PLACE_COUNT) placesBetween.AddRange(Enumerable.Range(placeFrom + 1, Keys.Board.PLACE_COUNT - (placeFrom + 1)));
                    if (placeTo > 0) placesBetween.AddRange(Enumerable.Range(0, placeTo + 1));
                }
            }
        }
        else Debug.LogError("Numery pól wykraczają poza zakres planszy");

        return placesBetween;
    }

    /// <summary>
    /// Zlicza liczbe pól o podanym typie w posiadaniu podanego gracza
    /// </summary>
    /// <param name="owner">Osoba, której pola liczymy</param>
    /// <param name="fieldType">Typ pól, które liczymy</param>
    /// <returns>Liczba pól o podanym typie w posiadaniu podanego gracza</returns>
    public int CountPlacesOfType(Player owner, Type fieldType)
    {
        int amount = 0;

        foreach (int fieldId in owner.GetOwnedFields())
            amount += GetField(fieldId).GetType().Equals(fieldType) ? 1 : 0;

        return amount;
    }

    /// <summary>
    /// Zlicza liczbę pól w posiadaniu właściciela i typie podanego pola
    /// </summary>
    /// <param name="placeId">Numer pola, które chcemy sprawdzić</param>
    /// <returns>Ilość pól o typie podanego w posiadaniu właściciela</returns>
    public int CountPlacesOfType(int placeId)
    {
        Player owner = GetOwner(placeId);
        Type fieldType = GetField(placeId).GetType();

        return CountPlacesOfType(owner, fieldType);
    }

    #endregion Właściowości pól na planszy

    #region Sterowanie rozgrywką

    /// <summary>
    /// Metoda przesuwająca gracza o daną ilość pól.
    /// </summary>
    /// <param name="player">Gracz</param>
    /// <param name="amount">Ilośc pól</param>
    public void MovePlayer(Player player, int amount)
    {
        //Jeżeli numer pola po wykonaniu ruchu przekroczy ilość pól na planszy - 1 (indeks ostatniego pola), gracz zostanie przeniesiony na odpowiedznie pole na początku planszy.
        //Np: gracz stoi na polu 10, przesuwamy go o 5 pól, skończy na polu 3.
        int toFieldIndex = (player.PlaceId + amount > Keys.Board.PLACE_COUNT - 1) ? (player.PlaceId + amount) - (Keys.Board.PLACE_COUNT - 1) : player.PlaceId + amount;   

        MovePlayerTo(player, toFieldIndex);
    }

    /// <summary>
    /// Przesuwa gracza na określone pole
    /// </summary>
    /// <param name="player">Gracz, którego chcemy przesunąć</param>
    /// <param name="placeId">Miejsce, na które chcemy przesunąć gracza</param>
    public void MovePlayerTo(Player player, int placeId)
    {
        int fromPlaceId = player.PlaceId;
        player.PlaceId = placeId;

        EventManager.instance.SendOnPlayerMoved(player.GetName(), fromPlaceId, placeId);
    }

    /// <summary>
    /// Teleportuje gracza na podane miejsce
    /// </summary>
    /// <param name="player">Gracz, którego teleportujemy</param>
    /// <param name="placeId">Miejsce, na które teleportujemy gracza</param>
    public void TeleportPlayer(Player player, int placeId)
    {
        int fromPlaceId = player.PlaceId;
        player.PlaceId = placeId;

        EventManager.instance.SendOnPlayerTeleported(player.GetName(), fromPlaceId, placeId);
    }

    /// <summary>
    /// Metoda podwyższająca numer tieru budynku stającego na podanym polu
    /// </summary>
    /// <param name="placeId">Numer pola, na którym stoi docelowy budynek</param>
    public void NextTier(int placeId)
    {
        if (GetField(placeId) is NormalBuilding)
        {
            //By przypisanie wartości zadziałało na słowniku, trzeba go wyciągnąć z sieci, zmodyfikować i znów wsadzić
            Dictionary<int, int> tiers = new Dictionary<int, int>(this.tiers);
            tiers[placeId]++;
            this.tiers = new Dictionary<int, int>(tiers);
        }
        else
        {
            Debug.LogError("Wywołano metodę NextTier(), dla pola kltóre nie ma tierów!");
        }
    }

    public void SetTier(int placeId, int tier)
    {
        if (GetField(placeId) is NormalBuilding)
        {
            Dictionary<int, int> tiers = new Dictionary<int, int>(this.tiers);
            tiers[placeId] = tier;
            this.tiers = new Dictionary<int, int>(tiers);
        }
        else
        {
            Debug.LogError("Wywołano metodę SetTier(), dla pola kltóre nie ma tierów!");
        }
    }

    #endregion Sterowanie rozgrywką

    #region Obsługa eventów

    private void OnPlayerQuit(string playerName) 
    {
        for (int i = 0; i < tiers.Count; i++)
        {
            if (GetOwner(i) == null)
                SetTier(i, 0);
        }
    }

    #endregion Obsługa eventów
}
