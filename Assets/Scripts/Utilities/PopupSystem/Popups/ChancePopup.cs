using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ChancePopup : Popup
{
    public ChanceCard card { get; private set; }

    public ChancePopup(ChanceCard card, Player caller)
    {
        this.card = card;
        onClose += delegate
        {
            card.CallActions(caller);
        };
    }
}