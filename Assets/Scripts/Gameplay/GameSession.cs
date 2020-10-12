using ExitGames.Client.Photon;
using Photon.Pun;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

[System.Serializable]
public class GameSession : IEventSubscribable
{
    /// <summary>
    /// Łączny czas rozgrywki
    /// </summary>
    public float gameTime
    {
        get => lastGameTime + currentGameTime;
    }
    /// <summary>
    /// Czas rozgrywki, który upłynął od jej rozpoczęcia, do ostatniego zapisania savea
    /// </summary>
    public float lastGameTime 
    { 
        get
        {
            return (float)PhotonNetwork.CurrentRoom.CustomProperties["session_lastGameTime"];
        }
        set
        {
            Hashtable table = new Hashtable()
            {
                { "session_lastGameTime", value }
            };

            PhotonNetwork.CurrentRoom.SetCustomProperties(table);
        }
    }
    /// <summary>
    /// Czass gry, który upłynął od ostatniego wczytania savea
    /// </summary>
    public float currentGameTime { get; private set; }
    /// <summary>
    /// Stan gry
    /// </summary>
    public GameState gameState
    {
        get
        {
            return (GameState)(int)PhotonNetwork.CurrentRoom.CustomProperties["session_gameState"];
        }
        set
        {
            GameState previousState;
            //Zabezpieczenie przy pierwszym odpaleniu (odpowiednia zmienna nie istnieje)
            if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("session_gameState")) previousState = gameState;
            else previousState = GameState.running;

            Hashtable table = new Hashtable();
            table.Add("session_gameState", (int)value);
            PhotonNetwork.CurrentRoom.SetCustomProperties(table);

            //Wysyła informacje o zmianie stanu rozgrywki do graczy  
            GameState newState = value;
            EventManager.instance.SendOnGameStateChanged(previousState, newState);
        }
    }

    /// <summary>
    /// Lista ustalaąca kolejność graczy w rozgrywce
    /// </summary>
    public List<string> playerOrder 
    {
        get 
        {
            List<string> list = new List<string>();
            list.AddRange((string[])PhotonNetwork.CurrentRoom.CustomProperties["session_playerOrder"]);
            return list;
        }
        set 
        {
            Hashtable table = new Hashtable();
            table.Add("session_playerOrder", value.ToArray());
            PhotonNetwork.CurrentRoom.SetCustomProperties(table);
        }
    }
    /// <summary>
    /// Lista graczy, zmapowana na ich nicki w ustalonej serwerowo kolejności
    /// </summary>
    private Dictionary<string, Player> players = new Dictionary<string, Player>();
    /// <summary>
    /// Zwraca liczbę graczy na liście
    /// </summary>
    /// <returns>Liczba graczy</returns>
    public int playerCount => playerOrder.Count;

    /// <summary>
    /// Instancja lokalnego gracza
    /// </summary>
    public Player localPlayer => FindPlayer(PhotonNetwork.LocalPlayer.NickName);
    /// <summary>
    /// Instancja właściciela pokoju rozgrywki
    /// </summary>
    public Photon.Realtime.Player roomOwner => PhotonNetwork.MasterClient;

    #region Inicjalizacja

    public void Update()
    {
        //Czas nie jest liczony, gdy rozgrywka jest wstrzymana
        if (gameState == GameState.running) currentGameTime += Time.deltaTime;
    }

    public void SubscribeEvents()
    {
        EventManager.instance.onGameStateChanged += OnGameStateChanged;
        EventManager.instance.onPlayerQuited += OnPlayerQuit;
    }

    public void UnsubscribeEvents()
    {
        EventManager.instance.onGameStateChanged -= OnGameStateChanged;
        EventManager.instance.onPlayerQuited -= OnPlayerQuit;
    }

    /// <summary>
    /// Ładowanie ustawień z pliku zapisu
    /// </summary>
    /// <param name="save">Informację wyciągnięte z pliku zapisu</param>
    public void LoadFromSave(ref GameSave save)
    {
        gameState = save.gameState;
        lastGameTime = save.gameTime;

        List<string> playerOrder = new List<string>();
        foreach (PlayerSettings ps in save.players)
        {

            Player p = FindPlayer(ps.nick);
            if (p != null)
            {
                playerOrder.Add(ps.nick);
                p.SetTurnsToSkip(ps.turnsToSkip);
                p.SetMoney(ps.money);
                p.PlaceId = ps.placedId;
                p.MainColor = new Color
                (
                    ps.mainColorComponents[0],
                    ps.mainColorComponents[1],
                    ps.mainColorComponents[2],
                    ps.mainColorComponents[3]
                );
                p.BlinkColor = new Color
                (
                    ps.blinkColorComponents[0],
                    ps.blinkColorComponents[1],
                    ps.blinkColorComponents[2],
                    ps.blinkColorComponents[3]
                );
                p.IsLoser = ps.isLoser;
                p.TookLoan = ps.tookLoan;
                p.OutstandingAmount = ps.outstandingAmount;
                p.Imprisoned = ps.imprisoned;
                foreach (int i in ps.fieldList) p.AddOwnership(i);
            }
            else
            {
                Debug.LogWarning("Gracz o nicku " + ps.nick + " nie jest podłączony");
            }
        }

        this.playerOrder = playerOrder;
    }

    #endregion Inicjalizacja

    #region Sterowanie rozgrywką

    /// <summary>
    /// Zapisywanie danych z sesji do obiektu złużącego do zapisu gry
    /// </summary>
    /// <param name="save">Obiekt służący do zapisu gry</param>
    public void SaveProgress(ref GameSave save)
    {
        save.gameState = gameState;
        save.gameTime = gameTime;
        save.players = new List<PlayerSettings>();
        foreach (string p in playerOrder)
        {
            Player player = FindPlayer(p);
            PlayerSettings ps = new PlayerSettings();
            ps.placedId = player.PlaceId;
            ps.fieldList = player.GetOwnedFields();
            ps.money = player.Money;
            ps.nick = player.GetName();
            ps.turnsToSkip = player.TurnsToSkip;

            //Kolory
            ps.mainColorComponents = new float[4];
            ps.mainColorComponents[0] = player.MainColor.r;
            ps.mainColorComponents[1] = player.MainColor.g;
            ps.mainColorComponents[2] = player.MainColor.b;
            ps.mainColorComponents[3] = player.MainColor.a;

            ps.blinkColorComponents = new float[4];
            ps.blinkColorComponents[0] = player.BlinkColor.r;
            ps.blinkColorComponents[1] = player.BlinkColor.g;
            ps.blinkColorComponents[2] = player.BlinkColor.b;
            ps.blinkColorComponents[3] = player.BlinkColor.a;

            ps.isLoser = player.IsLoser;
            ps.tookLoan = player.TookLoan;
            ps.outstandingAmount = player.OutstandingAmount;
            ps.imprisoned = player.Imprisoned;

            save.players.Add(ps);
        }
    }

    #endregion Sterowanie rozgrywką

    #region Obsługa eventów

    /// <summary>
    /// Przypisuje zmiennej wewnętrznej nowy stan gry.
    /// Wyświetla komunikaty o stanie rozgrywki.
    /// Wpływa na rozgrywkę i graczy w zależności od stanu rozgrywki.
    /// </summary>
    /// <param name="previousState">Poprzedni stan gry</param>
    /// <param name="newState">Nowy stan gry</param>
    private void OnGameStateChanged(GameState previousState, GameState newState)
    {
        //Jeżeli pokój został zniszczony, wyrzuca gracza
        if (newState == GameState.roomDestroyed)
        {
            PhotonNetwork.LeaveRoom();
            PhotonNetwork.LoadLevel(Keys.SceneNames.MAIN_MENU);
        }
    }

    /// <summary>
    /// Odnajduje gracza, który wyszedł z gry na liście i usuwa go z tej listy.
    /// Wyświetla komunikat o wyjściu gracza.
    /// </summary>
    /// <param name="playerName">Nazwa gracza na liście</param>
    private void OnPlayerQuit(string playerName)
    {
        if (playerName != localPlayer.GetName())
        {
            string message = SettingsController.instance.languageController.GetWord("PLAYER") + playerName + SettingsController.instance.languageController.GetWord("PLAYER_LEFT");
            if (FindPlayer(playerName).NetworkPlayer.IsInactive) message = SettingsController.instance.languageController.GetWord("PLAYER") + playerName + SettingsController.instance.languageController.GetWord("KICKED_FOR_INACTIVITY");
            RemovePlayer(playerName);
            InfoPopup playerLeft = new InfoPopup(message, 2f);
            PopupSystem.instance.AddPopup(playerLeft);
        }
        else 
        {
            PhotonNetwork.LeaveRoom();
            PhotonNetwork.LoadLevel(Keys.SceneNames.MAIN_MENU);
        }
    }

    #endregion Obsługa eventów

    #region Funkcje graczy

    /// <summary>
    /// Odnajduje gracza na liście graczy
    /// </summary>
    /// <param name="name">Nazwa gracza</param>
    /// <returns>Gracz o podanej nazwie</returns>
    public Player FindPlayer(string name)
    {
        if (playerOrder.Contains(name))
        {
            return players[name];
        }
        return null;
    }

    /// <summary>
    /// Odnajduje gracza na liście graczy
    /// </summary>
    /// <param name="numberInQueue">Numer gracza w kolejce</param>
    /// <returns>Gracz o podanym numerze w kolejce</returns>
    public Player FindPlayer(int numberInQueue)
    {
        if (numberInQueue >= 0 && numberInQueue < players.Count) return players[playerOrder[numberInQueue]];
        else return null;
    }

    /// <summary>
    /// Zwraca miejsce gracza w kolejce
    /// </summary>
    /// <param name="name">Nazwa gracza</param>
    /// <returns>Jeżeli gracz istnieje: jego numer, jeżeli nie istnieje: -1</returns>
    public int GetPlayerNumberInQueue(string name)
    {
        if (playerOrder.Contains(name)) 
        { 
            return playerOrder.IndexOf(name);
        }

        return -1;
    }

    /// <summary>
    /// Zwraca miejsce gracza w kolejce
    /// </summary>
    /// <param name="player">Instancja gracza</param>
    /// <returns>Jeżeli gracz istnieje: jego numer, jeżeli nie istnieje: -1</returns>
    public int GetPlayerNumberInQueue(Player player)
    {
        return GetPlayerNumberInQueue(player.GetName());
    }

    /// <summary>
    /// Usuwa gracza z listy graczy
    /// </summary>
    /// <param name="name">Nazwa gracza</param>
    public void RemovePlayer(string name)
    {
        if (playerOrder.Contains(name))
        {
            players.Remove(name);
            List<string> playerOrder = this.playerOrder;
            playerOrder.Remove(name);
            this.playerOrder = playerOrder;
        }
        else Debug.LogError("Gracz o podanej nazwie(" + name + ") nie istnieje");
    }

    /// <summary>
    /// Usuwa gracza z listy graczy
    /// </summary>
    /// <param name="numberInQueue">Numer gracza w kolejce</param>
    public void RemovePlayer(int numberInQueue)
    {
        if (numberInQueue >= 0 && numberInQueue < players.Count) RemovePlayer(playerOrder[numberInQueue]);
        else Debug.LogError("Nie można usunąć gracza o podanym numerze(" + numberInQueue + ") ponieważ nie ma go na liście");
    }

    /// <summary>
    /// Usuwa gracza z listy graczy
    /// </summary>
    /// <param name="player">Instancja gracza</param>
    public void RemovePlayer(Player player)
    {
        RemovePlayer(player.GetName());
    }

    /// <summary>
    /// Dodaje graczy do listy graczy
    /// </summary>
    /// <param name="player">Obiekt gracza</param>
    public void AddPlayer(Player player)
    {
        players.Add(player.GetName(), player);
    }

    /// <summary>
    /// Funkcja służąca do wyrzucania 
    /// </summary>
    /// <param name="player"></param>
    public void KickPlayer(Player player) 
    {
        if(GameplayController.instance.board.dice.currentPlayer == player.GetName()) GameplayController.instance.EndTurn();
        EventManager.instance.SendOnPlayerQuited(player.GetName());
    }

    #endregion #Funkcje graczy
}
