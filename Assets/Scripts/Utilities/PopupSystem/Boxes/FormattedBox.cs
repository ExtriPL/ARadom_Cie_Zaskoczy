using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FormattedBox : PopupBox
{
    [SerializeField] private GameObject titleBar;
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private GameObject content;
    [SerializeField] private GameObject verticalBar;

    /// <summary>
    /// Ilość elementów, które zostały zespawnowane w FormattedBox-ie
    /// </summary>
    private int elementsCount = 0;

    public override void Init(Popup source)
    {
        base.Init(source);
        FormattedPopup popup = source as FormattedPopup;
        title.text = popup.title;
        popup.contentWidth -= verticalBar.GetComponent<RectTransform>().sizeDelta.x;
        //Ukrywanie titlebar-u jeżeli tytuł jest pusty
        if (popup.title.Equals("")) titleBar.SetActive(false);
        InitElements();
    }

    /// <summary>
    /// Inicjuje elementy zawarte w FormattedPopup-ie
    /// </summary>
    private void InitElements()
    {
        FormattedPopup popup = source as FormattedPopup;
        
        for (int i = 0; i < popup.GetSymbolsCount(); i++)
        {
            string symbol = popup.GetSymbol(i);
            FormattedElementType type = popup.GetElementType(symbol);
            GameObject element = GetPrefab(type);
            element.GetComponent<RectTransform>().SetParent(content.GetComponent<RectTransform>());

            element.GetComponent<RectTransform>().anchoredPosition = popup.GetElementPosition(symbol);
            element.GetComponent<RectTransform>().sizeDelta = popup.GetElementSize(symbol);

            elementsCount++;
        }
    }

    private GameObject GetPrefab(FormattedElementType type)
    {
        GameObject element = new GameObject("Element " + elementsCount, typeof(RectTransform));
        element.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 1f);
        element.GetComponent<RectTransform>().anchorMax = new Vector2(0f, 1f);
        element.GetComponent<RectTransform>().pivot = new Vector2(0f, 1f);

        switch (type)
        {
            case FormattedElementType.Button:
                {
                    element.AddComponent<Image>();
                    element.AddComponent<Button>();
                    
                    
                    GameObject text = new GameObject("Text", typeof(TextMeshProUGUI));
                    text.GetComponent<RectTransform>().SetParent(element.GetComponent<RectTransform>());
                    text.GetComponent<TextMeshProUGUI>().color = Color.black;
                }
                break;
            case FormattedElementType.Sprite:
                {
                    element.AddComponent<Image>();
                }
                break;
            case FormattedElementType.Text:
                {
                    element.AddComponent<TextMeshProUGUI>();
                }
                break;
            default:
                return null;
        }

        return element;
    }
}
