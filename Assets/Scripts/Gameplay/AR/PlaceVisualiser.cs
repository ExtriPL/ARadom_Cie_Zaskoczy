using GoogleARCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceVisualiser : MonoBehaviour
{
    /// <summary>
    /// Model aktualnie wyświetlany na polu
    /// </summary>
    public GameObject model;
    /// <summary>
    /// Ustawienia pola
    /// </summary>
    public Field field;
    /// <summary>
    /// Numer pola na planszy
    /// </summary>
    [SerializeField] private int placeIndex;

    private ARController ARController;

    /// <summary>
    /// Lista graczy znajdujących się aktualnie na polu
    /// </summary>
    private List<Player> playersOnField = new List<Player>();
    /// <summary>
    /// Lista materiałów obiektów tworzących światła na polu
    /// </summary>
    private List<Material> backlightsList = new List<Material>();

    private Player activePlayer = null;

    private void Update()
    {
        Shine();
    }

    #region Inicjalizacja

    /// <summary>
    /// Inicjalizacja pola
    /// </summary>
    /// <param name="field">Pole, które reprezentuje visualiser w przestrzeni AR</param>
    /// <param name="placeIndex">Numer pola, które reprezentuje visualiser w przestrzeni AR</param>
    public void Init(Field field, int placeIndex)
    {
        this.field = field;
        this.placeIndex = placeIndex;

        ARController = GameplayController.instance.arController;

        model = Instantiate(field.GetStartModel(), gameObject.GetComponent<Transform>());

        CreateBacklight();
    }

    /// <summary>
    /// Subskrybcja eventów
    /// </summary>
    public void SubscribeEvents()
    {
        EventManager.instance.onPlayerMove += OnPlayerMove;
        EventManager.instance.onTurnChange += OnTurnChange;
    }

    /// <summary>
    /// Odsubskrybowanie eventów
    /// </summary>
    public void UnsubscribeEvents()
    {
        EventManager.instance.onPlayerMove -= OnPlayerMove;
        EventManager.instance.onTurnChange -= OnTurnChange;
    }

    #endregion Inicjalizacja

    #region Obsługa eventów

    /// <summary>
    /// Zdarzenia wywołoływane, gdy gracz ruszy się
    /// </summary>
    /// <param name="playerName">Nazwa gracza</param>
    /// <param name="fromPlaceIndex">Numer pola, z którego ruszał się gracz</param>
    /// <param name="toPlaceIndex">Numer pola, na które przeszedł gracz</param>
    private void OnPlayerMove(string playerName, int fromPlaceIndex, int toPlaceIndex)
    {
        if (fromPlaceIndex == placeIndex)
        {
            field.OnPlayerLeave(GameplayController.instance.session.FindPlayer(playerName), this);
            playersOnField.Remove(GameplayController.instance.session.FindPlayer(playerName));
            DeactivateField();
        }
        else if (toPlaceIndex == placeIndex)
        {
            field.OnPlayerEnter(GameplayController.instance.session.FindPlayer(playerName), this);
            playersOnField.Add(GameplayController.instance.session.FindPlayer(playerName));
            ActivateField(GameplayController.instance.session.FindPlayer(playerName));
        }

        //Numer pola z jest mniejszy od numeru pola do i numer tego pola (this) zawiera się pomiędzy nimi
        bool betweenMinMax = toPlaceIndex > fromPlaceIndex && placeIndex > fromPlaceIndex && placeIndex <= toPlaceIndex;

        //Numer pola zawiera się międzyy 0 a do
        bool between0To = placeIndex >= 0 && placeIndex <= toPlaceIndex;
        //Numer pola zawiera się między z a ostatnim polem
        bool betweenFrom0 = placeIndex > fromPlaceIndex && placeIndex < Keys.Board.FIELD_COUNT;
        //Numer pola z jest większy od numeru pola do i numer tego pola (this) zawiera się pomiędzy nimi
        bool betweenMaxMin = fromPlaceIndex > toPlaceIndex && (betweenFrom0 || between0To);

        //Jeżeli numer pola jest pomiędzy polami z i do
        if (betweenMinMax || betweenMaxMin) field.OnPlayerPassby(GameplayController.instance.session.FindPlayer(playerName), this);
    }

    /// <summary>
    /// Obsługa eventu zmiany tury
    /// </summary>
    /// <param name="previousPlayerName">Gracz, którego tura się skończyła</param>
    /// <param name="currentPlayerName">Gracz, którego tura się zaczeła</param>
    private void OnTurnChange(string previousPlayerName, string currentPlayerName)
    {
        Player previousPlayer = GameplayController.instance.session.FindPlayer(previousPlayerName);
        Player currentPlayer = GameplayController.instance.session.FindPlayer(currentPlayerName);

        if (playersOnField.Contains(previousPlayer)) DeactivateField();
        if (playersOnField.Contains(currentPlayer)) ActivateField(currentPlayer);
    }

    /// <summary>
    /// Zdarzenie wywoływane po wciśnięciu pola na ekranie
    /// </summary>
    public void OnClick()
    {
        //Usuwanie poprzednich budynkow srodkowych
        foreach (Transform transform in ARController.centerBuilding.GetComponentsInChildren<Transform>()) 
        {
            if (transform.gameObject != ARController.centerBuilding)
            {
                Destroy(transform.gameObject);
            }   
        }
        GameObject centerModel;

        if (field is NormalBuilding)
        {
            centerModel = Instantiate(((NormalBuilding)field).GetTier(((NormalBuilding)field).tiersCount - 1).model, ARController.centerBuilding.GetComponent<Transform>());
        }
        else
        {
            
            centerModel = Instantiate(field.GetStartModel(), ARController.centerBuilding.GetComponent<Transform>());
        }
        centerModel.name = field.GetFieldName();
    }


    #endregion Obsługa eventów

    #region Podświetlenie pól

    /// <summary>
    /// Tworzy obekty, które będą służyły do podświetlania pola, gdy stanie na nim gracz
    /// </summary>
    private void CreateBacklight()
    {
        GameObject backlights = new GameObject("Backlights"); //Obiekt przechowujący światła
        backlights.GetComponent<Transform>().parent = gameObject.GetComponent<Transform>();

        for (int i = 0; i < Keys.Board.BOARD_SIDES; i++)
        {
            //Ustawienia pozycjonowania obiektu
            float distanceFromCenter = Keys.Board.FIELD_WIDTH / 2f / Keys.Board.SCALLING_FACTOR;
            float rotationAngel = 2 * Mathf.PI / Keys.Board.BOARD_SIDES * i + Keys.Board.Backlight.START_ANGEL - GameplayController.instance.arController.GetBuildingRotation(placeIndex);
            Quaternion R = Quaternion.Euler(new Vector3(0f, 0f, -rotationAngel * 180f / Mathf.PI));
            Vector2 v = new Vector2(0f, -distanceFromCenter);
            v = R * v;

            //Tworzenie odpowiedniego obiektu
            GameObject light = GameObject.CreatePrimitive(PrimitiveType.Cube);
            light.GetComponent<Transform>().parent = backlights.GetComponent<Transform>();
            light.GetComponent<Transform>().localPosition = v;
            light.GetComponent<Transform>().rotation = Quaternion.Euler(new Vector3(0f, 0f, -rotationAngel / Mathf.PI * 180));
            light.GetComponent<Transform>().localScale = new Vector3(Keys.Board.FIELD_SIDE_LENGHT / Keys.Board.SCALLING_FACTOR, Keys.Board.Backlight.THICKNESS, Keys.Board.Backlight.THICKNESS);

            //Ustawianie jego startowego koloru
            Material startMaterial = new Material(Resources.Load("Materials/Fade") as Material); //Ładowanie template-a odpowiedniego materiału z folderu Resources i tworzenie na jego podstawie odpowiedniego materiału
            startMaterial.color = new Color(0f, 0f, 1f, 0f);

            light.GetComponent<Renderer>().material = startMaterial;

            backlightsList.Add(light.GetComponent<Renderer>().material);
        }
    }

    /// <summary>
    /// Tworzy on nowa strukturę świateł. Podmienia kolory obiektów pełniących rolę świateł na kolory odpowiadające graczą na liście graczy
    /// </summary>
    private void RecreateLights()
    {
        for(int i = 0; i < playersOnField.Count; i++)
        {
            Color playerColor = playersOnField[i].mainColor;
            backlightsList[i].color = new Color(playerColor.r, playerColor.g, playerColor.b);
        }

        for(int i = playersOnField.Count; i < backlightsList.Count; i++)
        {
            backlightsList[i].color = new Color(1f, 1f, 1f, 0f);
        }
    }

    /// <summary>
    /// Rozświetla pole na kolor aktywnego gracza
    /// </summary>
    /// <param name="activeColor"></param>
    private void ActivateField(Player activePlayer)
    {
        foreach(Material back in backlightsList)
        {
            back.color = activePlayer.mainColor;
        }

        this.activePlayer = activePlayer;
    }

    private void DeactivateField()
    {
        activePlayer = null;
        RecreateLights();
    }

    /// <summary>
    /// Miga światłami obecnie włączonymi
    /// </summary>
    private void Shine()
    {
        if (activePlayer != null)
        {
            foreach (Material back in backlightsList)
            {
                float t = Mathf.Pow(Mathf.Cos(Time.time * (2 * Mathf.PI) / Keys.Board.Backlight.SHINING_PERIOD), 2f);
                back.color = Color.Lerp(activePlayer.mainColor, activePlayer.blinkColor, t);
            }
        }
    }

    #endregion Podświetlenie pól
}