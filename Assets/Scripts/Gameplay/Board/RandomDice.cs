using ExitGames.Client.Photon;
using JetBrains.Annotations;
using Photon.Pun;
using System.Runtime.InteropServices;
using UnityEngine;

public class RandomDice
{
    #region Właściwości kostki

    /// <summary>
    /// Wynik ostatniego rzutu pierwszą kostką.
    /// </summary>
    public int last1
    {
        get
        {
            return (int)PhotonNetwork.CurrentRoom.CustomProperties["dice_last1"];
        }
        private set
        {
            Hashtable table = new Hashtable();
            table.Add("dice_last1", value);
            PhotonNetwork.CurrentRoom.SetCustomProperties(table);
        }
    }

    /// <summary>
    /// Wynik ostatniego rzutu drugą kostką.
    /// </summary>
    public int last2
    {
        get
        {
            return (int)PhotonNetwork.CurrentRoom.CustomProperties["dice_last2"];
        }
        private set
        {
            Hashtable table = new Hashtable();
            table.Add("dice_last2", value);
            PhotonNetwork.CurrentRoom.SetCustomProperties(table);
        }
    }

    /// <summary>
    /// Wynik ostatnich rzutów kostką
    /// </summary>
    public RollResult rollResult { get => new RollResult(last1, last2); }

    /// <summary>
    /// Numer gracza którego jest teraz kolejka.
    /// </summary>
    public string currentPlayer
    {
        get
        {
            return (string)PhotonNetwork.CurrentRoom.CustomProperties["dice_currentPlayer"];
        }
        private set
        {
            Hashtable table = new Hashtable();
            table.Add("dice_currentPlayer", value);
            PhotonNetwork.CurrentRoom.SetCustomProperties(table);
        }
    }

    public int currentPlayerIndex
    {
        get
        {
            return (int)PhotonNetwork.CurrentRoom.CustomProperties["dice_currentPlayerIndex"];
        }
        private set
        {
            Hashtable table = new Hashtable();
            table.Add("dice_currentPlayerIndex", value);
            PhotonNetwork.CurrentRoom.SetCustomProperties(table);
        }
    }

    /// <summary>
    /// Ilość rzutów kostką.
    /// </summary>
    public int amountOfRolls
    {
        get
        {
            return (int)PhotonNetwork.CurrentRoom.CustomProperties["dice_amountOfRolls"];
        }
        private set
        {
            Hashtable table = new Hashtable();
            table.Add("dice_amountOfRolls", value);
            PhotonNetwork.CurrentRoom.SetCustomProperties(table);
        }
    }

    /// <summary>
    /// Numer rundy.
    /// </summary>
    public int round
    {
        get
        {
            return (int)PhotonNetwork.CurrentRoom.CustomProperties["dice_round"];
        }
        private set
        {
            Hashtable table = new Hashtable();
            table.Add("dice_round", value);
            PhotonNetwork.CurrentRoom.SetCustomProperties(table);
        }
    }

    #endregion Właściwości kostki

    /// <summary>
    /// Konstruktor iniciujący domyślne wartośći
    /// </summary>
    public RandomDice()
    {
        //Konstruktor dla graczy, którzy nie są adminem
    }

    /// <summary>
    /// Konstruktor inicionujący wartości wczytane z pliku
    /// </summary>
    /// <param name="last1">Ostatni wynik rzutu pierwszej kostki</param>
    /// <param name="last2">Ostatni wynik rzutu drugiej kostki</param>
    /// <param name="currentPlayer">Numer obecnego gracza</param>
    /// <param name="amountOfRolls">Ile razy odbywały się rzuty kością</param>
    /// <param name="round">Obecna runda</param>
    public RandomDice(int last1, int last2, string currentPlayer, int amountOfRolls, int round)
    {
        this.last1 = last1;
        this.last2 = last2;
        this.currentPlayer = currentPlayer;
        this.currentPlayerIndex = 0;
        this.amountOfRolls = amountOfRolls;
        this.round = round;
    }

    /// <summary>
    /// Rzut kostką, wartość 7 jest zarezerwowana dla szansy/losu.
    /// </summary>
    /// <returns>Wynik rzutu kostką.</returns>
    public void RollDice()
    {
        last1 = Random.Range(Keys.Gameplay.MIN_DICE_VALUE, Keys.Gameplay.MAX_DICE_VALUE);
        last2 = Random.Range(Keys.Gameplay.MIN_DICE_VALUE, Keys.Gameplay.MAX_DICE_VALUE);
        amountOfRolls++;
    }

    /// <summary>
    /// Przekazuje kolejke następnemu graczowi.
    /// </summary>
    public void NextTurn()
    {
        GameSession session = GameplayController.instance.session;
        do
        {          
            if (currentPlayerIndex < GameplayController.instance.session.playerCount - 1)
            {
                if (GameplayController.instance.session.playerOrder.Contains(currentPlayer))
                    currentPlayer = session.playerOrder[++currentPlayerIndex];
                else
                    currentPlayer = session.playerOrder[currentPlayerIndex];
            }
            else
            {
                currentPlayer = session.playerOrder[0];
                currentPlayerIndex = 0;
                round++;
            }
            if (GameplayController.instance.session.FindPlayer(currentPlayer).TurnsToSkip != 0)
            {
                GameplayController.instance.session.FindPlayer(currentPlayer).SubstractTurnToSkip();
            }
            else if(!session.FindPlayer(currentPlayer).IsLoser)
            {
                break;
            }
        }
        while (true);
    }

    public void SetLast(int last1, int last2)
    {
        this.last1 = last1;
        this.last2 = last2;
    }
}

[System.Serializable]
public class RollResult
{
    [SerializeField, Tooltip("Wynik rzutu kostką"), Range(0, 6)]
    private int roll1 = 0, roll2 = 0;
    /// <summary>
    /// Tryb porównywanie wyników rzutu
    /// </summary>
    public enum ValidationType
    {
        /// <summary>
        /// Rzuty są tożsame, gdy mają takie same wyniki na odpowiednich miejscach
        /// </summary>
        OneToOne,
        /// <summary>
        /// Rzuty są tożsame, gdy mają te same wartości. Kolejność wartości nie musi się zgadzać
        /// </summary>
        CorrectValue,
        /// <summary>
        /// Rzuty są tożsame, gdy jeden z wyników pokrywa się
        /// </summary>
        SingleCorrect,
        /// <summary>
        /// Rzuty są tożsame, gdy wynik rzutu to para
        /// </summary>
        Pair,
        /// <summary>
        /// Rzuty są tożsame, gdy suma wyrzuconych oczek się zgadza
        /// </summary>
        Sum,
        /// <summary>
        /// Jest tożsame z każdym możliwym RollResult
        /// </summary>
        Any
    }
    [SerializeField, Tooltip("Typ walidacji wyników rzutu kostką")]
    private ValidationType validation = ValidationType.CorrectValue;

    public RollResult(int roll1, int roll2)
    {
        this.roll1 = roll1;
        this.roll2 = roll2;
    }

    public RollResult(ValidationType validation)
    {
        this.validation = validation;
    }

    public RollResult(int roll1, int roll2, ValidationType validation)
    {
        this.roll1 = roll1;
        this.roll2 = roll2;
        this.validation = validation;
    }

    /// <summary>
    /// Wynik pierwszego rzutu kostką
    /// </summary>
    public int Roll1 { get => roll1; }
    /// <summary>
    /// Wynik drugiego rzutu kostką
    /// </summary>
    public int Roll2 { get => roll2; }
    /// <summary>
    /// Suma obu rzutów
    /// </summary>
    public int Sum { get => roll1 + roll2; }

    public override bool Equals(object obj)
    {
        if (!(obj is RollResult)) return false;
        RollResult result = obj as RollResult;

        switch (validation)
        {
            case ValidationType.OneToOne:
                return roll1 == result.roll1 && roll2 == result.roll2;
            case ValidationType.CorrectValue:
                return (roll1 == result.roll1 || roll1 == result.roll2) && (roll2 == result.roll1 || roll2 == result.roll2);
            case ValidationType.SingleCorrect:
                return roll1 == result.roll1 || roll2 == result.roll2;
            case ValidationType.Pair:
                return result.roll1 == result.roll2 || validation == result.validation;
            case ValidationType.Sum:
                return (roll1 + roll2) == (result.roll1 + result.roll2);
            case ValidationType.Any:
                return true;
        }

        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}