using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RightPanel : MonoBehaviour
{
    private UIPanels uIPanels;

    public void Init(UIPanels UIPanels)
    {
        uIPanels = UIPanels;
        gameObject.GetComponent<Animation>().Play("RightToMiddle");
        uIPanels.bottomPanel.GetComponent<Animation>().Play("MiddleToLeft");
    }

    public void Deinit()
    {
        gameObject.GetComponent<Animation>().Play("MiddleToRight");
    }

    public void Close()
    {
        Deinit();
        uIPanels.bottomPanel.GetComponent<Animation>().Play("LeftToMiddle");
    }

}
