using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InfoBox : PopupBox
{
    [SerializeField] private TextMeshProUGUI content;

    public override void Init(Popup source)
    {
        base.Init(source);
        content.text = (source as InfoPopup).message;
    }
}
