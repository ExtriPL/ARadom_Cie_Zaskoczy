using System.Collections;
using System.Collections.Generic;
using Unity.RemoteConfig;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour
{
    public static SettingsController instance;
    [SerializeField] public ApplicationSettings settings;
    public LanguageController languageController = new LanguageController();

    public AudioMixer musicMixer;
    public Slider musicSlider;
    public Slider soundEffectsSlider;
    public AudioSource buttonAudioSource;

    
    private void Start()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }
    public void Init()
    {
        FileManager.LoadApplicationSettings(ref settings);
        FindReferences();
        LoadSavedApplicationSettings();
        buttonAudioSource = gameObject.GetComponents<AudioSource>()[1];
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
        SetMusicVolume(settings.musicVolume);
        SetSoundEffectsVolume(settings.soundEffectsVolume);
    }

    public void SetMusicVolume(float volume)
    {
        GetComponents<AudioSource>()[0].volume = volume/ Keys.Menu.MAX_MUSIC_VOLUME;
    }

    public void SetSoundEffectsVolume(float volume)
    {
        GetComponents<AudioSource>()[1].volume = volume/Keys.Menu.MAX_MUSIC_VOLUME;
    }

    public void SaveSettingsToFile()
    {
        FileManager.SaveApplicationSettings(settings);
    }
}
