using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface IAnimable
{
    /// <summary>
    /// Animowanie wejścia obiektu
    /// </summary>
    /// <param name="onAnimationBegin">Akcje wywoływane gdy animacja się rozpocznie</param>
    /// <param name="onAnimationEnd">Akcje wywoływane gdy animacja się skończy</param>
    IEnumerator AnimateEntrance(Action onAnimationBegin = null, Action onAnimationEnd = null);

    /// <summary>
    /// Animowanie wyjścia obiektu
    /// </summary>
    /// <param name="onAnimationBegin">Akcje wywoływane gdy animacja się rozpocznie</param>
    /// <param name="onAnimationEnd">Akcje wywoływane gdy animacja się skończy</param>
    IEnumerator AnimateExit(Action onAnimationBegin = null, Action onAnimationEnd = null);
}