using GoogleARCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using Input = GoogleARCore.InstantPreviewInput;
#endif

public class ARController : MonoBehaviour
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
    private List<PlaceVisualiser> fields = new List<PlaceVisualiser>();
    /// <summary>
    /// Lista wczytanych obrazów na scenie
    /// </summary>
    private List<AugmentedImage> tempPlaceImages = new List<AugmentedImage>();

    private void OnDisable()
    {
        foreach (PlaceVisualiser visualiser in fields) visualiser.UnsubscribeEvents();
    }

    private void Awake()
    {
        Application.targetFrameRate = Keys.Gameplay.TARGET_FRAMERATE; //Ustawienie stałego framerate-u
    }

    private void Update()
    {
        FindTriggers();
        OnScreenClick();
    }

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
                    Anchor anchor = image.CreateAnchor(image.CenterPose); //Kotwica służąca do utrzymywania śledzenia przez ARCore, jest powiązana z obrazem w bazie danych
                    board.gameObject.SetActive(true);
                    board.GetComponent<Transform>().parent = anchor.GetComponent<Transform>();
                    board.GetComponent<Transform>().localPosition = new Vector3();
                }
                //Gdy obrazek zniknie z pola widzenia, plansza jest ukrywana
                else if (image.TrackingState == TrackingState.Stopped && image.Name.Equals(Keys.Board.AR_IMAGE_NAME))
                {
                    board.gameObject.SetActive(false);
                    board.GetComponent<Transform>().parent = gameObject.GetComponent<Transform>();
                }
            }
            else
            {
                board.gameObject.SetActive(false);
                board.GetComponent<Transform>().parent = gameObject.GetComponent<Transform>();
            }
        }
    }

    /// <summary>
    /// Inicjalizuje w przestrzeni AR plansze i pola się na niej znajdujące
    /// </summary>
    public void InitBoard()
    {
        board = new GameObject("Board"); //Tworzenie instancji planszy
        board.GetComponent<Transform>().parent = gameObject.GetComponent<Transform>();
        board.gameObject.SetActive(false);

        //Tworzenie obiektu przechowującego środkowy budynek
        centerBuilding = new GameObject("centerBuilding");
        centerBuilding.GetComponent<Transform>().parent = board.GetComponent<Transform>();
        float BuildingScaleFactor = Keys.Board.SCALLING_FACTOR * Keys.Board.CENTER_BUILDING_SCALE_MULTIPLIER;
        centerBuilding.GetComponent<Transform>().localScale *= BuildingScaleFactor;
        centerBuilding.AddComponent<BoxCollider>();
        centerBuilding.GetComponent<BoxCollider>().size = (new Vector3(Keys.Board.FIELD_WIDTH * BuildingScaleFactor, Keys.Board.FIELD_WIDTH * BuildingScaleFactor, Keys.Board.FIELD_HEIGHT * BuildingScaleFactor)) / (Keys.Board.SCALLING_FACTOR * BuildingScaleFactor);
        centerBuilding.GetComponent<BoxCollider>().center = new Vector3(0f, 0f, -Keys.Board.FIELD_WIDTH * BuildingScaleFactor / (Keys.Board.SCALLING_FACTOR * BuildingScaleFactor) / 2f);
        centerBuilding.GetComponent<BoxCollider>().isTrigger = true;

        //Uzupełnianie planszy budynkami
        for (int i = 0; i < Keys.Board.FIELD_COUNT; i++)
        {
            GameObject field = new GameObject("field" + i); //Tworzenie obiektu pola
            field.GetComponent<Transform>().parent = board.GetComponent<Transform>(); //Przypisywanie obiektu do planszy

            field.AddComponent<PlaceVisualiser>();
            field.GetComponent<PlaceVisualiser>().Init(GameplayController.instance.board.GetField(i), i);
            field.GetComponent<PlaceVisualiser>().SubscribeEvents();
            fields.Add(field.GetComponent<PlaceVisualiser>()); //Dodawanie pola do listy pól na wyświetlonych na planszy

            //Dodawanie collidera do wykrywania raycastów
            field.AddComponent<BoxCollider>();
            field.GetComponent<BoxCollider>().size = (new Vector3(Keys.Board.FIELD_WIDTH, Keys.Board.FIELD_HEIGHT, Keys.Board.FIELD_WIDTH)) / Keys.Board.SCALLING_FACTOR;
            field.GetComponent<BoxCollider>().center = new Vector3(0f, 0f, -Keys.Board.FIELD_WIDTH / Keys.Board.SCALLING_FACTOR / 2f);
            field.GetComponent<BoxCollider>().isTrigger = true;

            //Skalowanie i obracanie, by dostosować planszę
            field.GetComponent<Transform>().localPosition = GetFieldPosition(i);
            field.GetComponent<Transform>().rotation = Quaternion.Euler(0, 0f, GetBuildingRotation(i) / Mathf.PI * 180);
            field.GetComponent<Transform>().localScale *= Keys.Board.SCALLING_FACTOR;
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
    /// Funkcja wykrywająca kliknięcia na ekranie i wywołująca na odpowienich obiektach event kliknięcia
    /// </summary>
    private void OnScreenClick()
    {
        if (!GameplayController.instance.menu.MenuPanelOpen)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                //Wykrywa tylko kliknięcia, które się rozpoczęły
                if (Input.GetTouch(i).phase == TouchPhase.Began)
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(i).position);
                    if (Physics.Raycast(ray, out RaycastHit hit))
                    {
                        PlaceVisualiser visualiser = hit.collider.gameObject.GetComponent<PlaceVisualiser>();
                        if (visualiser != null) visualiser.OnClick();

                        //Jeśli trafimy w budynek centralny to wywołujemy funkcję
                        else if(hit.collider.gameObject.name == "centerBuilding")
                        {
                            //do zaimplementowania
                            string name = "";
                            foreach (Transform child in hit.collider.gameObject.GetComponentsInChildren<Transform>()) 
                            {
                                if (child.name != "centerBuilding") 
                                {
                                    name = child.name;
                                    break;
                                }
                            }
                            Field field = GameplayController.instance.board.GetField(name);

                            string message="";
                            foreach (string info in field.GetFieldInfo())
                            {
                                message += info + "||";
                            }
                            MessageSystem.instance.AddMessage(message, MessageType.LongMessage);
                            MessageSystem.instance.AddMessage("OOF", MessageType.LongMessage);

                        }
                    }
                    else
                    {
                        //Jeśli dotkniemy poza budynkami środkowy budynek jest usuwany
                        foreach (Transform transform in centerBuilding.GetComponentsInChildren<Transform>())
                        {
                            if (transform.gameObject != centerBuilding)
                            {
                                Destroy(transform.gameObject);
                            }
                        }
                    }
                }
            }
        }
    }
}
