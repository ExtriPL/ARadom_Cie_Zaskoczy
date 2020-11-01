using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ImprisonAction : ActionCard
{
    public override void Call(Player caller, bool showMessage = false)
    {
        GameplayController.instance.Imprison(caller);

        if(showMessage)
        {
            string[] message = new string[] { lang.PackKey("YOU_HAVE_BEEN_IMPRISONED") };// "Zostałeś uwięziony";
            EventManager.instance.SendPopupMessage(message, IconPopupType.Prison, caller);
        }
    }
}