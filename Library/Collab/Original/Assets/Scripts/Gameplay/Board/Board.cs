using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Board
{
    private GameplayController gameplayController;

    /// <summary>
    /// Lista pól na planszy.
    /// </summary>
    [Tooltip("Lista pól w na planszy.")]
    public List<Field> fields;

    /// <summary>
    /// Kostka do gry.
    /// </summary>
    private RandomDice dice = new RandomDice();

    /// <summary>
    /// Słownik przypisujący każdemu miejscu na planszy pole(tj. budynek albo pole specjalne itp.).
    /// </summary>
    public Dictionary<int, Field> places = new Dictionary<int, Field>();

    #region Inicjalizacja

    public void Init(GameplayController gameplayController)
    {
        this.gameplayController = gameplayController;

        RandomizeFields();
    }

    public void SubscribeEvents()
    {
        EventManager.instance.onPlayerMove += MovePlayer;
    }

    public void UnsubscribeEvents()
    {
        EventManager.instance.onPlayerMove -= MovePlayer;
    }

    #endregion Inicjalizacja

    /// <summary>
    /// Odnajduje budynek po jego numerze na planszy
    /// </summary>
    /// <param name="index">Numer pola</param>
    /// <returns>Pole o podanym numerze</returns>
    public Field GetField(int index)
    {
        return places[index];
    }

    /// <summary>
    /// Metoda wyszukująca numer pola.
    /// </summary>
    /// <param name="building">Budynek</param>
    /// <returns>Numer pola, na którym znajduje się ten budynek.</returns>
    public int GetFieldId(Field field)
    {
        return places.FirstOrDefault(x => x.Value == field).Key;
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
            if (p.fieldList.Contains(field))
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
        //Jeżeli numer pola po wykonaniu ruchu przekroczy ilość pól na planszy - 1 (indeks ostatniego pola), gracz zostanie przeniesiony na odpowiedznie pole na początku planszy.
        //Np: gracz stoi na polu 10, przesuwamy go o 5 pól, skończy na polu 3.
        p.fieldId = (p.fieldId + amount > Keys.Board.FIELD_COUNT - 1) ? p.fieldId = (p.fieldId + amount) - (Keys.Board.FIELD_COUNT - 1) : p.fieldId + amount;
    }

    /// <summary>
    /// Metoda przypisująca przypadkowe budynki do pól.
    /// </summary>
    private void RandomizeFields()
    {
        List<int> used = new List<int>();

        for (int i = 0; i < Keys.Board.FIELD_COUNT; i++)
        {
            int j;
            do
            {
                j = Random.Range(0, Keys.Board.FIELD_COUNT);
            }
            while (used.Contains(j));

            used.Add(j);
            places.Add(j, fields[i]);
        }
    }
}
