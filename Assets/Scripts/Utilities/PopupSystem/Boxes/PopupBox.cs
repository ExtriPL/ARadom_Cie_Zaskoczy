using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public abstract class PopupBox : MonoBehaviour
{
    protected Popup pattern;
    protected PopupSystem sytem = PopupSystem.instance;

    /// <summary>
    /// Inicjuje box-a zawierającego popup o podanym typie
    /// </summary>
    /// <param name="pattern">Popup wyświetlany przez box-a</param>
    public virtual void Init(Popup pattern)
    {
        this.pattern = pattern;
    }
}