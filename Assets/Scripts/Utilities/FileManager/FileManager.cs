using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Manager do zarządzania serializacją plików
/// </summary>
public static class FileManager
{
    static FileManager()
    {
        CreateBasicFiles();
    }

    /// <summary>
    /// Funkcja tworząca posstawowe pliki i foldery
    /// </summary>
    private static void CreateBasicFiles()
    {
        //Główny folder
        if (Keys.Files.MAIN_DIRECTORY != "./" && !Directory.Exists(Keys.Files.MAIN_DIRECTORY)) Directory.CreateDirectory(Keys.Files.MAIN_DIRECTORY);

        //Folder zapisów gry
        if (Keys.Files.SAVES_DIRECTORY != "./" && !Directory.Exists(Keys.Files.SAVES_DIRECTORY)) Directory.CreateDirectory(Keys.Files.SAVES_DIRECTORY);

        //Ustawienia aplikacji
        if ((Keys.Files.MAIN_DIRECTORY + Keys.Files.APPLICATION_SETTINGS_FILE) != "./" && !File.Exists(Keys.Files.MAIN_DIRECTORY + Keys.Files.APPLICATION_SETTINGS_FILE)) CreateApplicationSettings();
    }

    /// <summary>
    /// Zapisuje do pliku ustawienia aplikacji
    /// </summary>
    /// <param name="settings">Obiekt przechowujący ustawienia</param>
    public static void SaveApplicationSettings(ApplicationSettings settings)
    {
        IFormatter formatter = new BinaryFormatter();
        Stream stream = new FileStream(Keys.Files.MAIN_DIRECTORY + Keys.Files.APPLICATION_SETTINGS_FILE, FileMode.Create, FileAccess.Write);
        formatter.Serialize(stream, settings);
        stream.Close();
    }

    /// <summary>
    /// Wczytuje z pliku ustawienia aplikacji
    /// </summary>
    /// <param name="settings">Obiekt przechowujący ustawienia</param>
    public static void LoadApplicationSettings(ref ApplicationSettings settings)
    {
        IFormatter formatter = new BinaryFormatter();

        //Zabezpieczenie przed nieprawiłowym plikiem, lub jego brakiem
        try
        {
            Stream stream = new FileStream(Keys.Files.MAIN_DIRECTORY + Keys.Files.APPLICATION_SETTINGS_FILE, FileMode.Open, FileAccess.Read);
            settings = (ApplicationSettings)formatter.Deserialize(stream);
            stream.Close();
        }
        catch
        {
            Debug.LogWarning("Plik ustawień aplikacji nie został poprawnie wczytany. Zostanie utworzona domyślna jego wersja");
            CreateApplicationSettings();

            Stream stream = new FileStream(Keys.Files.MAIN_DIRECTORY + Keys.Files.APPLICATION_SETTINGS_FILE, FileMode.Open, FileAccess.Read);
            settings = (ApplicationSettings)formatter.Deserialize(stream);
            stream.Close();
        }
    }

    /// <summary>
    /// Zapisuje stan rozgrywki do pliku
    /// </summary>
    /// <param name="gameSave">Opiekt przechowujący ustawienia gry</param>
    /// <param name="fileName">Nazwa pliku zapisu gry podana bez rozszerzenia</param>
    public static void SaveGame(GameSave gameSave, string fileName)
    {
        IFormatter formatter = new BinaryFormatter();
        Stream stream = new FileStream(Keys.Files.SAVES_DIRECTORY + fileName + Keys.Files.GAME_SAVE_EXTENSION, FileMode.Create, FileAccess.Write);
        formatter.Serialize(stream, gameSave);
        stream.Close();
    }

    /// <summary>
    /// Wczytuje stan rozgrywki z pliku
    /// </summary>
    /// <param name="gameSave">Opiekt przechowujący ustawienia gry</param>
    /// <param name="fileName">Nazwa pliku zapisu gry podana bez rozszerzenia</param>
    public static void LoadGame(ref GameSave gameSave, string fileName)
    {
        //Sprawdzanie, czy zapis gry o podanej nazwie istnieje
        if (File.Exists(Keys.Files.SAVES_DIRECTORY + fileName + Keys.Files.GAME_SAVE_EXTENSION))
        {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(Keys.Files.SAVES_DIRECTORY + fileName + Keys.Files.GAME_SAVE_EXTENSION, FileMode.Open, FileAccess.Read);
            gameSave = (GameSave)formatter.Deserialize(stream);
            stream.Close();
        }
        else Debug.LogError("Zapis gry o nazwie " + fileName + " nie istnieje, a nastąpiła próba jego wczytania");
    }

    public static void RemoveGame(string fileName)
    {
        string filePath = Keys.Files.SAVES_DIRECTORY + fileName + Keys.Files.GAME_SAVE_EXTENSION;

        if (File.Exists(filePath))
            File.Delete(filePath);
    }

    /// <summary>
    /// Funkcja sprawdzająca, czy plik zapisu gry istnieje
    /// </summary>
    /// <returns></returns>
    public static bool DoesSaveFileExist(string saveName)
    {
        return File.Exists(Keys.Files.SAVES_DIRECTORY + saveName + Keys.Files.GAME_SAVE_EXTENSION);
    }

    /// <summary>
    /// Funkcja zwracająca nazwe pliku zapisu gry
    /// </summary>
    /// <returns></returns>
    public static List<string> GetSavesName()
    {

        List<string> filesNameList = new List<string>();
        foreach(string s in Directory.GetFiles(Keys.Files.SAVES_DIRECTORY))
        {
            int lastDot = s.LastIndexOf('.');
            int lastSlash = s.LastIndexOf('/');

            filesNameList.Add(s.Substring(lastSlash+1, lastDot-lastSlash-1));
        }
        return filesNameList;
    }

    #region Tworzenie plików z domyślnymi wartościami

    /// <summary>
    /// Tworzy plik z domyślnymi ustawieniami aplikacji
    /// </summary>
    private static void CreateApplicationSettings()
    {
        ApplicationSettings defaultSettings;
        defaultSettings.musicVolume = Keys.Files.DefaultValues.MUSIC_VOLUME;
        defaultSettings.soundEffectsVolume = Keys.Files.DefaultValues.SOUND_EFFECTS_VOLUME;
        defaultSettings.language = Keys.Files.DefaultValues.LANGUAGE;
        defaultSettings.playerNickname = Keys.Menu.DEFAULT_USERNAME;

        IFormatter formatter = new BinaryFormatter();
        Stream stream = new FileStream(Keys.Files.MAIN_DIRECTORY + Keys.Files.APPLICATION_SETTINGS_FILE, FileMode.Create, FileAccess.Write);
        formatter.Serialize(stream, defaultSettings);
        stream.Close();
    }

    #endregion Tworzenie plików
}
