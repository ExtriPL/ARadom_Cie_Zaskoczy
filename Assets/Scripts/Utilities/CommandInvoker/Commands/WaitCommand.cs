using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class WaitCommand : Command
{
    private bool finished = false;
    private float time;

    private float startTime;

    public WaitCommand(float time)
        : base ("Wait")
    {
        this.time = time;
    }

    public override void StartExecution(Action<Command> onCommandFinished)
    {
        base.StartExecution(onCommandFinished);
        startTime = Time.time;
    }

    public override void UpdateExecution()
    {
        base.UpdateExecution();
        if (Time.time - startTime >= time) FinishExecution();
    }
}
