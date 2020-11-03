using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerOrderList : MonoBehaviour, IEventSubscribable
{
    public GameObject content;
    public List<GameObject> playerElements;


    private GameplayController gC;
    private UIPanels uIPanels;
    private int playerCount;
    private int currentPlayerId;

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

        gC.session.playerOrder.ForEach((playerNick) =>
        {
            playerElements[i].GetComponent<TextMeshProUGUI>().text = playerNick;
            playerElements[i].GetComponent<TextMeshProUGUI>().color = gC.session.FindPlayer(playerNick).MainColor;
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
    }

    public void UnsubscribeEvents()
    {
        EventManager.instance.onTurnChanged -= OnTurnChanged;
        EventManager.instance.onPlayerLostGame -= OnPlayerLost;
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) 
        {
            NextPlayer();
        }
    }

    private void OnTurnChanged(string previousPlayerName, string currentPlayerName) 
    {
        currentPlayerId = gC.session.playerOrder.IndexOf(gC.board.dice.currentPlayer);
        NextPlayer();
    }

    private void NextPlayer() 
    {
        StartCoroutine(Transition());
    }

    private IEnumerator Transition() 
    {
        Vector3 contentPos = content.GetComponent<RectTransform>().anchoredPosition;
        float tempPosition = contentPos.y;
        int targetPostition = 60 * currentPlayerId;

        if (currentPlayerId == 0) content.GetComponent<RectTransform>().anchoredPosition = new Vector3(contentPos.x, 0, contentPos.z);
        else
        {
            while (tempPosition != targetPostition)
            {
                contentPos = new Vector3(contentPos.x, tempPosition, contentPos.z);
                tempPosition += 5;
                content.GetComponent<RectTransform>().anchoredPosition = contentPos;
                yield return new WaitForSeconds(0.05f);
            }
            yield return null;
        }
    }

    private void OnPlayerLost(string name) 
    {
        DeInit();
        Init();
    }
}
