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
    [SerializeField, Tooltip("Obiekt przechowujący wzór QuestionBox-u")] private GameObject QuestionBoxPrefab;
    [SerializeField, Tooltip("Obiekt przechowujący wzór ChanceBox-u")] private GameObject ChanceBoxPrefab;
    [SerializeField, Tooltip("Obiekt przechowujący wzór PlayerSelectorBox-u")] private GameObject PlayerSelectorPrefab;

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
        { typeof(QuestionBox), typeof(QuestionPopup) },
        { typeof(ChanceBox), typeof(ChancePopup) },
        { typeof(PlayerSelectorBox), typeof(PlayerSelectorPopup) }
    };
    public Dictionary<Type, BoxPool> boxPools = new Dictionary<Type, BoxPool>();

    public IconBox DiceBox { get; private set; }
    /// <summary>
    /// Flaga określająca, czy po zrobieniu się miejsca na ekranie, bądź dodaniu popupu, gdy to miejsce jest, może pojawić się on na ekranie
    /// </summary>
    private bool canShowNewPopups = true;

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
        boxPools.Add(typeof(IconBox), new BoxPool(gameObject, IconBoxPrefab, Keys.Popups.ICON_SHOWED_AMOUNT));
        boxPools.Add(typeof(QuestionBox), new BoxPool(gameObject, QuestionBoxPrefab, 1));
        boxPools.Add(typeof(ChanceBox), new BoxPool(gameObject, ChanceBoxPrefab, 1));
        boxPools.Add(typeof(PlayerSelectorBox), new BoxPool(gameObject, PlayerSelectorPrefab, 1));
    }

    /// <summary>
    /// Niszczy pule obiektów o wszystkich dostępnych typach
    /// </summary>
    public void DestroyPools()
    {
        foreach (BoxPool pool in boxPools.Values.ToList())
            pool.Deinit();
    }

    /// <summary>
    /// Inicjuje pule wszystkich dostępnych typów popup-ów
    /// </summary>
    public void InitPools()
    {
        foreach (BoxPool pool in boxPools.Values.ToList())
            pool.Init();
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
        popupQueue.Add(popup);

        CheckScreenAccessibility();
    }

    /// <summary>
    /// Zamyka podany popupbox
    /// </summary>
    /// <param name="box">Popupbox, który ma zostać zamknięty</param>
    public void ClosePopup(PopupBox box)
    {
        if(showedPopups.Contains(box)) 
           showedPopups.Remove(box);
        boxPools[box.GetType()].ReturnObject(box.gameObject);
        CheckScreenAccessibility();
        RepositionPopups();
    }

    /// <summary>
    /// Zamyka popupbox, który zawiera podany pattern
    /// </summary>
    /// <param name="source">Popup, który jest zawarty w popupbox-ie</param>
    public void ClosePopup(Popup source)
    {
        if(DiceBox != null && DiceBox.source == source)
        {
            DiceBox.Close();
            DiceBox = null;
        }
        else
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
        foreach(Type type in boxTypes.Values)
        {
            int amount = CountShowedPopups(type);
            int inQueue = CountPopupsInQueue(type);

            if (type.Equals(typeof(IconPopup)))
            {
                if (amount < Keys.Popups.ICON_SHOWED_AMOUNT && inQueue > 0)
                    ShowPopup(type);
                else if(inQueue > 0)
                {
                    CloseWhatNeeded(type);
                    if (amount < Keys.Popups.ICON_SHOWED_AMOUNT)
                        ShowPopup(type);
                }
            }
            else
            {
                if(inQueue > 0)
                    CloseWhatNeeded(type);
                if (amount < 1)
                    ShowPopup(type);
            }
        }
    }

    /// <summary>
    /// Wyszukuje popup o podanym typie w kolejce a następnie wysyła go do wyświetlenia
    /// </summary>
    /// <param name="type"></param>
    private void ShowPopup(Type type)
    {
        if (!canShowNewPopups)
            return;

        for(int i = 0; i < popupQueue.Count; i++)
        {
            Type pt = popupQueue[i].GetType();
            //Jeżeli zgadza się typ
            if (pt.Equals(type))
            {
                ForcePopup(popupQueue[i]);
                popupQueue.RemoveAt(i);

                break;
            }
        }
    }

    /// <summary>
    /// Jeżeli jest możliwe, zamyka obecnie wyświetlony popup, o podanym typie
    /// </summary>
    /// <param name="type">Typ popupu, który ma zostać zamknięty</param>
    private void CloseWhatNeeded(Type type)
    {
        foreach(PopupBox box in showedPopups)
        {
            if(box.source.GetType().Equals(type) && box.source.CloseMode <= AutoCloseMode.NewAppears)
            {
                box.Close();
                break;
            }
        }
    }

    public void ShowDice(Popup.PopupAction diceAction)
    {
        IconPopup dicePopup = new IconPopup(IconPopupType.Dice, diceAction);

        GameObject dice = boxPools[typeof(IconBox)].TakeObject();
        DiceBox = dice.GetComponent<IconBox>();
        DiceBox.Init(dicePopup);
        dice.GetComponent<Animator>().SetBool("Dice", true);
    }

    /// <summary>
    /// Zamyka wszystkie popupy o trybie zamknięcia równym, bądź niższym od podanego.
    /// Na czas zamykania blokuje możliwość wyświetlenia nowych popupów
    /// </summary>
    /// <param name="closeMode">Tryb zamknięcia popupów</param>
    public void ClosePopups(AutoCloseMode closeMode)
    {
        canShowNewPopups = false;

        //Zamykanie obecnie wyświetlonych popupów
        List<PopupBox> toClose = new List<PopupBox>();
        foreach(PopupBox box in showedPopups)
        {
            if (box.source.CloseMode <= closeMode)
                toClose.Add(box);
        }
        foreach (PopupBox box in toClose)
            ClosePopup(box);

        List<Popup> toRemove = new List<Popup>();

        //Usuwanie popupów jeszcze nie wyświetlonych
        foreach(Popup popup in popupQueue)
        {
            if(popup.CloseMode <= closeMode)
            {
                toRemove.Add(popup);
                popup.onClose?.Invoke(popup);
            }
        }

        
        foreach (Popup popup in toRemove)
            popupQueue.Remove(popup);

        canShowNewPopups = true;

        CheckScreenAccessibility();
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
            if (box.source.GetType().Equals(type)) amount++;
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

    /// <summary>
    /// Zwraca posortowaną listę pozycję wszystkich obecnie wyświetlonych popupboxów o danym typie
    /// </summary>
    /// <param name="type">Typ popupboxów</param>
    /// <returns>Posortowana lista pozycji popupboxów</returns>
    public List<int> GetShowedPositions(Type type)
    {
        List<int> positions = new List<int>();

        foreach(PopupBox box in showedPopups)
        {
            if (box.GetType().Equals(type))
                positions.Add(box.CurrentPosition);
        }

        positions.OrderBy(i => i);

        return positions;
    }

    /// <summary>
    /// Repozycjonowanie obecnie wyświetlonych popupów, by znalazły się na dobrych miejscach
    /// </summary>
    public void RepositionPopups()
    {
        foreach (PopupBox box in showedPopups)
            box.Reposition();
    }
}
