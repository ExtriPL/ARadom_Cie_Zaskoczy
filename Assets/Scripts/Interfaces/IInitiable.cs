using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInitiable<T>
{
    void PreInit();
    void Init(T controller);
    void DeInit();
}
