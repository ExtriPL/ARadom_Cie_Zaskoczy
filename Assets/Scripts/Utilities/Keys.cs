using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public static class Keys
{
    /// <summary>
    /// Wiadomości wyświetlane przez MessageSystem.
    /// </summary>
    public static class Messages
    {
        /// <summary>
        /// Komunikat wyświetlany podczas wstrzymania gry.
        /// </summary>
        public const string GAME_HAS_BEEN_PAUSED = "<color=red>Gra została wstrzymana</color>";
        /// <summary>
        /// Komunikat wyświetlany przy wychodzeniu gracza z rozgrywki.
        /// </summary>
        public const string PLAYER_LEAVE = "<color=red>opuścił grę</color>";
    }

    public static class Popups
    {
        /// <summary>
        /// Maksymalny domyślny czas istnienia popup-u
        /// </summary>
        public const float MAX_EXISTING_TIME = 20f;
        /// <summary>
        /// Ilość popup-ów tego samego typu, które mogą zostać jednocześnie wyświetlone
        /// </summary>
        public const int SHOWED_AMOUNT = 1;
        /// <summary>
        /// Ilość przycisków, jaką jest w stanie wyświetlić QuestionBox
        /// </summary>
        public const int QUESTIONBOX_BUTTONS_AMOUNT = 4;

        /// <summary>
        /// Domyślne ustawienia popup-u typu Formatted
        /// </summary>
        public static class Formated
        {
            /// <summary>
            /// Domyślna szerokość FormattedBox-u
            /// </summary>
            public static float DEFAULT_WIDTH = 600f;
            /// <summary>
            /// Domyślna wysokość FormattedBox-u
            /// </summary>
            public static float DEFAULT_HEIGHT = 1000f;
            /// <summary>
            /// Domyślna szerokość zawartości FormattedBox-u
            /// </summary>
            public static float DEFAULT_CONTENT_WIDTH = 600f;
            /// <summary>
            /// Domyślna wysokość zawartości FormattedBox-u
            /// </summary>
            public static float DEFAULT_CONTENT_HEIGHT = 1000f;
            /// <summary>
            /// Domyślna wysokość linii FormattedBox-u
            /// </summary>
            public static float DEFUALT_LINE_HEIGHT = 10f;
            /// <summary>
            /// Domyślny symbol pustki FormattedBox-u
            /// </summary>
            public static string EMPTY_SYMBOL = "_empty";
        }
    }

    /// <summary>
    /// Ustawienia planszy.
    /// </summary>
    public static class Board 
    {   
        /// <summary>
        /// Ilość pól na planszy.
        /// </summary>
        public const int FIELD_COUNT = 18;
        /// <summary>
        /// Liczba boków wielokąta, jakim jest plansza.
        /// </summary>
        public const int BOARD_SIDES = 6;
        /// <summary>
        /// Liczba pól przypadająca na każdą stronę planszy.
        /// </summary>
        public const int FIELDS_PER_SIDE = FIELD_COUNT / BOARD_SIDES + 1;
        /// <summary>
        /// Szerokość prostokąta, w który wpisany jest wielokąt. Kierunek tego wymiaru (boku prostokąta) jest zgodny z kierunkiem boku wielokąta tworzącego planszę
        /// </summary>
        public const float FIELD_WIDTH = 0.075f * SCALLING_FACTOR;
        /// <summary>
        /// Wysokość prostokąta, w który wpisany jest wielokąt. Kierunek tego wymiaru (boku prostokąta) jest prostopadły do boku wielokąta tworzącego planszę i skierowany radialnie do środka planszy
        /// </summary>
        public static float FIELD_HEIGHT = FIELD_WIDTH / Mathf.Cos(Mathf.PI / BOARD_SIDES);
        /// <summary>
        /// Długość boku wielokąta, jakim jest pole
        /// </summary>
        public static float FIELD_SIDE_LENGHT = FIELD_WIDTH * Mathf.Tan(Mathf.PI / BOARD_SIDES);
        /// <summary>
        /// Miara kąta wewnętrznego figury, jaką jest plansza.
        /// /// angel = anioł, angle = kąt, trzeba to poprawić dzbanie
        /// </summary>
        public const float INTERIOR_BOARD_ANGEL = Mathf.PI * (BOARD_SIDES - 2.0f) / BOARD_SIDES;
        /// <summary>
        /// Odległość środka pola na boku planszy od środka planszy.
        /// </summary>
        public static float SIDE_DISTANCE_FROM_CENTER = SIDE_LENGHT / (2f * Mathf.Tan(Mathf.PI / BOARD_SIDES));
        /// <summary>
        /// Nazwa obrazka w bazie danych AR, pod jakim umieszczona jest plansza.
        /// </summary>
        public const string AR_IMAGE_NAME = "board";
        /// <summary>
        /// Rozmiar planszy w rzeczywistości.
        /// </summary>
        public const float BOARD_REAL_SIZE = 0.375f;
        /// <summary>
        /// Rozmiar planszy podany w bazie danych obrazków na potrzeby wykrywania triggerów.
        /// </summary>
        public const float BOARD_AR_SIZE = 1f;
        /// <summary>
        /// Współczynnik przeskalowania planszy, określający ilukrotnie jest ona mniejsza w rzeczywistości od wymiarów podanych w AR.
        /// SCALING, nie SCALLING
        /// </summary>
        public const float SCALLING_FACTOR = BOARD_AR_SIZE / BOARD_REAL_SIZE;
        /// <summary>
        /// Mnożnik skali budynku wyświetlanego na środku planszy.
        /// </summary>
        public const float CENTER_BUILDING_SCALE_MULTIPLIER = 2f;
        /// <summary>
        /// Długość boku planszy
        /// </summary>
        public const float SIDE_LENGHT = (FIELDS_PER_SIDE - 1) * FIELD_WIDTH;

        /// <summary>
        /// Ustawienia świateł podświetlających pole
        /// </summary>
        public static class Backlight
        {
            /// <summary>
            /// Grubość obiektu odpowiedzialnego za podświetlenie
            /// </summary>
            public const float THICKNESS = 0.001f;
            /// <summary>
            /// Przesunięcie kąta obrotu boków podświetlenia (by móc je wyrównać)
            /// angel = anioł, angle = kąt, trzeba to poprawić dzbanie
            /// </summary>
            public const float START_ANGEL = Mathf.PI / BOARD_SIDES;
            /// <summary>
            /// Okreś cyklu świecenia podawany w sekundach
            /// </summary>
            public const float SHINING_PERIOD = 6f;
        }
    }

    /// <summary>
    /// Nazwy poszczególnych scen.
    /// </summary>
    public static class SceneNames
    {
        public const string MAIN_MENU = "MainMenu";
    }

    /// <summary>
    /// Wszystkie klucze dotyczące rozgrywki.
    /// </summary>
    public static class Gameplay
    {
        /// <summary>
        /// Docelowy framerate aplikacji
        /// </summary>
        public const int TARGET_FRAMERATE = 60;
        /// <summary>
        /// Ilość pieniędzy, jaką dostają gracze na start.
        /// </summary>
        public const float START_MONEY = 100;
        /// <summary>
        /// Minimalna ilość możliwa do wyrzucenia przez kostke.
        /// </summary>
        public const int MIN_DICE_VALUE = 1;
        /// <summary>
        /// Maksymalna ilość możliwa do wyrzucenia przez kostke.
        /// </summary>
        public const int MAX_DICE_VALUE = 6;
    }

    public static class Files
    {
        /// <summary>
        /// Ścieżka do głównego katalogu plików gry.
        /// </summary>
        public static string MAIN_DIRECTORY = Application.persistentDataPath + "/ApplicationData/";
        /// <summary>
        /// Ścieżka do folderu z zapisami gry.
        /// </summary>
        public static string SAVES_DIRECTORY = MAIN_DIRECTORY + "saves/";

        /// <summary>
        /// Nazwa pliku z ustawieniami aplikacji.
        /// </summary>
        public const string APPLICATION_SETTINGS_FILE = "application.settings";
        /// <summary>
        /// Rozszerzenie plików zapisu stanu gry.
        /// </summary>
        public const string GAME_SAVE_EXTENSION = ".save";

        /// <summary>
        /// Domyślne wartości do tworzenia nowych plików ustawień.
        /// </summary>
        public static class DefaultValues
        {
            #region ApplicationSettings

            /// <summary>
            /// Głośność muzyki.
            /// </summary>
            public const float MUSIC_VOLUME = -20f;
            /// <summary>
            /// Głośność efektów dźwiękowych.
            /// </summary>
            public const float SOUND_EFFECTS_VOLUME = 0f;
            /// <summary>
            /// Język gry.
            /// </summary>
            public const Languages LANGUAGE = Languages.Polish;

            #endregion ApplicationSettings
        }
    }
    /// <summary>
    /// Właściwości pokoju tworzonego gdy gra rozpocznie się od sceny Game
    /// </summary>
    public static class DefaultRoom
    {
        /// <summary>
        /// Nick gracza
        /// </summary>
        public const string NICKNAME = "admin";
        /// <summary>
        /// Nazwa pokoju
        /// </summary>
        public const string ROOM_NAME = "caseOfEmergency";
    }
}
