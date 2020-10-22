using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}
