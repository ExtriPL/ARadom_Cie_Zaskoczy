using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSelectorBox : PopupBox
{
    private BasePool buttonPool;
    [SerializeField, Tooltip("Obiekt przechowujący wszystkie przyciski(Radio Buttony) wyświetlane przez PlayerSelectorBox")]
    private GameObject buttons;
    //Lista wszystkich przycisków
    private List<GameObject> buttonList = new List<GameObject>();

    public override void InitBox()
    {
        base.InitBox();
        buttonPool = new BasePool(buttons, null, GameplayController.instance.session.playerCount);
        List<GameObject> poolObjects = new List<GameObject>();

        foreach (Toggle b in buttons.GetComponentsInChildren<Toggle>())
        {
            if (b.gameObject != buttons)
            {
                buttonList.Add(b.gameObject);
                poolObjects.Add(b.gameObject);
            }
        }

        buttonPool.Init(poolObjects);
    }

    public override void Init(Popup source)
    {
        base.Init(source);
        ShowPlayerButtons();

        boxAnimator.SetTrigger("Show");
    }

    private void ShowPlayerButtons()
    {
        GameSession session = GameplayController.instance.session;
        for(int i = 0; i < buttonList.Count; i++)
        {
            if (i < session.playerCount)
            {
                Player player = session.FindPlayer(i);
                if(!player.IsLoser)
                {
                    buttonList[i].GetComponentInChildren<TextMeshProUGUI>().text = player.GetName();
                    buttonList[i].SetActive(true);
                    continue;
                }
            }

            buttonList[i].SetActive(false);
        }
    }

    private Player GetSelectedPlayer()
    {
        for(int i = 0; i < buttonList.Count; i++)
        {
            if (buttonList[i].GetComponent<Toggle>().isOn)
                return GameplayController.instance.session.FindPlayer(i);
        }

        return null;
    }

    public override void OnCloseAnimationEnd()
    {
        PlayerSelectorPopup selector = source as PlayerSelectorPopup;
        selector.onSelectionEnded?.Invoke(GetSelectedPlayer());

        base.OnCloseAnimationEnd();
    }
}