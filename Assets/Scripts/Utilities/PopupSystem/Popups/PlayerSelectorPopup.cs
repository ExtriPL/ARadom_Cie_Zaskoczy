using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
public class PlayerSelectorPopup : Popup
{
    public delegate void SelectionEnded(Player caller, Player selected);
    public SelectionEnded onSelectionEnded;
    public Player caller;

    public PlayerSelectorPopup(Player caller, SelectionEnded onSelectionEnded)
        : base(AutoCloseMode.EndOfTurn)
    {
        this.caller = caller;
        this.onSelectionEnded = onSelectionEnded;
    }
}
