using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;

public class RandomDice
{
    /// <summary>
    /// Wynik ostatniego rzutu kostką.
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
    /// Numer gracza którego jest teraz kolejka.
    /// </summary>
    public int currentPlayer
    {
        get
        {
            return (int)PhotonNetwork.CurrentRoom.CustomProperties["dice_currentPlayer"];
        }

        private set
        {
            Hashtable table = new Hashtable();
            table.Add("dice_currentPlayer", value);
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

    /// <summary>
    /// Konstruktor iniciujący domyślne wartośći
    /// </summary>
    public RandomDice()
    {
        last1 = 0;
        last2 = 0;
        currentPlayer = 0;
        amountOfRolls = 0;
        round = 0;
    }

    /// <summary>
    /// Konstruktor inicionujący wartości wczytane z pliku
    /// </summary>
    /// <param name="last">Ostatni wynik rzutu kośćmi</param>
    /// <param name="currentPlayer">Numer obecnego gracza</param>
    /// <param name="amountOfRolls">Ile razy odbywały się rzuty kością</param>
    /// <param name="round">Obecna runda</param>
    public RandomDice(int last1, int last2, int currentPlayer, int amountOfRolls, int round)
    {
        this.last1 = last1;
        this.last2 = last2;
        this.currentPlayer = currentPlayer;
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
        do
        {
            if (currentPlayer < GameplayController.instance.session.GetPlayersCount() - 1)
            {
                currentPlayer++;
            }
            else
            {
                currentPlayer = 0;
                round++;
            }
            if (GameplayController.instance.session.FindPlayer(currentPlayer).turnsToSkip != 0)
            {
                GameplayController.instance.session.FindPlayer(currentPlayer).SubstractTurnToSkip();
            }
            else
            {
                break;
            }
        }
        while (true);
    }
}