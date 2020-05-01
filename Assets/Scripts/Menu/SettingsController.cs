using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour
{
    public static SettingsController instance;
    [SerializeField] private ApplicationSettings settings;

    public AudioMixer musicMixer;
    public Slider musicSlider;
    public Slider soundEffectsSlider;

    private void Start()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);

        Init();
    }

    private void Init()
    {
        FileManager.LoadApplicationSettings(ref settings);
        FindReferences();
        LoadSavedApplicationSettings();
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
        if (musicSlider) musicSlider.SetValueWithoutNotify(settings.musicVolume);
        SetSoundEffectsVolume(settings.soundEffectsVolume);
        if (soundEffectsSlider) soundEffectsSlider.SetValueWithoutNotify(settings.soundEffectsVolume);
    }

    public void SetMusicVolume(float volume)
    {
        musicMixer.SetFloat("MusicVolume", volume);
        settings.musicVolume = volume;
    }

    public void SetSoundEffectsVolume(float volume)
    {
        musicMixer.SetFloat("SoundEffectsVolume", volume);
        settings.soundEffectsVolume = volume;
    }

    public void SaveSettingsToFile()
    {
        FileManager.SaveApplicationSettings(settings);
    }
}
