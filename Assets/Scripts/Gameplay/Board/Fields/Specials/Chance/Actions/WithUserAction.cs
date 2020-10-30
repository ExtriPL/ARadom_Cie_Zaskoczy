using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class WithUserAction : ActionCard
{
    private ActionCard insideAction;

    public WithUserAction(ActionCard insideAction)
    {
        this.insideAction = insideAction;
    }

    public override void Call(Player caller)
    {
        throw new NotImplementedException();
    }
}
