using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class FormattedPopup : Popup
{
    /// <summary>
    /// Tytuł wyświetlanego na górze popupbox-u
    /// </summary>
    public string title { get; private set; }
    /// <summary>
    /// Szerokość popupbox-u
    /// </summary>
    public float width;
    /// <summary>
    /// Szerokość zawartości do przesuwania wyświetlana przez popupbox
    /// </summary>
    public float contentWidth;
    /// <summary>
    /// Wysokość popupbox-u
    /// </summary>
    public float height;
    /// <summary>
    /// Wysokość zawartości do przesuwania wyświetlana przez popupbox
    /// </summary>
    public float contentHeight;
    /// <summary>
    /// Wysokość linii pojedyńczego elementu
    /// </summary>
    public float lineHeight;
    /// <summary>
    /// Symbol reprezentujący pustą przestrzeń
    /// </summary>
    public string emptySymbol;
    /// <summary>
    /// Lista przechowująca sformatowany wygląd popupu. Główna lista zawiera mniejsze listy. Każda z mniejszych list symbolizuje jedną linijkę o wysokości równej lineHeight.
    /// Szerokość jednego elementu można obliczyć ze wzoru: ilość_elementów_jednego_typu_w_linijce / ilość_wszystkich_elementów_w_linijce * szerokość_linijki
    /// Elementy mogą mieć tylko kształt prostokątów (włącza się w to kwadrat). By stworzyć element wyższy niż jedna linia należy jego symbol umieścić na listach reprezentujących kolejne linijki.
    /// Te same symbole muszą się ze sobą stykać. Dotyczy to zarówno tej samej linijki jak i elementów rozciągniętych na kilka linijek. Jeżeli Nie stykają się ze sobą, użyte zostanie tylko pierwszeich wystąpienie (łącznie ze wszystkichmi symbolami stykającymi się). Nie dotyczy to symbolu pustki.
    /// Jeżeli rozmiary elementu rozciągającego się na kilka linijek nie pokrywają się w poszczególnych linijkach, zostanie użyty największy określony rozmiar. Pozwala to stworzyć elementy nachodzące na siebie. Na wierzchu będzie zawsze element, który został później wczytany.
    /// Kolejność wczytywania elementów jest pobierana ze słownika elementTypes. Jest ona zgodna z kolejnością dodawania elementów.
    /// </summary>
    public List<List<string>> formatedPositions = new List<List<string>>();

    /// <summary>
    /// Słownik mapujący symbol elementu na jego typ
    /// </summary>
    private Dictionary<string, FormattedElementType> elementTypes = new Dictionary<string, FormattedElementType>();
    /// <summary>
    /// Słownik mapujący symbol przycisku na pare: napis wyświetlany na przycisku i akcję wywoływaną po wciśnięciu go
    /// </summary>
    private Dictionary<string, Tuple<string, PopupAction>> buttons = new Dictionary<string, Tuple<string, PopupAction>>();
    /// <summary>
    /// Słownik mapujący symbol tekstu na parę: treść tekstu i akcję wywoływaną po wciśnięciu go
    /// </summary>
    private Dictionary<string, Tuple<string, PopupAction>> textes = new Dictionary<string, Tuple<string, PopupAction>>();
    /// <summary>
    /// Słownik mapujący symbole sprit-ów na parę: sprite i akcję wywoływaną po naciśnięciu go
    /// </summary>
    private Dictionary<string, Tuple<Sprite, PopupAction>> sprites = new Dictionary<string, Tuple<Sprite, PopupAction>>();

    /// <summary>
    /// Inicjowanie popup-u typu Formated
    /// </summary>
    /// <param name="title">Tytuł wyświetlany na górze popup-u na pasku razem z przyciskiem umożliwiającym wyjście. Jeżeli nie podano wartości, pasek tytułu całkowicie znika z popup-u</param>
    /// <param name="lifeSpan">Czas wyświetlania FormatedBox-u</param>
    /// <param name="onOpen">Akcje wywoływane po wyświetleniu FormatedBox-u</param>
    /// <param name="onClick">Akcje wywoływane po naciśnięciu na FormatedBox</param>
    /// <param name="onClose">Akcje wywoływane po zniszczeniu FormatedBox-u</param>
    public FormattedPopup(string title = "", float lifeSpan = Keys.Popups.MAX_EXISTING_TIME, PopupAction onOpen = null, PopupAction onClose = null, PopupAction onClick = null)
        : base(lifeSpan, onOpen, onClose, onClick)
    {
        this.title = title;
        width = Keys.Popups.Formated.DEFAULT_WIDTH;
        contentWidth = Keys.Popups.Formated.DEFAULT_CONTENT_WIDTH;
        height = Keys.Popups.Formated.DEFAULT_HEIGHT;
        contentHeight = Keys.Popups.Formated.DEFAULT_CONTENT_HEIGHT;
        lineHeight = Keys.Popups.Formated.DEFUALT_LINE_HEIGHT;
        emptySymbol = Keys.Popups.Formated.EMPTY_SYMBOL;
    }

    /// <summary>
    /// Dodaje przycisk do przestrzeni rozpoznawanych symboli przez FormatedPopup
    /// </summary>
    /// <param name="name">Napis wyświetlany na przycisku</param>
    /// <param name="symbol">Symbol pod jakim jest rozpoznawany przycisk</param>
    /// <param name="onClick">Akcja wywoływana po wciśnięciu przycisku</param>
    public void AddButton(string name, string symbol, PopupAction onClick)
    {
        if (!elementTypes.ContainsKey(symbol))
        {
            elementTypes.Add(symbol, FormattedElementType.Button);
            buttons.Add(symbol, Tuple.Create(name, onClick));
        }
        else Debug.LogError("Na liście elementów FormatedPopup-u istnieje już symbol " + symbol);
    }

    /// <summary>
    /// Dodaje tekst do przestrzeni rozpoznawanych symboli przez FormatedPopup
    /// </summary>
    /// <param name="text">Tekst wyświetlany przez element</param>
    /// <param name="symbol">Symbol pod jakim jest rozpoznawany tekst</param>
    /// <param name="onClick">Akcja wywoływana po naciśnięciu tekstu</param>
    public void AddText(string text, string symbol, PopupAction onClick)
    {
        if (!elementTypes.ContainsKey(symbol))
        {
            elementTypes.Add(symbol, FormattedElementType.Text);
            textes.Add(symbol, Tuple.Create(text, onClick));
        }
        else Debug.LogError("Na liście elementów FormatedPopup-u istnieje już symbol " + symbol);
    }

    /// <summary>
    /// Dodaje obrazek do przestrzeni rozpoznawanych symboli przez FormatedPopup
    /// </summary>
    /// <param name="sprite">Obrazek, który zostanie wyświetlony w elemencie</param>
    /// <param name="symbol"></param>
    /// <param name="onClick"></param>
    public void AddSprite(Sprite sprite, string symbol, PopupAction onClick)
    {
        if (!elementTypes.ContainsKey(symbol))
        {
            elementTypes.Add(symbol, FormattedElementType.Sprite);
            sprites.Add(symbol, Tuple.Create(sprite, onClick));
        }
        else Debug.LogError("Na liście elementów FormatedPopup-u istnieje już symbol " + symbol);
    }

    /// <summary>
    /// Zwraca symbol odpowiadający numerowi na liście symbolii
    /// </summary>
    /// <param name="id">Numer na liście symbolii</param>
    /// <returns>Symbol odpowiadający numerowi na liście symbolii</returns>
    public string GetSymbol(int id)
    {
        if (id >= 0 && id < elementTypes.Count) return elementTypes.Keys.ElementAt(id);
        else
        {
            Debug.LogError("Podano nieprawidłowy numer elementu (" + id + ")");
            return emptySymbol;
        }
    }

    /// <summary>
    /// Zwraca ilość symbol zawartych w słowniku typów elementów
    /// </summary>
    /// <returns>Ilość symboli w słowniku</returns>
    public int GetSymbolsCount()
    {
        return elementTypes.Keys.Count;
    }

    /// <summary>
    /// Wyszukuje typ elementu odpowiadający podanemu symbolowii
    /// </summary>
    /// <param name="symbol">Symbol poszukiwanego elementu</param>
    /// <returns>Typ elementu</returns>
    public FormattedElementType GetElementType(string symbol)
    {
        if (elementTypes.ContainsKey(symbol)) return elementTypes[symbol];
        else if (symbol.Equals(emptySymbol)) return FormattedElementType.Empty;
        else
        {
            Debug.LogError("Nie istnieje element o symbolu " + symbol);
            return FormattedElementType.Empty;
        }
    }

    /// <summary>
    /// Wyszukuje właściwości przycisku o podanym symbolu
    /// </summary>
    /// <param name="symbol">Symbol pod jakim zakodowany jest przycisk</param>
    /// <returns>Para zmiennych: napis na przycisku i akcja wywoływana po jego wciśnięciu</returns>
    public Tuple<string, PopupAction> GetButton(string symbol)
    {
        if (buttons.ContainsKey(symbol)) return buttons[symbol];
        else
        {
            Debug.LogError("Nie istnieje przycisk o symbolu " + symbol);
            return null;
        }
    }

    /// <summary>
    /// Wyszukuje właściwości tekstu o podanym symbolu
    /// </summary>
    /// <param name="symbol">Symbol pod jakim zakodowany jest tekst</param>
    /// <returns>Para zmiennych: tekst i akcja wywoływana po jego wciśnięciu</returns>
    public Tuple<string, PopupAction> GetText(string symbol)
    {
        if (textes.ContainsKey(symbol)) return textes[symbol];
        else
        {
            Debug.LogError("Nie istnieje tekst o symbolu " + symbol);
            return null;
        }
    }

    /// <summary>
    /// Wyszukuje właściwości sprite-a o podanym symbolu
    /// </summary>
    /// <param name="symbol">Symbol pod jakim zakodowany jest sprite</param>
    /// <returns>Para zmiennych: sprite i akcja wywoływana po jego wciśnięciu</returns>
    public Tuple<Sprite, PopupAction> GetSprite(string symbol)
    {
        if (sprites.ContainsKey(symbol)) return sprites[symbol];
        else
        {
            Debug.LogError("Nie istnieje sprite o symbolu " + symbol);
            return null;
        }
    }

    /// <summary>
    /// Funkcja oblicza wielkość elementu o podanym symbolu
    /// </summary>
    /// <param name="symbol">Symbol elementu</param>
    /// <returns>Wielkość elementu o podanym symbolu</returns>
    public Vector2 GetElementSize(string symbol)
    {
        float width = 0f;
        float height = 0f;

        if (!symbol.Equals(emptySymbol))
        {
            foreach (List<string> row in formatedPositions)
            {
                int inRowElements = 0;

                foreach (string element in row)
                {
                    //Jeżeli element znajduje się na danym miejscu, zwiększamy zmienną zliczającą elementy w danej linijce
                    if (element.Equals(symbol)) inRowElements++;
                    //Jeżeli wcześniej znaleziono w linijce element o zadanym symbolu, a teraz nastąpił inny symbol, oznacza to, że dalej już nie ma szukanego przez nas symbolu, więc możemy zakończyć sprawdzanie linijki
                    else if (inRowElements > 0) break;
                }

                //Obliczanie szerokości elementu na podstawie ilości symboli w jednej linijce
                width = inRowElements / Mathf.Max(1f, row.Count()) * contentWidth;

                //Jeżeli nie znaleziono wcześniej zadanego elementu, a liczba elementów w danej linijce jest większa od zera, zwiększamy wysokość elementu o wysokość linijki
                if (inRowElements > 0) height += lineHeight;
                //Jeżeli odnaleziono wcześniej element o zadanym symbolu (wysokość elementu jest większa od zera), a w danej linijce nie ma żadnego takiego elementum, nie powinno go już być nigdzie dalej. Oznacza to, że możemy zakończyć przeszukiwanie listy
                else if (height > 0 && inRowElements == 0) break;
            }
        }

        return new Vector2(width, height);
    }

    /// <summary>
    /// Oblicza położenie lewego górnego rogu elementu
    /// </summary>
    /// <param name="symbol">Symbol elementu</param>
    /// <returns>Pozycja lewego górnego rogu elementu</returns>
    public Vector2 GetElementPosition(string symbol)
    {
        float x = contentWidth;
        float y = 0f;

        if(!symbol.Equals(emptySymbol))
        {
            int rows = 0;

            for(int i = 0; i < formatedPositions.Count; i++)
            {
                List<string> row = formatedPositions[i];
                int elements = 0;

                for(int j = 0; j < row.Count; j++)
                {
                    string element = row[j];
                    if (element.Equals(symbol))
                    {
                        if (j * (contentWidth / row.Count) < x) x = j * (contentWidth / row.Count); //Przesuwamy położenie lewej strony tylko wtedy, gdy nowy x jest mniejszy od starego. Jest tak dlatego, że szukamy najbardziej wysuniętego elementu na lewo
                        if (rows == 0) y = i * lineHeight; //Jeżeli jest to pierwszy wiersz, w którym występuje element o podanym symbolu, możemy z niego wyliczyć pozycję y lewego górnego rogu elementu
                        rows++;
                        elements++;

                        break; //Tylko pierwszy element w wierzu liczy się, ponieważ chcemy określić lewy górny róg
                    }
                }

                if (rows > 0 && elements == 0) break; //Jeżeli elementty były znajdowane w poprzednich wierszach (rows > 0) a w obecnym ich nie ma (elements == 0), oznacza to, że nie powinno ich być już nigdzie dalej, więc możemy zakończyć pętle
            }
        }

        return new Vector2(x, y);
    }
}