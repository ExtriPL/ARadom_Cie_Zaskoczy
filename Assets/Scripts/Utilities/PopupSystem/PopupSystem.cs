using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupSystem : MonoBehaviour
{
    public static PopupSystem instance;

    /// <summary>
    /// Lista przechowująca wszystkie popup-y w kolejce w celu wyświetlenia ich, gdy na ekranie zrobi się miejsce
    /// </summary>
    private List<Popup> popupQueue = new List<Popup>();

    private List<GameObject> showedPopups = new List<GameObject>();

    /// <summary>
    /// Lista typów box-ów wyświetlanych przez PopupSystem
    /// </summary>
    private List<System.Type> boxTypes = new List<System.Type>()
    {
        typeof(IconBox),
        typeof(InfoBox)
    };

    // Start is called before the first frame update
    void Start()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// Dodaje popup-a do kolejki wyświetlania. Zostanie on wyświetlony natychmiast, gdy będzie na niego miejsce na ekranie
    /// </summary>
    /// <param name="popup">Popup do pokazania</param>
    public void AddPopup(Popup popup)
    {
        if (popup == null) return;
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

    public void ForcePopup(Popup popup)
    {
        if (popup == null) return;

        //Różne sposoby wyświetlania popup-u w zależności od jego typu
        if (popup is IconPopup)
        {

        }
        else if(popup is InfoPopup)
        {

        }
    }

    private void CheckScreenAccessibility()
    {

    }

    /// <summary>
    /// Wyszukuje popup o podanym typie w kolejce a następnie wysyła go do wyświetlenia
    /// </summary>
    /// <param name="type"></param>
    private void ShowPopup(System.Type type)
    {
        Popup toShow = null;

        for(int i = 0; i < popupQueue.Count; i++)
        {
            if(popupQueue[i].GetType().Equals(type))
            {
                toShow = popupQueue[i];
                popupQueue.RemoveAt(i);
                break;
            }
        }

        ForcePopup(toShow);
    }
}
