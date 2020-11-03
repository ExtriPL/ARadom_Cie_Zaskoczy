using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class SpecialField : Field
{
    /// <summary>
    /// Model budynku stojącego na polu
    /// </summary>
    [SerializeField, Tooltip("Model budynku stojącego na polu")]
    private GameObject model;
    
    public override GameObject GetStartModel() => model;

    public override GameObject GetModel() => model;
}
