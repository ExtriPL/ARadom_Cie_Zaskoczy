using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TradeListing : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public Button removeButton;
    public Field tradeField;
    private BasePool listingsPool;
    private RightPanel rightPanel;

    public void Init(RightPanel controller, BasePool pool, Field field, bool received = false)
    {
        listingsPool = pool;
        rightPanel = controller;
        tradeField = field;
        removeButton.gameObject.SetActive(!received);

        nameText.text = tradeField.GetFieldName();
    }

    public void DeInit()
    {
        tradeField = null;
        nameText.text = "";
        listingsPool.ReturnObject(gameObject);
    }
}
