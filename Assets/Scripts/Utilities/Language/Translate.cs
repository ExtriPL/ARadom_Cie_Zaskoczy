using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]

public class Translate : MonoBehaviour
{
    public string key;
    private void Start()
    {
        TextMeshProUGUI text = gameObject.GetComponent<TextMeshProUGUI>();
        text.text = SettingsController.instance.languageController.GetWord(key);
    }
}
