using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

[System.Serializable]
public class Board
{
    private GameplayController gameplayController;

    /// <summary>
    /// Lista pól na planszy.
    /// </summary>
    [Tooltip("Lista pól w na planszy.")]
    public List<Field> fields = new List<Field>();

    /// <summary>
    /// Kostka do gry.
    /// </summary>
    private RandomDice dice;

    /// <summary>
    /// Słownik przypisujący każdemu miejscu na planszy pole(tj. budynek albo pole specjalne itp.).
    /// </summary>
    private Dictionary<int, int> places
    {
        get
        {
            return (Dictionary<int, int>) PhotonNetwork.CurrentRoom.CustomProperties["board_places"];
        }
        set
        {
            Hashtable table = new Hashtable();
            table.Add("board_places", value);
            PhotonNetwork.CurrentRoom.SetCustomProperties(table);
        }
    }

    #region Inicjalizacja

    public void Init(GameplayController gameplayController)
    {
        this.gameplayController = gameplayController;

        //Inicjowanie właściwości rozgrywki przez właściciela pokoju
        if (gameplayController.session.roomOwner.IsLocal)
        {
            places = new Dictionary<int, int>();

            if (gameplayController.loadedFromSave)
            {
                GameSave save = gameplayController.save;
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

                this.places = places;
            }
            else
            {
                dice = new RandomDice();
                RandomizeFields();
            }
        }
        else dice = new RandomDice();
    }

    /// <summary>
    /// Zapisuje odpowiednie ustawienia do obiektu GameSave znajdującego się w klasie GameplayController
    /// </summary>
    public void SaveToInstance(ref GameSave save)
    {
        DiceSettings ds = new DiceSettings();
        ds.amountOfRolls = dice.amountOfRolls;
        ds.currentPlayer = dice.currentPlayer;
        ds.last1 = dice.last1;
        ds.last2 = dice.last2;
        ds.round = dice.round;

        save.places = new Dictionary<int, System.Tuple<int, string>>();
        for (int i = 0; i < places.Keys.Count; i++) save.places.Add(i, new System.Tuple<int, string>(places[i], GetField(i).name));

        save.dice = ds;
    }

    public void SubscribeEvents()
    {
        //EventManager.instance.onPlayerMove += MovePlayer;
    }

    public void UnsubscribeEvents()
    {
        //EventManager.instance.onPlayerMove -= MovePlayer;
    }

    #endregion Inicjalizacja

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
        for(int i = 0; i < fields.Count; i++)
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
    /// <param name="field">Pole.</param>
    /// <returns>Obiekt gracza, który jest właścicielem pola. Jeżeli pole nie ma właściciela, zwraca null.</returns>
    public Player GetOwner(int field)
    {
        for(int i = 0; i < gameplayController.session.GetPlayersCount(); i++)
        {
            Player p = gameplayController.session.FindPlayer(i);
            if (p.HasField(field))
            {
                return p;
            }
        }
        return null;
    }

    /// <summary>
    /// Metoda przesuwająca gracza o daną ilość pól.
    /// </summary>
    /// <param name="p">Gracz</param>
    /// <param name="amount">Ilośc pól</param>
    public void MovePlayer(Player p, int amount)
    {
        int fromFieldIndex = p.fieldId;
        int toFieldIndex;
        
        //Jeżeli numer pola po wykonaniu ruchu przekroczy ilość pól na planszy - 1 (indeks ostatniego pola), gracz zostanie przeniesiony na odpowiedznie pole na początku planszy.
        //Np: gracz stoi na polu 10, przesuwamy go o 5 pól, skończy na polu 3.
        p.fieldId = (p.fieldId + amount > Keys.Board.FIELD_COUNT - 1) ? p.fieldId = (p.fieldId + amount) - (Keys.Board.FIELD_COUNT - 1) : p.fieldId + amount;
        
        toFieldIndex = p.fieldId;

        EventManager.instance.SendOnPlayerMove(p.GetName(), fromFieldIndex, toFieldIndex);
    }

    /// <summary>
    /// Metoda przypisująca przypadkowe budynki do pól.
    /// </summary>
    private void RandomizeFields()
    {
        List<int> used = new List<int>();
        Dictionary<int, int> places = new Dictionary<int, int>();

        //Sprawdzanie, czy istnieje wystarczająca liczba budynków do rozpoczęcia gry
        if (fields.Count >= Keys.Board.FIELD_COUNT)
        {
            for (int i = 0; i < Keys.Board.FIELD_COUNT; i++)
            {
                int j;
                do j = Random.Range(0, fields.Count);
                while (used.Contains(j));

                used.Add(j);
                places.Add(i, j);
            }

            this.places = places;
        }
        else Debug.LogError("Nie ma wystarczającej ilości budynków by zainicjować grę!");
    }
}
