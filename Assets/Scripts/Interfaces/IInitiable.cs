using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInitiable<T>
{
    void PreInit(T controller);
    void Init();
    void DeInit();
}
