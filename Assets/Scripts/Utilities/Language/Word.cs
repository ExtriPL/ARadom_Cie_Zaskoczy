﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public struct Word
{
    public string key;
    public string value;
    public Word(string key, string value) 
    {
        this.key = key;
        this.value = value;
    }
}