using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class PopupSystem : MonoBehaviour
{
    public static PopupSystem instance;

    [SerializeField, Tooltip("Obiekt przechowujący wzór IconBox-u")] private GameObject IconBoxPrefab;
    [SerializeField, Tooltip("Obiekt przechowujący wzór InfoBox-u")] private GameObject InfoBoxPrefab;
    [SerializeField, Tooltip("Obiekt przechowujący wzór QuestionBox-u")] private GameObject QuestionBoxPrefab;
    [SerializeField, Tooltip("Obiekt przechowujący wzór FormattedBox-u")] private GameObject FormattedBoxPrefab;

    /// <summary>
    /// Lista przechowująca wszystkie popup-y w kolejce w celu wyświetlenia ich, gdy na ekranie zrobi się miejsce
    /// </summary>
    private List<Popup> popupQueue = new List<Popup>();
    /// <summary>
    /// Lista kontrolerów popup-ów wyświetlonych na ekranie
    /// </summary>
    private List<PopupBox> showedPopups = new List<PopupBox>();
    /// <summary>
    /// Lista typów box-ów wyświetlanych przez PopupSystem
    /// </summary>
    private readonly Dictionary<Type, Type> boxTypes = new Dictionary<Type, Type>()
    {
        { typeof(IconBox), typeof(IconPopup) },
        { typeof(InfoBox), typeof(InfoPopup) },
        { typeof(QuestionBox), typeof(QuestionPopup) },
        { typeof(FormattedBox), typeof(FormattedPopup) }
    };
    public Dictionary<Type, BoxPool> boxPools = new Dictionary<Type, BoxPool>();

    #region Inicjalizacja

    private void OnEnable()
    {
        instance = this;
    }

    private void OnDestroy()
    {
        DestroyPools();
    }

    void Start()
    {

    }

    /// <summary>
    /// Tworzy pule obiektów o wszystkich dostępnych typach
    /// </summary>
    public void CreatePools()
    {
        boxPools.Add(typeof(IconBox), new BoxPool(gameObject, IconBoxPrefab, Keys.Popups.SHOWED_AMOUNT));
        boxPools.Add(typeof(InfoBox), new BoxPool(gameObject, InfoBoxPrefab, Keys.Popups.SHOWED_AMOUNT));
        boxPools.Add(typeof(QuestionBox), new BoxPool(gameObject, QuestionBoxPrefab, Keys.Popups.SHOWED_AMOUNT));
        boxPools.Add(typeof(FormattedBox), new BoxPool(gameObject, FormattedBoxPrefab, Keys.Popups.SHOWED_AMOUNT));
    }

    /// <summary>
    /// Niszczy pule obiektów o wszystkich dostępnych typach
    /// </summary>
    public void DestroyPools()
    {
        foreach (BoxPool pool in boxPools.Values.ToList()) pool.Deinit();
    }

    /// <summary>
    /// Inicjuje pule wszystkich dostępnych typów popup-ów
    /// </summary>
    public void InitPools()
    {
        foreach (BoxPool pool in boxPools.Values.ToList()) pool.Init();
    }

    #endregion Inicjalizacja

    #region Obsługa popup-ów

    /// <summary>
    /// Dodaje popup-a do kolejki wyświetlania. Zostanie on wyświetlony natychmiast, gdy będzie na niego miejsce na ekranie
    /// </summary>
    /// <param name="popup">Popup do pokazania</param>
    public void AddPopup(Popup popup)
    {
        if (popup == null)
        {
            Debug.LogError("Nastąpiła próba dodania pustego popup-u");
            return;
        }
        if (popupQueue.Count > 0)
        {
            //Wstawianie popup-u na odpowiednie miejsce w zależności od priorytetu
            for (int i = 0; i < popupQueue.Count; i++)
            {
                if (popupQueue[i].priority < popup.priority)
                {
                    popupQueue.Insert(i, popup);
                    break;
                }
                //Jeżeli ma niższy priorytet, bądź taki sam jak obiekty na końcu listy, popup jest wstawiany na sam koniec kolejki
                else if (i == popupQueue.Count - 1)
                {
                    popupQueue.Add(popup);
                    break;
                }
            }
        }
        else popupQueue.Add(popup);

        CheckScreenAccessibility();
    }

    /// <summary>
    /// Zamyka podany popupbox
    /// </summary>
    /// <param name="box">Popupbox, który ma zostać zamknięty</param>
    public void ClosePopup(PopupBox box)
    {
        showedPopups.Remove(box);
        boxPools[box.GetType()].ReturnObject(box.gameObject);
        CheckScreenAccessibility();
    }

    /// <summary>
    /// Zamyka popupbox, który zawiera podany pattern
    /// </summary>
    /// <param name="source">Popup, który jest zawarty w popupbox-ie</param>
    public void ClosePopup(Popup source)
    {
        showedPopups.FirstOrDefault(box => box.source == source)?.Close();
    }

    /// <summary>
    /// Wyświetla okno z popup-em niezależnie od tego, czy na ekranie jest na niego miejsce
    /// </summary>
    /// <param name="popup">Popup, który ma zostać wyświetlony</param>
    public void ForcePopup(Popup popup)
    {
        if (popup == null) return;

        GameObject popupBox = boxPools[boxTypes.FirstOrDefault(x => x.Value == popup.GetType()).Key].TakeObject(); //Wyciąganie popupbox-u z odpowiedniej puli w zależności od popup-u źródłowego

        //Upewniamy się, że popupbox nie jest pusty
        if (popupBox != null)
        {
            popupBox.GetComponent<Transform>().SetAsLastSibling();
            PopupBox box = popupBox.GetComponent<PopupBox>();
            box.Init(popup); //Inicjalizacja popupbox-a według podanego patternu

            showedPopups.Add(box); //Dodawanie box-a do listy wyświetlonych box-ów
        }
        else Debug.LogError("Nie udało się zainicjować popup-u");
    }

    /// <summary>
    /// Sprawdza, czy ilość popup box-ów danego typu jest mniejsza, niż wskazana w ustawieniach. Jeżeli tak, wyświetla popup danego typu wyszukując go w kolejce.
    /// </summary>
    private void CheckScreenAccessibility()
    {
        //Dla każdego z typów sprawdzamy wszystkie wyświetlone obiekty
        foreach(Type type in boxTypes.Keys)
        {
            int amount = CountShowedPopups(type);
            if (amount < Keys.Popups.SHOWED_AMOUNT) ShowPopup(boxTypes[type]);
        }
    }

    /// <summary>
    /// Wyszukuje popup o podanym typie w kolejce a następnie wysyła go do wyświetlenia
    /// </summary>
    /// <param name="type"></param>
    private void ShowPopup(Type type)
    {
        Popup toShow = null;

        for(int i = 0; i < popupQueue.Count; i++)
        {
            Type pt = popupQueue[i].GetType();
            //Jeżeli zgadza się typ
            if (pt.Equals(type))
            {
                if (popupQueue[i].showDelay <= 0)
                {
                    toShow = popupQueue[i];
                    popupQueue.RemoveAt(i);

                }
                else if (GameplayController.instance.session.gameState == GameState.running) popupQueue[i].showDelay -= Time.deltaTime;

                break;
            }
        }

        ForcePopup(toShow);
    }  

    #endregion Obsługa popup-ów

    /// <summary>
    /// Oblicza liczbę popupbox-ów o podanym typie
    /// </summary>
    /// <param name="type">Typ popupbox-ów</param>
    /// <returns>Ilość popupbox-ów o padanym typie</returns>
    public int CountShowedPopups(Type type)
    {
        int amount = 0;

        foreach (PopupBox box in showedPopups)
        {
            if (box.GetType().Equals(type)) amount++;
        }

        return amount;
    }

    /// <summary>
    /// Zlicza popup-y o danym typie znajdujące się w kolejce
    /// </summary>
    /// <param name="type">Typ popup-ów</param>
    /// <returns>Liczba popup-ów o podanym typie znajdujących się w kolejce</returns>
    public int CountPopupsInQueue(Type type)
    {
        int count = 0;

        foreach(Popup popup in popupQueue)
        {
            if (popup.GetType().Equals(type)) count++;
        }

        return count;
    }
}
