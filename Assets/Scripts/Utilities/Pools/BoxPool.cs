using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BoxPool : BasePool
{
    /// <summary>
    /// Typ PopupBox-u przechowywanego przez pule
    /// </summary>
    public Type boxType { get; private set; }

    /// <summary>
    /// Inicjalizacja puli popupbox-ów
    /// </summary>
    /// <param name="parentObject">Obiekt macierzysty przechowujący wszystkie obiekty w puli</param>
    /// <param name="pattern">Wzór, według którego mają zaostać stworzone wszystkie obiekty puli</param>
    /// <param name="capacity">Pojemność puli</param>
    /// <param name="preserveOrder">Czy pula ma podjąć próbę zachowania porządku wyciągania</param>
    public BoxPool(GameObject parentObject, GameObject pattern, int capacity)
        : base(parentObject, pattern, capacity)
    {
        boxType = pattern.GetComponent<PopupBox>().GetType();
    }

    protected override void AddObject()
    {
        base.AddObject();
        poolObjects[poolObjects.Count - 1].GetComponent<PopupBox>().InitBox();
    }

    public override void ReturnObject(GameObject poolObject)
    {
        poolObject.GetComponent<PopupBox>().Deinit();
        base.ReturnObject(poolObject);
    }
}
