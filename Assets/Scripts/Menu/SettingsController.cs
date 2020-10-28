using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using Unity.RemoteConfig;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour
{
    public bool Loaded { get; private set; }
    public static SettingsController instance;
    [SerializeField] public ApplicationSettings settings;
    public LanguageController languageController = new LanguageController();
    public SoundController soundController;

    private void Start()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
        soundController = GetComponent<SoundController>();
    }
    public void Init()
    {
        FileManager.LoadApplicationSettings(ref settings);
        FindReferences();
        LoadSavedApplicationSettings();
        Loaded = true;
    }

    /// <summary>
    /// Znajdowanie odpowiednich obiektów
    /// </summary>
    private void FindReferences()
    {
        /*
         Problemy z odnalezieniem odpowiedniej funkcji. Funkcja z resources niekoniecznie będzie działać dobrze
         */
    }

    /// <summary>
    /// Przywracanie ustawień zapisanych w pliku
    /// </summary>
    private void LoadSavedApplicationSettings()
    {
        soundController.SetMusicVolume(settings.musicVolume);
        soundController.SetSoundEffectsVolume(settings.soundEffectsVolume);
    }

    public void SaveSettingsToFile()
    {
        FileManager.SaveApplicationSettings(settings);
    }
}
