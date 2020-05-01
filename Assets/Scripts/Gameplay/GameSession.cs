using ExitGames.Client.Photon;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameSession
{
    private GameplayController gameplayController;
    /// <summary>
    /// Czas rozpoczęcia rozgrywki
    /// </summary>
    public float gameStartTime
    {
        get
        {
            return (float)PhotonNetwork.CurrentRoom.CustomProperties["session_gameStartTime"];
        }
        private set
        {
            Hashtable table = new Hashtable();
            table.Add("session_gameStartTime", value);
            PhotonNetwork.CurrentRoom.SetCustomProperties(table);
        }
    }
    /// <summary>
    /// Czas, przez jaki toczy się rozgrywka
    /// </summary>
    public float currentGameTime => Time.time - gameStartTime;

    /// <summary>
    /// Stan gry
    /// </summary>
    public GameState gameState
    {
        get
        {
            return (GameState)(int)PhotonNetwork.CurrentRoom.CustomProperties["gameState"];
        }
        set
        {
            GameState previousState;
            //Zabezpieczenie przy pierwszym odpaleniu (odpowiednia zmienna nie istnieje)
            try
            {
               previousState = gameState;
            }
            catch
            {
                previousState = GameState.running;
            }

            Hashtable table = new Hashtable();
            table.Add("gameState", (int)value);
            PhotonNetwork.CurrentRoom.SetCustomProperties(table);
            
            //Wysyła informacje o zmianie stanu rozgrywki do graczy  
            GameState newState = value;
            EventManager.instance.SendOnRoomStateChanged(previousState, newState);
        }
    }

    /// <summary>
    /// Lista graczy na planszy
    /// </summary>
    private List<Player> players = new List<Player>();
    /// <summary>
    /// Instancja lokalnego gracza
    /// </summary>
    public Player localPlayer => FindPlayer(PhotonNetwork.LocalPlayer.NickName);
    /// <summary>
    /// Instancja właściciela pokoju rozgrywki
    /// </summary>
    public Photon.Realtime.Player roomOwner => PhotonNetwork.CurrentRoom.GetPlayer(PhotonNetwork.CurrentRoom.MasterClientId);

    #region Inicjalizacja

    public void Init(GameplayController gameplayController)
    {
        this.gameplayController = gameplayController;

        LoadPlayers();

        //Inicjalizacja sieci przez właściciela pokoju 
        if(roomOwner.IsLocal)
        {
            //Wczytywanie ustawień z pliku
            if (gameplayController.loadedFromSave)
            {
                GameSave save = gameplayController.save;
                gameState = save.gameState;
                gameStartTime = Time.time - save.gameTime; //Obliczanie czasu w taki sposób, by obecny czas gry zgadzał się po wczytaniu gry. Przez ten sposób zmienna gameStartTime może mieć wartość ujemną
                //Przywracanie właściwości graczy
                foreach(PlayerSettings ps in save.players)
                {
                    Player p = FindPlayer(ps.nick);
                    if(p != null)
                    {
                        p.SetTurnsToSkip(ps.turnsToSkip);
                        p.SetMoney(ps.money);
                        p.fieldId = ps.fieldId;
                        foreach (int i in ps.fieldList) p.AddOwnership(i);
                    }
                    else
                    {
                        Debug.LogWarning("Gracz o nicku " + ps.nick + " nie jest podłączony");
                    }
                }
            }
            else
            {
                gameState = GameState.running;
                gameStartTime = Time.time;
            }
        }
    }

    public void SaveToInstance(ref GameSave save)
    {
        save.gameState = gameState;
        save.gameTime = currentGameTime;
        save.players = new List<PlayerSettings>();
        foreach(Player p in players)
        {
            PlayerSettings ps = new PlayerSettings();
            ps.fieldId = p.fieldId;
            ps.fieldList = p.GetOwnedFields();
            ps.money = p.money;
            ps.nick = p.GetName();
            ps.turnsToSkip = p.turnsToSkip;
            save.players.Add(ps);
        }
    }

    public void SubscribeEvents()
    {
        EventManager.instance.onGameStateChanged += OnGameStateChanged;
        EventManager.instance.onPlayerQuit += OnPlayerQuit;
    }

    public void UnsubscribeEvents()
    {
        EventManager.instance.onGameStateChanged -= OnGameStateChanged;
        EventManager.instance.onPlayerQuit -= OnPlayerQuit;
    }

    #endregion Inicjalizacja

    #region Eventy

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
        Player player = FindPlayer(playerName);
        RemovePlayer(player.GetName());
        MessageSystem.instance.AddMessage("<color=red>" + player.GetName() + "</color> " + Keys.Messages.PLAYER_LEAVE, MessageType.MediumMessage);
    }

    #endregion Eventy

    #region Funkcje graczy

    /// <summary>
    /// Odnajduje gracza na liście graczy
    /// </summary>
    /// <param name="name">Nazwa gracza</param>
    /// <returns>Gracz o podanej nazwie</returns>
    public Player FindPlayer(string name)
    {
        foreach (Player p in players)
        {
            if (p.GetName().Equals(name)) return p;
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
        if (numberInQueue >= 0 && numberInQueue < players.Count) return players[numberInQueue];
        else return null;
    }

    /// <summary>
    /// Zwraca liczbę graczy na liście
    /// </summary>
    /// <returns>Liczba graczy</returns>
    public int GetPlayersCount()
    {
        return players.Count;
    }

    /// <summary>
    /// Zwraca miejsce gracza w kolejce
    /// </summary>
    /// <param name="name">Nazwa gracza</param>
    /// <returns>Jeżeli gracz istnieje: jego numer, jeżeli nie istnieje: -1</returns>
    public int GetPlayerNumberInQueue(string name)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].GetName().Equals(name)) return i;
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
        Player toRemove = FindPlayer(name);
        if (toRemove != null) players.Remove(toRemove);
        else Debug.LogError("Gracz o podanej nazwie(" + name + ") nie istnieje");
    }

    /// <summary>
    /// Usuwa gracza z listy graczy
    /// </summary>
    /// <param name="numberInQueue">Numer gracza w kolejce</param>
    public void RemovePlayer(int numberInQueue)
    {
        if (numberInQueue >= 0 && numberInQueue < players.Count) players.RemoveAt(numberInQueue);
        else Debug.LogError("Nie można usunąć gracza o podanym numerze(" + numberInQueue + ") ponieważ nie ma go na liście");
    }

    /// <summary>
    /// Usuwa gracza z listy graczy
    /// </summary>
    /// <param name="player">Instancja gracza</param>
    public void RemovePlayer(Player player)
    {
        players.Remove(player);
    }

    /// <summary>
    /// Wczytuje graczy do listy
    /// </summary>
    private void LoadPlayers()
    {
        foreach (Photon.Realtime.Player p in PhotonNetwork.CurrentRoom.Players.Values)
        {
            Player player = new Player(p);
            players.Add(player);
        }
    }

    #endregion #Funkcje graczy
}
