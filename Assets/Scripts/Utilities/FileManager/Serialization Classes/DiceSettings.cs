using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[System.Serializable]
public struct DiceSettings
{
    public int last1;
    public int last2;
    public string currentPlayer;
    public int amountOfRolls;
    public int round;
}