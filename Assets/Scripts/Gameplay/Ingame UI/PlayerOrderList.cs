using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerOrderList : MonoBehaviour, IEventSubscribable
{
    public GameObject content;
    public List<GameObject> playerElements;
    public GameObject openPanel;


    private GameplayController gC;
    private UIPanels uIPanels;
    private int playerCount;
    private int currentPlayerId;
    private bool open = false;

    public void PreInit(UIPanels controller)
    {
        gC = GameplayController.instance;
        uIPanels = controller;
    }

    public void Init()
    {
        playerCount = gC.session.playerOrder.Count;
        currentPlayerId = gC.session.playerOrder.IndexOf(gC.board.dice.currentPlayer);
        
        int i = 0;

        openPanel.GetComponent<RectTransform>().anchorMin = new Vector2(openPanel.GetComponent<RectTransform>().anchorMin.x, 1 - playerCount);
            

        gC.session.playerOrder.ForEach((playerNick) =>
        {
            playerElements[i].GetComponent<TextMeshProUGUI>().text = playerNick;
            playerElements[i].GetComponent<TextMeshProUGUI>().color = gC.session.FindPlayer(playerNick).MainColor;
            playerElements[i].SetActive(true);
            i++;
        });
        NextPlayer();
    }

    public void DeInit()
    {
        playerElements.ForEach((playerEl) =>
        {
            playerEl.GetComponent<TextMeshProUGUI>().text = "";
            playerEl.GetComponent<TextMeshProUGUI>().color = Color.white;
            playerEl.SetActive(false);
        });
    }

    public void SubscribeEvents()
    {
        EventManager.instance.onTurnChanged += OnTurnChanged;
        EventManager.instance.onPlayerLostGame += OnPlayerLost;
        EventManager.instance.onPlayerQuited += OnPlayerLeft;
    }

    public void UnsubscribeEvents()
    {
        EventManager.instance.onTurnChanged -= OnTurnChanged;
        EventManager.instance.onPlayerLostGame -= OnPlayerLost;
        EventManager.instance.onPlayerQuited -= OnPlayerLeft;
    }

    private void OnTurnChanged(string previousPlayerName, string currentPlayerName) 
    {
        currentPlayerId = gC.session.playerOrder.IndexOf(gC.board.dice.currentPlayer);
        NextPlayer();
    }

    private void NextPlayer() 
    {
        if(gameObject.activeInHierarchy) StartCoroutine(Transition());
    }

    private IEnumerator Transition() 
    {
        Vector3 contentPos = openPanel.GetComponent<RectTransform>().anchoredPosition;
        float tempPosition = contentPos.y;
        int targetPostition = 60 * currentPlayerId;

        while (tempPosition < targetPostition)
        {
            tempPosition += 5;
            contentPos = new Vector3(contentPos.x, tempPosition, contentPos.z);
            openPanel.GetComponent<RectTransform>().anchoredPosition = contentPos;
            yield return new WaitForSeconds(0.05f);
        }
        while (tempPosition > targetPostition)
        {
            tempPosition -= 5;
            contentPos = new Vector3(contentPos.x, tempPosition, contentPos.z);
            openPanel.GetComponent<RectTransform>().anchoredPosition = contentPos;
            yield return new WaitForSeconds(0.05f);
        }

        yield return null;
    }

    private void OnPlayerLost(string name) 
    {
        DeInit();
        Init();
    }

    private void OnPlayerLeft(string name)
    {
        DeInit();
        Init();
    }

    public void ToggleOpen() 
    {
        if (open)
        {
            //zamykamy
            gameObject.GetComponent<Animation>().Play("PlayerOrderListClose");
            gameObject.GetComponent<RectMask2D>().enabled = true;
            open = false;
        }
        else 
        {
            //otwieramy
            gameObject.GetComponent<Animation>().Play("PlayerOrderListOpen");
            gameObject.GetComponent<RectMask2D>().enabled = false;
            open = true;
        }
    }

}
