using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "Prison", menuName = "ARadom/Field/Special/Prison")]
public class PrisonSpecial : SpecialField
{
    [SerializeField, Tooltip("Lista rzutów, które uwalniają z więzienia")]
    private List<RollResult> freeingThrows = new List<RollResult>();
}
