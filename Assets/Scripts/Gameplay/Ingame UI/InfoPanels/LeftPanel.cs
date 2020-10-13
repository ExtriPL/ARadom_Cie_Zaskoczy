using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeftPanel : MonoBehaviour
{
    private UIPanels uIPanels;
    public GameObject closeButton;
    public void Init(UIPanels uIPanels) 
    {
        this.uIPanels = uIPanels;
        gameObject.GetComponent<Animation>().Play("LeftToMiddle");
        uIPanels.bottomPanel.GetComponent<Animation>().Play("MiddleToRight");
    }

    public void Deinit() 
    {
        gameObject.GetComponent<Animation>().Play("MiddleToLeft");
    }

    public void Close()
    {
        Deinit();
        uIPanels.bottomPanel.GetComponent<Animation>().Play("RightToMiddle");
    }
}
