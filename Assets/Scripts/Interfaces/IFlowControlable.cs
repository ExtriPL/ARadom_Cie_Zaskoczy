using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface IFlowControlable
{
    /// <summary>
    /// Priorytet przekazania kontroli nad przepływem
    /// </summary>
    int FlowPriority { get; }
    /// <summary>
    /// Funkcja wywoływana tuż po tym, jak kontrola nad przepływem zostanie przekazana danemu obiektowi
    /// </summary>
    /// <param name="args">Argumenty potrzebne do rozpoczęcia akcji własnych obiektu, który staje się nowym kontrolerem</param>
    void TransmitFlow(object[] args = null);
}
