using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MessageLifecycle : MonoBehaviour
{
    public float lifeSpan { get; private set; }
    public int messageId { get; private set; }
    public string messageIdentity { get; private set; }

    [SerializeField] private float currentLifeTime;

    private void Update()
    {
        //Niższy wiadomość, jeżeli jej czas życia jest dłuższy od maksymalnego
        if(lifeSpan > 0)
        {
            currentLifeTime += Time.deltaTime;
            if (currentLifeTime >= lifeSpan) MessageSystem.instance.RemoveMessage(messageIdentity);
        }
    }

    /// <summary>
    /// Rozpoczyna cykl życia wiadomości
    /// </summary>
    /// <param name="messageId">Identyfikuje wiadomość na liście w klasie MessageSystem</param>
    /// <param name="lifeSpan">Czas życia wiadomości w sekundach. Jeżeli ujemny, wiadomość sama nie zniknie</param>
    public void StartLifeCycle(string messageIdentity, int messageId, float lifeSpan)
    {
        this.lifeSpan = lifeSpan;
        this.messageId = messageId;
        this.messageIdentity = messageIdentity;
    }

    /// <summary>
    /// Zmienia tekst wyświetlanej wiadomości
    /// </summary>
    /// <param name="message"></param>
    public void ChangeMessage(string message)
    {
        currentLifeTime = 0;
        gameObject.GetComponent<TextMeshProUGUI>().SetText(message);
    }

    /// <summary>
    /// Zwraca obecnie wyświetlaną wiadomość
    /// </summary>
    /// <returns></returns>
    public string GetMessage()
    {
        return gameObject.GetComponent<TextMeshProUGUI>().text;
    }
}
