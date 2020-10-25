using GoogleARCore;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[System.Serializable]
public class PlaceVisualiser : Visualiser
{
    /// <summary>
    /// Ustawienia pola
    /// </summary>
    public Field field;
    /// <summary>
    /// Numer pola na planszy
    /// </summary>
    public int placeIndex { get; private set; } 

    /// <summary>
    /// Lista graczy znajdujących się aktualnie na polu
    /// </summary>
    private List<string> playersOnField = new List<string>();
    /// <summary>
    /// Lista materiałów obiektów tworzących światła na polu
    /// </summary>
    private List<GameObject> backlightsList = new List<GameObject>();
    /// <summary>
    /// Odwołanie do gracza, który stoi na polu i jednocześnie jest jego kolejka
    /// </summary>
    private Player activePlayer = null;
    /// <summary>
    /// Współczynnik podświetlenia
    /// </summary>
    private float tParameter = 0f;

    private GameObject particleEffect;
    private GameObject pointingArrow;
    private Animator arrowAnimator;

    private float startShiningTime;

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
        visible = true;

        ARController = GameplayController.instance.arController;
        InitModels();

        CreateBacklight();

        //Inicjalizacja podświetlenia startowego
        for (int i = 0; i < GameplayController.instance.session.playerCount; i++)
        {
            Player player = GameplayController.instance.session.FindPlayer(i);
            if (player.PlaceId == placeIndex) playersOnField.Add(player.GetName());
        }

        Player activePlayer = GameplayController.instance.session.FindPlayer(GameplayController.instance.board.dice.currentPlayer);
        if (activePlayer.PlaceId == placeIndex) ActivateField(activePlayer);
        else DeactivateField();

        particleEffect = Instantiate(ARController.highlightEffect, transform);
        particleEffect.GetComponent<Transform>().localPosition = Vector3.zero;

        pointingArrow = Instantiate(ARController.pointingArrow, transform);

        arrowAnimator = pointingArrow.GetComponentInChildren<Animator>();

        //pointingArrow.GetComponent<Transform>().localPosition = new Vector3(0, 0, -0.06f);
        //if (placeIndex == 10) StartCoroutine(Shine(GameplayController.instance.session.localPlayer, true));
    }

    private void Update()
    {
        Shine();
    }

    /// <summary>
    /// Inicjuje modele stojące na polu
    /// </summary>
    protected override void InitModels()
    {
        GameObject startModel = Instantiate(field.GetStartModel(), gameObject.GetComponent<Transform>());
        models.Add(startModel);
        if (field is NormalBuilding)
        {
            startModel.SetActive(false);
            startModel.GetComponent<Transform>().Rotate(new Vector3(0f, 0f, -gameObject.GetComponent<Transform>().rotation.eulerAngles.z));
            NormalBuilding nb = field as NormalBuilding;
            for(int i = 1; i < nb.tiersCount; i++)
            {
                GameObject model = Instantiate(nb.tiers[i].model, gameObject.GetComponent<Transform>());
                model.SetActive(false);
                models.Add(model);
            }
        }

        //Przywracanie budynku po wczytaniu save
        ShowModel(GameplayController.instance.board.GetTier(placeIndex));
    }

    public override void SubscribeEvents()
    {
        EventManager.instance.onPlayerMoved += OnPlayerMove;
        EventManager.instance.onTurnChanged += OnTurnChange;
        EventManager.instance.onPlayerQuited += OnPlayerQuit;
        EventManager.instance.onAquiredBuilding += OnAquiredBuilding;
        EventManager.instance.onUpgradedBuilding += OnUpgradeBuilding;
        EventManager.instance.onPlayerLostGame += OnPlayerLostGame;
        EventManager.instance.onPlayerTeleported += OnPlayerTeleported;
    }

    /// <summary>
    /// Odsubskrybowanie eventów
    /// </summary>
    public override void UnsubscribeEvents()
    {
        EventManager.instance.onPlayerMoved -= OnPlayerMove;
        EventManager.instance.onTurnChanged -= OnTurnChange;
        EventManager.instance.onPlayerQuited -= OnPlayerQuit;
        EventManager.instance.onAquiredBuilding -= OnAquiredBuilding;
        EventManager.instance.onUpgradedBuilding -= OnUpgradeBuilding;
        EventManager.instance.onPlayerLostGame -= OnPlayerLostGame;
        EventManager.instance.onPlayerTeleported -= OnPlayerTeleported;
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
        if (GameplayController.instance.board.dice.amountOfRolls == 0) return;
        OnPlayerTeleported(playerName, fromPlaceIndex, toPlaceIndex);

        if(GameplayController.instance.board.GetBetweenPlaces(fromPlaceIndex, toPlaceIndex).Contains(placeIndex))
        {
            Player player = GameplayController.instance.session.FindPlayer(playerName);

            //Jeżeli obiekt jest nieaktywny, StartCoroutine nie zadziała, co wywoła błędy
            if(gameObject.activeInHierarchy)
                StartCoroutine(ActivationInterval(player, GameplayController.instance.board.GetPlacesDistance(fromPlaceIndex, placeIndex), toPlaceIndex));
            else
            {
                if (placeIndex != toPlaceIndex)
                    DeactivateField();
                else
                {
                    ActivateField(player);
                    field.OnEnter(player, this);
                }
            }
        }
    }

    /// <summary>
    /// Zdarzenia wywoływane gdy gracz zostanie przeniesiony
    /// </summary>
    /// <param name="playerName">Nazwa gracza</param>
    /// <param name="fromPlaceIndex">Numer pola, z którego ruszał się gracz</param>
    /// <param name="toPlaceIndex">Numer pola, na które przeszedł gracz</param>
    private void OnPlayerTeleported(string playerName, int fromPlaceIndex, int toPlaceIndex)
    {
        if (fromPlaceIndex == placeIndex)
        {
            field.OnLeave(GameplayController.instance.session.FindPlayer(playerName), this);
            playersOnField.Remove(playerName);
            DeactivateField();
        }
        else if (toPlaceIndex == placeIndex)
        {
            playersOnField.Add(playerName);
            ActivateField(GameplayController.instance.session.FindPlayer(playerName));
        }
    }

    /// <summary>
    /// Obsługa eventu zmiany tury
    /// </summary>
    /// <param name="previousPlayerName">Gracz, którego tura się skończyła</param>
    /// <param name="currentPlayerName">Gracz, którego tura się zaczeła</param>
    private void OnTurnChange(string previousPlayerName, string currentPlayerName)
    {
        Player currentPlayer = GameplayController.instance.session.FindPlayer(currentPlayerName);
        Player previousPlayer = GameplayController.instance.session.FindPlayer(previousPlayerName);

        //Sterowanie podświetleniem aktywnego gracza
        if (playersOnField.Contains(previousPlayerName))
        {
            field.OnEnd(previousPlayer, this);
            DeactivateField();
        }
        if (playersOnField.Contains(currentPlayerName))
        {
            field.OnAwake(currentPlayer, this);
            ActivateField(currentPlayer);
        }
    }

    /// <summary>
    /// Zdarzenie wywoływane po wciśnięciu pola na ekranie
    /// </summary>
    public override void OnClick()
    {
        base.OnClick();

        ARController.centerBuilding.GetComponent<CenterVisualiser>().ShowField(field, placeIndex);
    }

    /// <summary>
    /// Obsługa eventu wyjścia gracza z pokoju
    /// </summary>
    /// <param name="playerName">Nick gracza, który wychodzi z gry</param>
    private void OnPlayerQuit(string playerName)
    {
        //Zrestartowanie modelu do stanu początkowego
        if (showedModel != 0 && GameplayController.instance.board.GetTier(placeIndex) == 0) ShowModel(0);

        //Usuwanie podświetlenia gracza, który właśnie wyszedł
        RemovePlayerFromField(playerName);
    }

    /// <summary>
    /// Obsługa eventu kupna pola
    /// </summary>
    /// <param name="playerName">Nazwa gracza, który kupił pole</param>
    /// <param name="placeId">Id pola, które zostało kupione</param>
    private void OnAquiredBuilding(string playerName, int placeId)
    {
        //Jeżeli pole obsługiwane przez ten PlaceVisualiser zostało kupione
        if(placeIndex == placeId)
        {
            if (field is BuildingField)
            {
                BuildingField buildingField = field as BuildingField;

                buildingField.OnBuy(GameplayController.instance.session.FindPlayer(playerName), this);

                Explosion();
            }
        }
    }

    /// <summary>
    /// Obsługa eventu ulepszenia pola
    /// </summary>
    /// <param name="playerName">Nazwa gracza, który ulepszył pole</param>
    /// <param name="placeId"></param>
    private void OnUpgradeBuilding(string playerName, int placeId)
    {
        if(placeIndex == placeId)
        {
            if (field is NormalBuilding)
            {
                NormalBuilding upgradeBuilding = field as NormalBuilding;

                upgradeBuilding.OnUpgrade(GameplayController.instance.session.FindPlayer(playerName), this);

                Explosion();
            }
        } 
    }

    /// <summary>
    /// Obsługa eventu przegranej gracza
    /// </summary>
    /// <param name="playerName"></param>
    private void OnPlayerLostGame(string playerName)
    {
        //Zrestartowanie modelu do stanu początkowego
        if (showedModel != 0 && GameplayController.instance.board.GetTier(placeIndex) == 0) ShowModel(0);

        RemovePlayerFromField(playerName);
    }

    #endregion Obsługa eventów

    #region Podświetlenie pól

    /// <summary>
    /// Tworzy obekty, które będą służyły do podświetlania pola, gdy stanie na nim gracz
    /// </summary>
    private void CreateBacklight()
    {
        GameObject backlights = new GameObject("Backlights"); //Obiekt przechowujący światła
        backlights.GetComponent<Transform>().SetParent(gameObject.GetComponent<Transform>());

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
            light.GetComponent<Transform>().SetParent(backlights.GetComponent<Transform>());
            light.GetComponent<Transform>().localPosition = v;
            light.GetComponent<Transform>().localRotation = Quaternion.Euler(new Vector3(0f, 0f, -rotationAngel / Mathf.PI * 180));
            light.GetComponent<Transform>().localScale = new Vector3(Keys.Board.FIELD_SIDE_LENGHT / Keys.Board.SCALLING_FACTOR, Keys.Board.Backlight.THICKNESS, Keys.Board.Backlight.THICKNESS);

            //Ustawianie jego startowego koloru
            Material startMaterial = new Material(Resources.Load("Materials/Fade") as Material); //Ładowanie template-a odpowiedniego materiału z folderu Resources i tworzenie na jego podstawie odpowiedniego materiału
            startMaterial.color = new Color(0f, 0f, 1f, 0f);

            light.GetComponent<Renderer>().material = startMaterial;

            backlightsList.Add(light);
        }

        backlights.GetComponent<Transform>().localPosition = new Vector3();
        backlights.GetComponent<Transform>().localRotation = Quaternion.Euler(new Vector3());
        backlights.GetComponent<Transform>().localScale = new Vector3(1f, 1f, 1f);
    }

    /// <summary>
    /// Tworzy on nowa strukturę świateł. Podmienia kolory obiektów pełniących rolę świateł na kolory odpowiadające graczą na liście graczy
    /// </summary>
    private void RecreateLights()
    {
        //Ustawienie koloru dla graczy stojących na polu
        for(int i = 0; i < playersOnField.Count; i++)
        {
            Color playerColor = GameplayController.instance.session.FindPlayer(playersOnField[i]).MainColor;
            backlightsList[i].GetComponent<Renderer>().material.color = new Color(playerColor.r, playerColor.g, playerColor.b);
        }

        //Dla wszystkich części pola, na których nie przypada żaden gracz, ustawiany jest domyślny kolor
        for(int i = playersOnField.Count; i < backlightsList.Count; i++)
            backlightsList[i].GetComponent<Renderer>().material.color = Keys.Board.Backlight.INACTIVE_COLOR;
    }

    /// <summary>
    /// Rozświetla pole na kolor aktywnego gracza
    /// </summary>
    /// <param name="activeColor"></param>
    private void ActivateField(Player activePlayer)
    {
        foreach(GameObject back in backlightsList)
        {
            back.GetComponent<Renderer>().material.color = activePlayer.MainColor;
        }

        this.activePlayer = activePlayer;
        startShiningTime = Time.time - (Keys.Board.Backlight.SHINING_PERIOD / Mathf.PI) * Mathf.Acos(Mathf.Sqrt(tParameter));
    }

    private void DeactivateField()
    {
        //if (activePlayer != null) StartCoroutine(Shine(activePlayer, true, tParameter));
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
            tParameter = Mathf.Pow(Mathf.Cos((Time.time - startShiningTime) * Mathf.PI / Keys.Board.Backlight.SHINING_PERIOD), 2f);
            
            foreach (GameObject back in backlightsList) 
                back.GetComponent<Renderer>().material.color = Color.Lerp(activePlayer.MainColor, activePlayer.BlinkColor, tParameter);
        }
    }

    /// <summary>
    /// Miga światłami gracza przechodzącego przez pola
    /// </summary>
    /// <param name="activePlayer">Gracz, z którego ma pobrać kolor</param>
    /// <param name="turn">Jeżeli true - wyłacza podświetlenie, jeżeli false - włącza podświetlenie</param>
    /// <param name="shiftT">Współczynnik przesunięcia sekwencji.</param>
    private IEnumerator Shine(Player activePlayer, bool turn, float shiftT = 0f)
    {
        float startTime = Time.time;
        float halfPeriod = Keys.Board.Backlight.SHINING_PERIOD / 2f;
        float shift = turn ? halfPeriod : 0f;

        //Wykonujemy pętle przez połowę okresu świecenia
        while (Time.time - startTime < halfPeriod)
        {
            float time = Time.time - startTime;

            float t = Mathf.Pow(Mathf.Cos((time + shift) * Mathf.PI / Keys.Board.Backlight.SHINING_PERIOD), 2f) + shiftT;
            foreach (GameObject back in backlightsList) back.GetComponent<Renderer>().material.color = Color.Lerp(activePlayer.MainColor, activePlayer.BlinkColor, t);

            yield return null;
        }

        tParameter = turn ? 1f : 0f;
    }

    /// <summary>
    /// Aktywuje sekwencje podświetlenia pól, przez które przechodzi gracz (sekwencja przypomina ciągnącego się wężyka)
    /// </summary>
    /// <param name="activePlayer">Gracz, który przechodzi nad polami</param>
    /// <param name="distanceFromStart">Odległość od pola, z którego gracz się rusza</param>
    /// <param name="targetPlace">Pole, na które gracz się rusza</param>
    /// <returns></returns>
    private IEnumerator ActivationInterval(Player activePlayer, int distanceFromStart, int targetPlace)
    {
        yield return new WaitForSeconds(Keys.Board.Backlight.SHINING_PERIOD / 2f);
        yield return new WaitForSeconds(Keys.Board.Backlight.ANIMATION_STAY_TIME * (distanceFromStart - 1)); //Oczekiwanie na minięcie animacji poprzednich pól
        
        StartCoroutine(Shine(activePlayer, false));
        field.OnPassby(GameplayController.instance.session.FindPlayer(activePlayer.GetName()), this);

        yield return new WaitForSeconds(Keys.Board.Backlight.SHINING_PERIOD / 2f);
        yield return new WaitForSeconds(Keys.Board.Backlight.ANIMATION_STAY_TIME * (distanceFromStart - 1));


        if (placeIndex != targetPlace)
        {
            StartCoroutine(Shine(activePlayer, true, tParameter));
            yield return new WaitForSeconds(Keys.Board.Backlight.SHINING_PERIOD / 2f);
            DeactivateField();
        }
        else
        {
            ActivateField(activePlayer);
            field.OnEnter(activePlayer, this);
        }
    }

    /// <summary>
    /// Ukrywa podświetlenie pola
    /// </summary>
    private void HideBacklights()
    {
        foreach (GameObject back in backlightsList)
            back.SetActive(false);
    }

    /// <summary>
    /// Pokazuje podświetlenie pola
    /// </summary>
    private void ShowBacklights()
    {
        foreach (GameObject back in backlightsList)
            back.SetActive(true);
    }

    /// <summary>
    /// Usuwa podświetlenie gracza z pola
    /// </summary>
    /// <param name="playerName">Gracz, któego chcemy usunąć</param>
    private void RemovePlayerFromField(string playerName)
    {
        playersOnField.Remove(playerName);
        Player activePlayer = GameplayController.instance.session.FindPlayer(GameplayController.instance.board.dice.currentPlayer);
        if (activePlayer.PlaceId == placeIndex) ActivateField(activePlayer);
        else DeactivateField();
    }

    #endregion Podświetlenie pól

    /// <summary>
    /// Przełącza widoczność pola
    /// </summary>
    /// <param name="visible">Zmienna określająca, czy pole ma być widoczne</param>
    public override void ToggleVisibility(bool visible)
    {
        base.ToggleVisibility(visible);

        if(visible)
            ShowBacklights();
        else
            HideBacklights();
    }

    public void Highlight()
    {
        if(!arrowAnimator.GetCurrentAnimatorStateInfo(0).IsName("PointingArrow")) arrowAnimator.SetTrigger("Start");
    }

    public void Explosion() 
    {
        particleEffect.GetComponent<ParticleSystem>().Play();
    }
}