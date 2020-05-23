using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[System.Serializable]
public class GameMenu : MonoBehaviour
{
    private GameplayController gameplayController;

    [SerializeField] private GameObject OpenMenuButton;
    [SerializeField] private GameObject MenuPanel;
    [SerializeField] private GameObject AdminPanel;
    [SerializeField] private GameObject AccountPanel;
    [SerializeField] private GameObject SettingsPanel;
    [SerializeField] private Button ResumeButton;
    [SerializeField] private Button AdminPanelButton;
    [SerializeField] private Button AccountPanelButton;
    [SerializeField] private Button SettingsPanelButton;

    /// <summary>
    /// Czy menu jest aktualnie otwarte.
    /// </summary>
    public bool MenuPanelOpen { get; private set; }

    #region Inicjalizacja

    public void Init(GameplayController gameplayController)
    {
        this.gameplayController = gameplayController;

        MenuPanel.SetActive(false);
        OpenMenuButton.SetActive(true);
        MenuPanelOpen = false;

        Screen.sleepTimeout = SleepTimeout.NeverSleep; //Gdy menu jest wyłączone, ekran nigdy się nie wygasza 
        bool isLocal = true;

        try
        {
            isLocal = gameplayController.session.roomOwner.IsLocal;
        }
        catch
        {
            Debug.LogWarning("Gra niepodłączona do sieci");
        }

        if (!isLocal)
        {
            ResumeButton.interactable = false;
            AdminPanelButton.interactable = false;
        }
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

    private void OnGameStateChanged(GameState previousState, GameState newState)
    {
        //Komunikat o wstrzymaniu rozgrywki
        if (newState == GameState.paused)
        {
            MessageSystem.instance.AddMessage(Keys.Messages.GAME_HAS_BEEN_PAUSED, MessageType.MediumMessage, "pause", -1);
            ResumeButton.GetComponentInChildren<TextMeshProUGUI>().text = SettingsController.instance.languageController.GetWord("UNPAUSE");
        }
        //Usuwanie komunikatu o wstrzymaniu rozgrywki po jej przywróceniu
        else if (previousState == GameState.paused && newState == GameState.running)
        {
            MessageSystem.instance.RemoveMessage("pause");
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
        if (active && !MenuPanelOpen)
        {
            MenuPanel.SetActive(true);
            OpenMenuButton.SetActive(false);
            Screen.sleepTimeout = SleepTimeout.SystemSetting; //Gdy menu jest włączone, ekran wygasza się według ustawień telefonu
            MenuPanelOpen = true;
        }
        else if (!active && MenuPanelOpen)
        {
            MenuPanel.SetActive(false);
            OpenMenuButton.SetActive(true);
            Screen.sleepTimeout = SleepTimeout.NeverSleep; //Gdy menu jest wyłączone, ekran nigdy się nie wygasza
            MenuPanelOpen = false;
        }
    }

    /// <summary>
    /// Przełącza stan rozgrywki(gra/wstrzymanie). 
    /// Jeżeli twórca pokoju wciśnie przycisk, stan jest przełączany natychmiastowo, Natomiast jeżeli inny gracz wciśnie przycisk rozpoczyna się głosowanie nad zmianą stanu a właściciel otrzymuje powiadomienie
    /// </summary>
    public void SwitchGameState()
    {
        //Przełączanie stanu gry przez właściciela pokoju
        if (gameplayController.session.roomOwner.IsLocal)
        {
            if (gameplayController.session.gameState == GameState.running) gameplayController.session.gameState = GameState.paused;
            else if (gameplayController.session.gameState == GameState.paused) gameplayController.session.gameState = GameState.running;
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
    /// Przełącza okna tak, by wyświetlić moduł bankowości
    /// </summary>
    public void OpenBankAccount()
    {
        MenuPanel.SetActive(false);
        AccountPanel.SetActive(true);
    }

    public void CloseBankAccount()
    {
        MenuPanel.SetActive(true);
        AccountPanel.SetActive(false);
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
        if (gameplayController.session.roomOwner.IsLocal)
        {
            //EventManager.instance.SendOnRoomOvnerQuit();
            gameplayController.session.gameState = GameState.roomDestroyed;
            /*
             * Zakończenie rozgrywki
             */
        }
        else EventManager.instance.SendOnPlayerQuit(gameplayController.session.localPlayer.GetName());

        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LoadLevel(Keys.SceneNames.MAIN_MENU);
    }

    #endregion Obsłuiga przycisków menu
}
