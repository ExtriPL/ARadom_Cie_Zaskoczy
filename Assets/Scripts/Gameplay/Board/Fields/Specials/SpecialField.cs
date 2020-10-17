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
    [SerializeField, Tooltip("Opis pola"), TextArea]
    protected string fieldHistory;

    public override GameObject GetStartModel() => model;
}
