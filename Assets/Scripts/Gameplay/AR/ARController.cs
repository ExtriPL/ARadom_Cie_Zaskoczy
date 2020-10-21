using GoogleARCore;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

#if UNITY_EDITOR
using Input = GoogleARCore.InstantPreviewInput;
#endif

public class ARController : MonoBehaviour, IEventSubscribable
{
    /// <summary>
    /// Obiekt przechowujący odwołanie do planszy wyświetlanej przez AR
    /// </summary>
    private GameObject board;
    /// <summary>
    /// Obiekt przechowujący model budynku wyświetlanego na środku planszy
    /// </summary>
    public GameObject centerBuilding;
    /// <summary>
    /// Lista wszystkich pól wyświetlonych na planszy
    /// </summary>
    private List<PlaceVisualiser> places = new List<PlaceVisualiser>();
    /// <summary>
    /// Lista wczytanych obrazów na scenie
    /// </summary>lse
    private List<AugmentedImage> tempPlaceImages = new List<AugmentedImage>();

    /// <summary>
    /// Panel z informacjami o wybranym budynku
    /// </summary>
    public GameObject buildingInfoPanel;

    #region Inicjalizacja

    public void UpdateAR()
    {
        FindTriggers();
        OnScreenClick();
    }

    /// <summary>
    /// Inicjalizuje w przestrzeni AR plansze i pola się na niej znajdujące
    /// </summary>
    public void InitBoard()
    {
        board = new GameObject("Board"); //Tworzenie instancji planszy
        board.GetComponent<Transform>().parent = gameObject.GetComponent<Transform>();

        //Tworzenie obiektu przechowującego środkowy budynek
        centerBuilding = new GameObject("centerBuilding");
        centerBuilding.GetComponent<Transform>().parent = board.GetComponent<Transform>();
        float buildingScaleFactor = Keys.Board.SCALLING_FACTOR * Keys.Board.CENTER_BUILDING_SCALE_MULTIPLIER;
        centerBuilding.GetComponent<Transform>().localScale *= buildingScaleFactor;
        centerBuilding.AddComponent<BoxCollider>();
        centerBuilding.GetComponent<BoxCollider>().size = (new Vector3(Keys.Board.FIELD_WIDTH, Keys.Board.FIELD_WIDTH, 0f /*Keys.Board.FIELD_HEIGHT*/)) / (1.8f * Keys.Board.SCALLING_FACTOR);
        centerBuilding.GetComponent<BoxCollider>().center = new Vector3(0f, 0f, -Keys.Board.FIELD_WIDTH / Keys.Board.SCALLING_FACTOR / 2f) / 1.8f;
        centerBuilding.GetComponent<BoxCollider>().isTrigger = true;
        centerBuilding.AddComponent<CenterVisualiser>().Init();

        //Uzupełnianie planszy budynkami
        for (int i = 0; i < Keys.Board.PLACE_COUNT; i++)
        {
            GameObject field = new GameObject("field" + i); //Tworzenie obiektu pola
            field.GetComponent<Transform>().parent = board.GetComponent<Transform>(); //Przypisywanie obiektu do planszy
            field.AddComponent<PlaceVisualiser>();
            
            field.GetComponent<PlaceVisualiser>().SubscribeEvents();
            places.Add(field.GetComponent<PlaceVisualiser>()); //Dodawanie pola do listy pól na wyświetlonych na planszy

            //Dodawanie collidera do wykrywania raycastów
            field.AddComponent<BoxCollider>();
            field.GetComponent<BoxCollider>().size = (new Vector3(Keys.Board.FIELD_WIDTH, Keys.Board.FIELD_HEIGHT, 0f /*Keys.Board.FIELD_WIDTH*/)) / Keys.Board.SCALLING_FACTOR;
            field.GetComponent<BoxCollider>().center = new Vector3(0f, 0f, -Keys.Board.FIELD_WIDTH / Keys.Board.SCALLING_FACTOR / 2f);
            field.GetComponent<BoxCollider>().isTrigger = true;

            //Skalowanie i obracanie, by dostosować planszę
            field.GetComponent<Transform>().localPosition = GetFieldPosition(i);
            field.GetComponent<Transform>().rotation = Quaternion.Euler(0, 0f, GetBuildingRotation(i) / Mathf.PI * 180);
            field.GetComponent<Transform>().localScale *= Keys.Board.SCALLING_FACTOR;

            field.GetComponent<PlaceVisualiser>().Init(GameplayController.instance.board.GetField(i), i);
        }

#if !UNITY_EDITOR
        ToggleBoardVisibility(false);
#endif
    }

    public void SubscribeEvents()
    {

    }

    public void UnsubscribeEvents()
    {
        foreach (PlaceVisualiser visualiser in places) visualiser.UnsubscribeEvents();
    }

    #endregion Inicjalizacja

    #region Obsługa AR

    /// <summary>
    /// Odnajduje triggery i na ich podstawie tworzy instancje PlaceVisualiser
    /// </summary>
    private void FindTriggers()
    {
        Session.GetTrackables<AugmentedImage>(tempPlaceImages, TrackableQueryFilter.Updated); //Pobieranie listy aktualnie widocznych obrazów w kamerze

        foreach (AugmentedImage image in tempPlaceImages)
        {
            //Obiekty są pokazywane tylko wtedy, gdy gra się toczy
            if (GameplayController.instance.session.gameState == GameState.running)
            {
                //Jeżeli obrazek zostanie wykryty plansza jest pokazywana
                if (image.TrackingState == TrackingState.Tracking && image.Name.Equals(Keys.Board.AR_IMAGE_NAME))
                {
                    List<Anchor> anchors = new List<Anchor>();
                    image.GetAllAnchors(anchors);
                    Anchor anchor;
                    if (anchors.Count == 0) //Kotwica służąca do utrzymywania śledzenia przez ARCore, jest powiązana z obrazem w bazie danych
                        anchor = image.CreateAnchor(image.CenterPose);
                    else
                        anchor = anchors[0];

                    ToggleBoardVisibility(true);
                    board.transform.SetParent(anchor.GetComponent<Transform>());
                    board.transform.localPosition = Vector3.zero;
                    board.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                }
                //Gdy obrazek zniknie z pola widzenia, plansza jest ukrywana
                else if (image.TrackingState == TrackingState.Stopped && image.Name.Equals(Keys.Board.AR_IMAGE_NAME))
                {
                    ToggleBoardVisibility(false);
                    board.GetComponent<Transform>().SetParent(gameObject.GetComponent<Transform>());
                }
            }
            else
            {
                ToggleBoardVisibility(false);
                board.GetComponent<Transform>().SetParent(gameObject.GetComponent<Transform>());
            }
        }
    }

    /// <summary>
    /// Mapuje numer pola na jego pozycję względę środka planszy
    /// </summary>
    /// <param name="fieldIndex">Numer pola na planszy</param>
    /// <returns>Położenie pola na planszy w dwóch wymiarach</returns>
    private Vector2 GetFieldPosition(int fieldIndex)
    {
        float theta = (2f * Mathf.PI / Keys.Board.BOARD_SIDES) * (int)(fieldIndex / (Keys.Board.FIELDS_PER_SIDE - 1)); //Kąt odpowiadający za obrót wektora. Zależy on od ilości boków figury i od indeksu danego pola. Odpowiedni kąt jest mnożony przez floor(fieldIndex / (fieldsPerSide - 1)) by obracać wektorem tylko wtedy, gdy indeks znajduje się na kolejnym boku
        Quaternion R = Quaternion.Euler(new Vector3(0f, 0f, -theta * 180f / Mathf.PI)); //Macierz obrotu odpowiedzialna za obrót wektora położenia pola o kąt theta
        Vector2 v0 = new Vector2(Keys.Board.FIELD_WIDTH * (Keys.Board.FIELDS_PER_SIDE - 1f) / 2f, -Keys.Board.SIDE_DISTANCE_FROM_CENTER); //Wektor położenia pierwszego pola. Jest to pole znajdujące się po prawej stronie boku, który jest poniżej środka planszy
        Vector2 vd = new Vector2(-Keys.Board.FIELD_WIDTH * (fieldIndex % (Keys.Board.FIELDS_PER_SIDE - 1)), 0f); //Wektor przesunięcia na danym boku. Bodany do wektora początkowego pozwala uzyskać wszystkie pola na danym boku

        Vector2 shiftedVector = v0 + vd; //Położenie jakie miało by pole na dolnym boku
        Vector2 fieldPosition = R * shiftedVector; //Mnożenie wektora shiftedVector przez macierz Rc w celu uzyskania odpowiednio obróconego wektora położenia pola

        return fieldPosition;
    }

    /// <summary>
    /// Mapuje numer pola, na jego obrót
    /// </summary>
    /// <param name="fieldIndex">Numer pola</param>
    /// <returns>Obrót pola</returns>
    public float GetBuildingRotation(int fieldIndex)
    {
        float theta = (2f * Mathf.PI / Keys.Board.BOARD_SIDES) * (int)(fieldIndex / (Keys.Board.FIELDS_PER_SIDE - 1)); //Kąt odpowiadający za obrót wektora. Zależy on od ilości boków figury i od indeksu danego pola. Odpowiedni kąt jest mnożony przez floor(fieldIndex / (fieldsPerSide - 1)) by obracać wektorem tylko wtedy, gdy indeks znajduje się na kolejnym boku

        if (fieldIndex % (Keys.Board.FIELDS_PER_SIDE - 1) == 0) return -theta + Keys.Board.INTERIOR_BOARD_ANGEL - Mathf.PI / 2f;
        else return -theta;
    }

    /// <summary>
    /// Zwraca PlaceVisualiser sterujący podanym polem
    /// </summary>
    /// <param name="placeIndex">Pole, którego PlaceVisualiser chcemy znaleźć</param>
    public PlaceVisualiser GetVisualiser(int placeIndex)
    {
        return places.Find(place => place.placeIndex == placeIndex);
    }

    /// <summary>
    /// Przełącza widoczność planszy
    /// </summary>
    /// <param name="visible">Określa, czy plansza ma być widoczna</param>
    private void ToggleBoardVisibility(bool visible)
    {
        foreach (PlaceVisualiser visualiser in places)
            visualiser.ToggleVisibility(visible);
    }

    #endregion Obsługa AR

    #region Obsługa eventów

    /// <summary>
    /// Funkcja wykrywająca kliknięcia na ekranie i wywołująca na odpowienich obiektach event kliknięcia
    /// </summary>
    private void OnScreenClick()
    {
        if (GameplayController.instance.session.gameState == GameState.running)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                //Wykrywa tylko kliknięcia, które się rozpoczęły
                if (Input.GetTouch(i).phase == TouchPhase.Began && !EventSystem.current.IsPointerOverGameObject(Input.GetTouch(i).fingerId))
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(i).position);
                    if (Physics.Raycast(ray, out RaycastHit hit))
                    {
                        Visualiser visualiser = hit.collider.gameObject.GetComponent<Visualiser>();
                        if (visualiser != null) 
                            visualiser.OnClick();
                    }
                    else //Jeśli dotkniemy poza budynkami środkowy budynek jest usuwany
                        centerBuilding.GetComponent<CenterVisualiser>().ToggleVisibility(false);
                }
            }
        }
    }

    #endregion Obsługa eventów
}
