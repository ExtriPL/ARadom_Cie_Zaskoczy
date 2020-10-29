using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
public class PlayerSelectorPopup : Popup
{
    public Action<Player> onSelectionEnded;

    public PlayerSelectorPopup(Action<Player> onSelectionEnded)
        : base(AutoCloseMode.EndOfTurn)
    {
        this.onSelectionEnded = onSelectionEnded;
    }
}
