using Photon.Pun;
using UnityEngine;

public class SavePanel : MonoBehaviourPunCallbacks, IInitiable<MainMenuController>
{

    private MainMenuController mainMenuController;
    private BasePool basePool;
    public GameObject content;
    public GameObject template;

    public void PreInit()
    {
        basePool = new BasePool(content, template, 3);
        basePool.Init();
    }

    public void Init(MainMenuController mainMenuController)
    {
        this.mainMenuController = mainMenuController;
        ListItems();
    }

    public void DeInit() {}

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) mainMenuController.OpenPanel(4);
    }

    public void ListItems()
    {
        for (int i = 0; i < FileManager.GetSavesName().Count; i++)
        {

            basePool.TakeObject().GetComponent<SaveListing>().Init(FileManager.GetSavesName()[i], basePool, mainMenuController);
            //Debug.Log((FileManager.GetSavesName()[i] != null) + ";" + (basePool != null) + ";" + (mainMenuController != null));
        }
    }
}
