using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class SubscribeEventsCommand : Command
{
    List<IEventSubscribable> eventSubscribables = new List<IEventSubscribable>();

    public SubscribeEventsCommand(List<IEventSubscribable> eventSubscribables)
        : base("Subscribe Events")
    {
        this.eventSubscribables = eventSubscribables;
    }

    public override void StartExecution(Action<Command> OnCommandFinished)
    {
        base.StartExecution(OnCommandFinished);

        foreach (IEventSubscribable es in eventSubscribables) es.SubscribeEvents();

        FinishExecution();
    }
}
