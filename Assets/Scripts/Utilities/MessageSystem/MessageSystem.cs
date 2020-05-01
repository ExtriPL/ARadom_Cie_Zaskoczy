using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageSystem : MonoBehaviour
{
    public static MessageSystem instance;

    /// <summary>
    /// Lista zawierająca odwołania do kontrolerów wszystkich wiadomości
    /// </summary>
    private List<GameObject> messages = new List<GameObject>();
    /// <summary>
    /// Słownik pozwalający zidentyfikować wiadomości po specjalnych nazwach
    /// </summary>
    private Dictionary<string, int> identities = new Dictionary<string, int>();

    [SerializeField] private GameObject shortMessage;
    [SerializeField] private GameObject mediumMessage;
    [SerializeField] private GameObject longMessage;

    // Start is called before the first frame update
    void Start()
    {
        if (instance == null) instance = this;
        else if (instance != this) Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        gameObject.GetComponent<Transform>().SetAsLastSibling(); //Obiekt zawsze będzie znajować się na wierzchu
    }

    /// <summary>
    /// Niszczy wiadomość
    /// </summary>
    /// <param name="messageId">Id, jakie otrzymała wiadomość przy tworzeniu</param>
    private void RemoveMessage(int messageId)
    {
        int messageIndex = FindMessageIndex(messageId);
        if (messageIndex != -1)
        {
            GameObject toRemove = messages[messageIndex].gameObject;
            messages.RemoveAt(messageIndex);
            Destroy(toRemove);

            RepositionElements();
        }
        else Debug.LogError("Wiadomość o podanym id(" + messageId + ") nie istnieje");
    }

    /// <summary>
    /// Niszczy wiadomość
    /// </summary>
    /// <param name="identity">Identyfikator nadany wiadomości przy tworzeniu</param>
    public void RemoveMessage(string identity)
    {
        //Sprawdza, czy słownik zawiera podany identyfikator
        if (identities.ContainsKey(identity))
        {
            RemoveMessage(identities[identity]);
            identities.Remove(identity);
        }
        else Debug.LogError("Próba usunięcia wiadomości, która nie istnieje (identyfikator: " + identity + ")");
    }

    /// <summary>
    /// Tworzenie nowej wiadomości
    /// </summary>
    /// <param name="message">Tekst wiadomości</param>
    /// <param name="type">Rozmiar wiadomości, jej wielkość</param>
    /// <param name="lifeSpan">Czas życia wiadomości</param>
    public void AddMessage(string message, MessageType type, float lifeSpan = 5f)
    {
        AddMessage(message, type, ";", lifeSpan);
    }

    /// <summary>
    /// Tworzenie nowej wiadomośći
    /// </summary>
    /// <param name="message">Tekst wiadomości</param>
    /// <param name="type">Rozmiar wiadomości, jej wielkość</param>
    /// <param name="identity">Identyfikator wiadomości</param>
    /// <param name="lifeSpan">Czas życia wiadomości</param>
    public void AddMessage(string message, MessageType type, string identity, float lifeSpan = 5f)
    {
        GameObject newMessage;
        if (type == MessageType.ShortMessage) newMessage = Instantiate(shortMessage, gameObject.GetComponent<Transform>());
        else if (type == MessageType.ShortMessage) newMessage = Instantiate(mediumMessage, gameObject.GetComponent<Transform>());
        else newMessage = Instantiate(longMessage, gameObject.GetComponent<Transform>());

        //Ustalenie pozycji na osi Y
        float y = 0;
        if (messages.Count > 0)
        {
            Vector2 anchoredPosition = messages[messages.Count - 1].GetComponent<RectTransform>().anchoredPosition;
            Vector2 size = messages[messages.Count - 1].GetComponent<RectTransform>().sizeDelta;
            y = anchoredPosition.y - size.y;
        }
        newMessage.GetComponent<RectTransform>().anchoredPosition = new Vector2(newMessage.GetComponent<RectTransform>().anchoredPosition.x, y);

        //Ustalenie właściwości wiadomości
        int messageId = GetNewId();
        if (identity.Equals(";")) identity = messageId.ToString();
        newMessage.GetComponent<MessageLifecycle>().StartLifeCycle(identity, messageId, lifeSpan);
        newMessage.GetComponent<MessageLifecycle>().ChangeMessage(message);
        messages.Add(newMessage);

        //Dodanie wiadomości do słownika
        identities.Add(identity, messageId);
    }

    /// <summary>
    /// Aktualizuje istniejącą wiadomość
    /// </summary>
    /// <param name="message">Treść nowej wiadomości</param>
    /// <param name="identity">Identyfikator wiadomości</param>
    public void UpdateMessage(string message, string identity)
    {
        int index = FindMessageIndex(identity);

        if (index != -1)
        {
            messages[index].GetComponent<MessageLifecycle>().ChangeMessage(message);
        }
        else AddMessage(message, MessageType.MediumMessage, identity);
    }

    /// <summary>
    /// Odnajduje indeks, jaki ma dana wiadomość na liści
    /// </summary>
    /// <param name="messageId">Id, jakie otrzymała wiadomość przy tworzeniu</param>
    /// <returns>Indeks wiadomości na liście</returns>
    private int FindMessageIndex(int messageId)
    {
        for(int i = 0; i < messages.Count; i++)
        {
            if (messages[i].GetComponent<MessageLifecycle>().messageId == messageId) return i;
        }

        return -1;
    }

    /// <summary>
    /// Odnajduje indeks, jaki ma dana wiadomość na liście
    /// </summary>
    /// <param name="identity">Identyfikator wiadomości</param>
    /// <returns></returns>
    private int FindMessageIndex(string identity)
    {
        if (identities.ContainsKey(identity)) return FindMessageIndex(identities[identity]);

        return -1;
    }

    /// <summary>
    /// Zwraca unikatowe id wiadomości
    /// </summary>
    /// <returns>Unikatowe id</returns>
    private int GetNewId()
    {
        int id = Random.Range(0, 1000);
        while (identities.ContainsValue(id)) id = Random.Range(0, 1000);

        return id;
    }

    /// <summary>
    /// Przesunięcie wiadomości tak, by nie powstawała przerwa
    /// </summary>
    private void RepositionElements()
    {
        if (messages.Count > 0)
        {
            messages[0].GetComponent<RectTransform>().anchoredPosition = new Vector2(messages[0].GetComponent<RectTransform>().anchoredPosition.x, 0);

            for (int i = 1; i < messages.Count; i++)
            {
                GameObject message = messages[i];
                Vector2 anchoredPosition = messages[i - 1].GetComponent<RectTransform>().anchoredPosition;
                Vector2 size = messages[i - 1].GetComponent<RectTransform>().sizeDelta;
                float y = anchoredPosition.y - size.y;

                message.GetComponent<RectTransform>().anchoredPosition = new Vector2(message.GetComponent<RectTransform>().anchoredPosition.x, y);
            }
        }
    }
}
