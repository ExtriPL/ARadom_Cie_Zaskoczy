using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[System.Serializable]
public class GameMenu : MonoBehaviour, IEventSubscribable
{
    [SerializeField] public GameObject OpenMenuButton;
    [SerializeField] public GameObject MenuPanel;
    [SerializeField] private GameObject AdminPanel;
    [SerializeField] private GameObject SettingsPanel;
    [SerializeField] private Button ResumeButton;
    [SerializeField] private Button AdminPanelButton;
    [SerializeField] private Button SettingsPanelButton;

    [SerializeField] private Button NextTurnButton;
    [SerializeField] private TextMeshProUGUI NextTurnButtonTimer;

    /// <summary>
    /// Czy menu jest aktualnie otwarte.
    /// </summary>
    public bool menuPanelOpen { get; private set; }

    #region Inicjalizacja

    public void InitMenu()
    {
        MenuPanel.SetActive(false);
        OpenMenuButton.SetActive(true);
        menuPanelOpen = false;

        Screen.sleepTimeout = SleepTimeout.NeverSleep; //Gdy menu jest wyłączone, ekran nigdy się nie wygasza 
    }

    public void SubscribeEvents()
    {
        EventManager.instance.onGameStateChanged += OnGameStateChanged;
    }

    public void UnsubscribeEvents()
    {
        EventManager.instance.onGameStateChanged -= OnGameStateChanged;
    }

    #endregion Inicjalizacja

    #region Eventy

    private void OnApplicationQuit()
    {
        QuitGame();
    }

    private void OnGameStateChanged(GameState previousState, GameState newState)
    {
        //Komunikat o wstrzymaniu rozgrywki
        if (newState == GameState.paused)
        {
            string gamePauesedMessage = SettingsController.instance.languageController.GetWord("GAME_PAUSED");
            /*gamePaused = new InfoPopup(gamePauesedMessage, Mathf.Infinity);
            PopupSystem.instance.AddPopup(gamePaused);*/
            Debug.Log("Potrzebny komunikat");

            ResumeButton.GetComponentInChildren<TextMeshProUGUI>().text = SettingsController.instance.languageController.GetWord("UNPAUSE");
        }
        //Usuwanie komunikatu o wstrzymaniu rozgrywki po jej przywróceniu
        else if (previousState == GameState.paused && newState == GameState.running)
        {
            /*PopupSystem.instance.ClosePopup(gamePaused);
            gamePaused = null;*/
            Debug.Log("Potrzebny komunikat");

            ResumeButton.GetComponentInChildren<TextMeshProUGUI>().text = SettingsController.instance.languageController.GetWord("PAUSE");
        }
    }

    #endregion

    #region Obsłuiga przycisków menu

    /// <summary>
    /// Służy do ustawienia stanu podręcznego menu
    /// </summary>
    /// <param name="active">Stan menu, true - pokaż, false - ukryj</param>
    public void MenuState(bool active)
    {
        //Menu włącza się tylko wtedy, gdy jest aktualnie wyłączone. Analogicznie wyłącza się tylko wtedy, gdy jest włączone
        if (active && !menuPanelOpen)
        {
            MenuPanel.SetActive(true);
            OpenMenuButton.SetActive(false);
            Screen.sleepTimeout = SleepTimeout.SystemSetting; //Gdy menu jest włączone, ekran wygasza się według ustawień telefonu
            menuPanelOpen = true;
        }
        else if (!active && menuPanelOpen)
        {
            MenuPanel.SetActive(false);
            OpenMenuButton.SetActive(true);
            Screen.sleepTimeout = SleepTimeout.NeverSleep; //Gdy menu jest wyłączone, ekran nigdy się nie wygasza
            menuPanelOpen = false;
        }
    }

    /// <summary>
    /// Przełącza stan rozgrywki(gra/wstrzymanie). 
    /// Jeżeli twórca pokoju wciśnie przycisk, stan jest przełączany natychmiastowo, Natomiast jeżeli inny gracz wciśnie przycisk rozpoczyna się głosowanie nad zmianą stanu a właściciel otrzymuje powiadomienie
    /// </summary>
    public void SwitchGameState()
    {
        //Przełączanie stanu gry przez właściciela pokoju
        if (GameplayController.instance.session.roomOwner.IsLocal)
        {
            if (GameplayController.instance.session.gameState == GameState.running) GameplayController.instance.session.gameState = GameState.paused;
            else if (GameplayController.instance.session.gameState == GameState.paused) GameplayController.instance.session.gameState = GameState.running;
        }
        else
        {
            /*
            * Głosowanie do implementacji
            */
        }
    }

    /// <summary>
    /// Otwiera panel administracyjny. Dostępne tylko dla twórcy pokoju
    /// </summary>
    public void OpenAdminPanel()
    {
        MenuPanel.SetActive(false);
        AdminPanel.SetActive(true);
    }

    /// <summary>
    /// Zamyka panel administracyjny. 
    /// </summary>
    public void CloseAdminPanel()
    {
        MenuPanel.SetActive(true);
        AdminPanel.SetActive(false);
    }

    /// <summary>
    /// Otwiera ustawienia aplikacji
    /// </summary>
    public void OpenSettings()
    {
        SettingsPanel.SetActive(true);
        MenuPanel.SetActive(false);
    }

    public void CloseSettings()
    {
        SettingsPanel.SetActive(false);
        MenuPanel.SetActive(true);
    }

    /// <summary>
    /// Wychodzi z rozgrywki i wraca do menu głównego. 
    /// Przed wyjściem daje możliwość zapisania lokalnie obecnego stanu rozgrywki. 
    /// Jeżeli funkcja zostanie wywołana przez twórcę pokoju, jest jednocześnie zamykany pokój
    /// </summary>
    public void QuitGame()
    {
        //Jeżeli właściciel pokoju opuszcza rozgrywkę, jest ona kończona
        if (GameplayController.instance.session.roomOwner.IsLocal)
        {
            GameplayController.instance.session.gameState = GameState.roomDestroyed;
            /*
             * Zakończenie rozgrywki
             */
        }
        else
        {
            GameplayController.instance.session.KickPlayer(GameplayController.instance.session.localPlayer);
        }    
    }

    /// <summary>
    /// Przełącza stan włączenie przycisku następnej tury
    /// </summary>
    /// <param name="active">Stan włączenia przycisku</param>
    public void SetActiveNextTurnButton(bool active)
    {
        NextTurnButton.gameObject.SetActive(active);
    }

    /// <summary>
    /// Ustawia możliwośc wciśnięcia przycisku zmiany tury
    /// </summary>
    /// <param name="interactable">Czy przycisk może zostać wciśnięty</param>
    public void SetInteractableNextTurnButton(bool interactable)
    {
        NextTurnButton.interactable = interactable;
    }

    /// <summary>
    /// Przełącza stan włączenia licznika przy przycisku zakończenia tury
    /// </summary>
    /// <param name="time">Czas, który został do automatycznego skończenia tury</param>
    public void SetActiveNextTurnButtonTimer(bool active)
    {
        NextTurnButtonTimer.gameObject.SetActive(active);
    }

    /// <summary>
    /// Ustawia wskaźnik timera
    /// </summary>
    /// <param name="time">Czas pozostały do końca rundy</param>
    public void SetNextTurnButtonTimer(int time)
    {
        NextTurnButtonTimer.text = time.ToString();
    }

    /// <summary>
    /// Funkcja używana do wywołania końca tury za pomocą przycisku
    /// </summary>
    public void EndTurn()
    {
        GameplayController.instance.flow.EndTurn();
    }

    #endregion Obsłuiga przycisków menu
}
