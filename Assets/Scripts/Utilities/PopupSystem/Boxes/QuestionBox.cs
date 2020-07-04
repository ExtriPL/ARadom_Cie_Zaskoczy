using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class QuestionBox : PopupBox
{
    [SerializeField] private TextMeshProUGUI message;
    [SerializeField, Tooltip("Obiekt przechowujący przyciski wyświetlane przez QuestionBox")]
    private GameObject buttons;

    private BasePool buttonPool;
    private List<GameObject> showedButtons = new List<GameObject>();

    public override void InitBox()
    {
        buttonPool = new BasePool(buttons, null, Keys.Popups.QUESTIONBOX_BUTTONS_AMOUNT);
        List<GameObject> poolObjects = new List<GameObject>(); //Lista obiektów podpiętych na starcie do obiektu buttons
        foreach (Button b in buttons.GetComponentsInChildren<Button>())
        {
            if (b.gameObject != buttons) poolObjects.Add(b.gameObject);
        }

        buttonPool.Init(poolObjects);
    }

    public override void Init(Popup source)
    {
        if (buttonPool == null) InitBox();
        base.Init(source);
        QuestionPopup popup = source as QuestionPopup;
        message.text = popup.message;
        InitButons();
    }

    public override void Deinit()
    {
        message.text = "QuestionBox";

        //Zwracanie użytych przycisków do puli
        for (int i = showedButtons.Count - 1; i >= 0; i--)
        {
            GameObject button = showedButtons[i];
            button.GetComponent<Button>().onClick.RemoveAllListeners();
            button.GetComponentInChildren<TextMeshProUGUI>().text = "";
            buttonPool.ReturnObject(button);
        }

        showedButtons.Clear();

        base.Deinit();
    }

    /// <summary>
    /// Rozmieszczanie przycisków w zależności od ich ilości
    /// </summary>
    private void InitButons()
    {
        QuestionPopup popup = source as QuestionPopup;

        foreach(Tuple<string, Popup.PopupAction> button in popup.buttons)
        {
            GameObject gameObject = buttonPool.TakeObject();
            gameObject.GetComponentInChildren<TextMeshProUGUI>().text = button.Item1;
            gameObject.GetComponent<Button>().onClick.AddListener(delegate 
            {
                button.Item2?.Invoke(source);
            });

            showedButtons.Add(gameObject);
        }
    }
}
