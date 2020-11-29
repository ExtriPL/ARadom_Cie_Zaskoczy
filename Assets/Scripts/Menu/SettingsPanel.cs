using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsPanel : MonoBehaviourPunCallbacks, IInitiable<MainMenuController>
{
    //Zmienne pomocnicze
    private MainMenuController mainMenuController;
    private LanguageController langController;

    //Obiekty w Unity
    public Slider musicVolumeSlider; //Slider do muzyki
    public Slider soundEffectsVolumeSlider; //Slider do efektów dźwiękowych
    public TMP_Dropdown languageDropDown; //Dropdown do języka
    public GameObject restartWarning; // Ostrzezenie o koniecznosci restartu gry
    public GameObject saveRevert; //Guziki pojawiające się, gdy zmienione są ustawienia

    //Flagi dotyczące ustawień
    private bool langChanged = false; //Czy język został zmieniony
    private bool restartGame = false;

    #region Inicjalizacja
    public void Init()
    {
        //Ustawianie zmiennych pomocniczych
        langController = SettingsController.instance.languageController;

        //Przygotowywanie sliderów: muzyka, efekty dźwiękowe
        SetupSliders();

        //Przygotowywanie Dropdownu z językami
        SetupLanguageDropDown();

        mainMenuController.loadingScreen.EndLoading();
    }

    private void SetupSliders() 
    {
        //Ustawianie opisu Slidera muzyki, oraz wczytanie jego wartości z ustawień
        musicVolumeSlider.value = SettingsController.instance.settings.musicVolume;
        OnMusicVolumeChanged(musicVolumeSlider.value);

        //Ustawianie opisu Slidera efektów dźwiękowych, oraz wczytanie jego wartości z ustawień
        soundEffectsVolumeSlider.value = SettingsController.instance.settings.soundEffectsVolume;
        OnSoundEffectsVolumeChanged(soundEffectsVolumeSlider.value);
    }
    
    private void SetupLanguageDropDown() 
    {
        languageDropDown.options.Clear(); //czyszczenie opcji
        for (int i = 0; i < langController.languages.Count; i++)
        {
            //Dodawanie kolejnych języków z listy języków w LanguageControllerze do opcji
            languageDropDown.options.Add(new TMP_Dropdown.OptionData()
            { text = langController.GetWord(langController.languages[i].language.ToString()) });

            //Ustawianie wartości początkowej dropdownu na język zapisany w ustawieniach
            if (SettingsController.instance.settings.language == langController.languages[i].language)
            {
                languageDropDown.value = i;
            };
        }
        //Ustawianie początkowej etykiety dropdownu na nazwę ustawionego języka
        languageDropDown.captionText.text = SettingsController.instance.languageController.GetWord(SettingsController.instance.settings.language.ToString());
    }
    public void PreInit(MainMenuController mainMenuController) 
    {
        this.mainMenuController = mainMenuController;
    }

    public void DeInit() {}

    #endregion Inicjalizacja

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) Exit();
    }

    #region Przyciski
    public void Save()
    {
        //Zapisujemy wartości kontrolek do ustawień
        SettingsController.instance.settings.musicVolume = musicVolumeSlider.value; 
        SettingsController.instance.settings.soundEffectsVolume = soundEffectsVolumeSlider.value;
        SettingsController.instance.settings.language = SettingsController.instance.languageController.languages[languageDropDown.value].language;

        //Chowamy guziki zapisu i przywrócenia
        CheckIfSettingsChanged();
        if (langChanged) restartGame = true;
        saveRevert.SetActive(false);

        //Zapisujemy ustawienia do pliku
        SettingsController.instance.SaveSettingsToFile(); 
    }

    public void Exit()
    {
        if (restartGame) //Jeżeli został zmieniony język, przeładowujemy całą grę
        {
            PhotonNetwork.Disconnect();
        }
        else //Jeżeli nie, przechodzimy do MenuPanelu
        {
            mainMenuController.OpenPanel(Panel.MenuPanel);
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        SceneManager.LoadScene(Keys.SceneNames.MAIN_MENU, LoadSceneMode.Single);
    }

    public void SetDefaults() 
    {
        //Ustawianie opisu Slidera muzyki, oraz wczytanie jego wartości z wartości domyślnych
        musicVolumeSlider.value = Keys.Files.DefaultValues.MUSIC_VOLUME;
        OnMusicVolumeChanged(musicVolumeSlider.value);

        //Ustawianie opisu Slidera efektów dźwiękowych, oraz wczytanie jego wartości z wartości domyślnych
        soundEffectsVolumeSlider.value = Keys.Files.DefaultValues.SOUND_EFFECTS_VOLUME;
        OnSoundEffectsVolumeChanged(soundEffectsVolumeSlider.value);

        //Ustawianie domyślnego języka
        languageDropDown.value = 0;
        languageDropDown.captionText.text = SettingsController.instance.languageController.GetWord(Keys.Files.DefaultValues.LANGUAGE.ToString());

        CheckIfSettingsChanged();
    }
    
    public void OnChangeLanguage() 
    {
        CheckIfSettingsChanged();
    }
    
    public void OnMusicVolumeChanged(float value) 
    {
        musicVolumeSlider.GetComponentInChildren<TextMeshProUGUI>().text = SettingsController.instance.languageController.GetWord("MUSIC") +" : " + value.ToString();
        SettingsController.instance.soundController.SetMusicVolume(value);
        CheckIfSettingsChanged();
    }
    
    public void OnSoundEffectsVolumeChanged(float value)
    {
        soundEffectsVolumeSlider.GetComponentInChildren<TextMeshProUGUI>().text = SettingsController.instance.languageController.GetWord("SOUND_EFFECTS") + " : " + value.ToString();
        SettingsController.instance.soundController.SetSoundEffectsVolume(value);
        CheckIfSettingsChanged();
    }
    
    private void CheckIfSettingsChanged()
    {
        bool music = musicVolumeSlider.value != SettingsController.instance.settings.musicVolume;
        bool soundEffects = soundEffectsVolumeSlider.value != SettingsController.instance.settings.soundEffectsVolume;
        if (SettingsController.instance.languageController.GetKey(languageDropDown.captionText.text).ToLower() == SettingsController.instance.settings.language.ToString().ToLower())
        {
            restartWarning.SetActive(false);
            langChanged = false;
        }
        else
        {
            langChanged = true;
            restartWarning.SetActive(true);
        }

        if (music || soundEffects || langChanged)
        {
            saveRevert.SetActive(true);
        }
        else if (!(music && soundEffects && langChanged))
        {
            saveRevert.SetActive(false);
        }
    }

    
    #endregion Przyciski
}
