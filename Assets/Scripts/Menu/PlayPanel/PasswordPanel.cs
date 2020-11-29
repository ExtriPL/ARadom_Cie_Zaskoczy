using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PasswordPanel : MonoBehaviourPunCallbacks, IInitiable<MainMenuController>
{
    private MainMenuController mainMenuController;

    public void Init() 
    {
        mainMenuController.loadingScreen.EndLoading();
    }

    public void PreInit(MainMenuController mainMenuController) 
    {
        this.mainMenuController = mainMenuController;
    }

    public void DeInit() {}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
