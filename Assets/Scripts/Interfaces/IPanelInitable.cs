using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPanelInitable
{
    void PreInit();
    void Init(MainMenuController mainMenuController);
}
