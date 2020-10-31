using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public static class MathExtension
{
    public static bool IsBetween<T>(this T item, T start, T end) where T : IComparable
    {
        return Comparer<T>.Default.Compare(item, start) >= 0
            && Comparer<T>.Default.Compare(item, end) <= 0;
    }

    /// <summary>
    /// Zmienia losowo kolejność elementów na liście
    /// </summary>
    public static void Shuffle<T>(this IList<T> list)
    {
        int last = list.Count - 1;

        for(int i = 0; i < last; i++)
        {
            int rIndex = UnityEngine.Random.Range(0, last);
            (list[i], list[rIndex]) = (list[rIndex], list[i]);
        }
    }

    /// <summary>
    /// Funkcja mapująca wartość z jednego przedziału, na wartość z innego przedziału
    /// </summary>
    /// <param name="min">Minimalna wartość obecnego przedziału</param>
    /// <param name="max">Maksymalna wartość obecnego przedziału</param>
    /// <param name="newMin">Minimalna wartość nowego przedziału</param>
    /// <param name="newMax">Maksymalna wartość nowego przedziału</param>
    /// <param name="value">Wartość zawierająca się w przedziale [min;max]</param>
    /// <returns>Zmapowana wartość na nowy przedział</returns>
    public static float Map(float min, float max, float newMin, float newMax, float value)
    {
        if (min > max)
            (min, max) = (max, min);
        if (newMin > newMax)
            (newMin, newMax) = (newMax, newMin);

        value = Mathf.Clamp(value, min, max);

        float diff = max - min;
        float progress = (value - min) / diff;
        float newDiff = newMax - newMin;

        return progress * newDiff + newMin;
    }

    /// <summary>
    /// Zwraca losowo wybrany znak liczby
    /// </summary>
    /// <returns>1 lub -1</returns>
    public static int RandomSign()
    {
        int sign = Random.Range(0, 2);

        if (sign == 0)
            return -1;
        else
            return 1;
    }

    /// <summary>
    /// Zostawia tylko jedną współrzędną wektora. Pozostałe zeruje
    /// </summary>
    public static Vector3 LeaveOne(Vector3 vector)
    {
        int stay = Random.Range(0, 3);

        if (stay != 0)
            vector.x = 0;
        if (stay != 1)
            vector.y = 0;
        if (stay != 2)
            vector.z = 0;

        return vector;
    }

    /// <summary>
    /// Zwraca kąt obrotu uzyskiwany przez obrót w drugą stronę
    /// </summary>
    /// <param name="rotation">Obecny kąt</param>
    public static float InverseRotation(float rotation)
    {
        int inverseSign = -Math.Sign(rotation);
        float angle = 360f - Mathf.Abs(rotation);

        return inverseSign * angle;
    }
}
