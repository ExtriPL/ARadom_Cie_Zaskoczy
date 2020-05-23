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

    public BoxPool(GameObject parentObject, GameObject pattern, int capacity)
        : base(parentObject, pattern, capacity)
    {
        boxType = pattern.GetComponent<PopupBox>().GetType();
    }

    public override void Deinit()
    {
        foreach (GameObject poolObject in poolObjects) poolObject.GetComponent<PopupBox>().Deinit();

        base.Deinit();
    }

    public override void ReturnObject(GameObject poolObject)
    {
        poolObject.GetComponent<PopupBox>().Deinit();
        base.ReturnObject(poolObject);
    }
}
