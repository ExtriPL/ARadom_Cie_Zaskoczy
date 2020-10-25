using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Photon.Pun;
using System;

[CustomEditor(typeof(GameplayController))]

public class GameplayControllerEditor : Editor
{
    string AddMoneyPlayerName = "";
    float AddMoneyAmount = 0;
    string DecreaseMoneyPlayerName = "";
    float DecreaseMoneyAmount = 0;
    string GiveBuildingPlayerName = "";
    int GiveBuildingPlaceID = 0;

    string GiveRangeBuildingPlayerName = "";
    int GiveRangeBuildingStartPlaceID = 0;
    int GiveRangeBuildingEndPlaceID = 0;

    int ChangeDiceRoll1 = 1;
    int ChangeDiceRoll2 = 1;

    string TeleportPlayerName = "";
    int TeleportPlaceId = 0;

    bool playersOpen = false;
    bool placesOpen = false;
    bool diceOpen = false;
    bool sessionOpen = false;
    //zmienne potrzebne do niedziałającej tabeli save
    //bool saveOpen = false;
    //bool savePlayersOpen = false;
    //bool savePlacesOpen = false;
    //bool saveDiceOpen = false;
    //bool saveSessionOpen = false;
    bool cheatsOpen = false;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GameplayController gameplayController = (GameplayController)target;

        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            PlayerTable(gameplayController);
            PlacesTable(gameplayController);
            DiceTable(gameplayController);
            SessionTable(gameplayController);
            Cheats(gameplayController);
            //Nie działa, nie ruszać
            //if(PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("loadSavedGame")) SaveTable(gameplayController);
        }
    }

    private void PlayerTable(GameplayController gameplayController)
    {
        playersOpen = EditorGUILayout.BeginFoldoutHeaderGroup(playersOpen, "PLAYERS");
        if (playersOpen)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Nr", GUILayout.Width(20f));
            GUILayout.Label("Name", GUILayout.Width(100f));
            GUILayout.Label("PID", GUILayout.Width(30f));
            GUILayout.Label("Money", GUILayout.Width(100f));
            GUILayout.Label("FieldAmount", GUILayout.Width(100f));
            GUILayout.Label("BlinkColor", GUILayout.Width(150f));
            GUILayout.Label("MainColor", GUILayout.Width(150f));
            GUILayout.Label("NetworkPlayer", GUILayout.Width(150f));
            GUILayout.Label("Inactive", GUILayout.Width(150f));
            GUILayout.EndHorizontal();

            foreach (string playerName in gameplayController.session.playerOrder)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(gameplayController.session.playerOrder.IndexOf(playerName).ToString(), GUILayout.Width(20f));
                GUILayout.Label(playerName, GUILayout.Width(100f));
                GUILayout.Label(gameplayController.session.FindPlayer(playerName).PlaceId.ToString(), GUILayout.Width(30f));
                GUILayout.Label(gameplayController.session.FindPlayer(playerName).Money.ToString(), GUILayout.Width(100f));
                int fieldAmount = 0;
                for (int i = 0; i < Keys.Board.PLACE_COUNT; i++) if (gameplayController.board.GetOwner(i)?.GetName() == playerName) fieldAmount++;
                GUILayout.Label(fieldAmount.ToString(), GUILayout.Width(100f));
                Color b = gameplayController.session.FindPlayer(playerName).BlinkColor;
                string blinkColor = "(" + Math.Round(b.r, 3) + "|" + Math.Round(b.g, 3) + "|" + Math.Round(b.b, 3) + "|" + Math.Round(b.a, 3) + ")";
                GUILayout.Label(blinkColor, GUILayout.Width(150f));
                Color m = gameplayController.session.FindPlayer(playerName).MainColor;
                string mainColor = "(" + Math.Round(m.r, 3) + "|" + Math.Round(m.g, 3) + "|" + Math.Round(m.b, 3) + "|" + Math.Round(m.a, 3) + ")";
                GUILayout.Label(mainColor, GUILayout.Width(150f));
                GUILayout.Label((gameplayController.session.FindPlayer(playerName).NetworkPlayer != null).ToString(), GUILayout.Width(100f));
                GUILayout.Label((gameplayController.session.FindPlayer(playerName).NetworkPlayer.IsInactive).ToString(), GUILayout.Width(100f));
                GUILayout.EndHorizontal();
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void PlacesTable(GameplayController gameplayController)
    {
        placesOpen = EditorGUILayout.BeginFoldoutHeaderGroup(placesOpen, "PLACES");
        if (placesOpen)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("PID", GUILayout.Width(20f));
            GUILayout.Label("Name", GUILayout.Width(100f));
            GUILayout.Label("Type", GUILayout.Width(100f));
            GUILayout.Label("Owner", GUILayout.Width(100f));
            GUILayout.Label("PlayerCount", GUILayout.Width(100f));
            GUILayout.Label("Tier", GUILayout.Width(40f));
            GUILayout.EndHorizontal();
            foreach (KeyValuePair<int, int> keyValuePair in gameplayController.board.places)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(keyValuePair.Key.ToString(), GUILayout.Width(20f));
                GUILayout.Label(gameplayController.board.GetField(keyValuePair.Key).GetFieldName(), GUILayout.Width(100f));
                GUILayout.Label(gameplayController.board.GetField(keyValuePair.Key).GetType().ToString(), GUILayout.Width(100f));
                string owner = (gameplayController.board.GetOwner(keyValuePair.Key) != null) ? gameplayController.board.GetOwner(keyValuePair.Key).GetName().ToString() : "-";
                GUILayout.Label(owner, GUILayout.Width(100f));
                int playerCount = 0;
                for (int i = 0; i < gameplayController.session.playerCount; i++)
                {
                    if (gameplayController.session.FindPlayer(i).PlaceId == keyValuePair.Key) playerCount++;
                }
                GUILayout.Label(playerCount.ToString(), GUILayout.Width(100f));
                GUILayout.Label(gameplayController.board.GetTier(keyValuePair.Key).ToString(), GUILayout.Width(40f));
                GUILayout.EndHorizontal();
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DiceTable(GameplayController gameplayController)
    {
        diceOpen = EditorGUILayout.BeginFoldoutHeaderGroup(diceOpen, "DICE");
        if (diceOpen)
        {
            RandomDice dice = gameplayController.board.dice;

            GUILayout.BeginHorizontal();
            GUILayout.Label("Round", GUILayout.Width(40f));
            GUILayout.Label("Current Player", GUILayout.Width(100f));
            GUILayout.Label("Amount of rolls", GUILayout.Width(100f));
            GUILayout.Label("Last1", GUILayout.Width(100f));
            GUILayout.Label("Last2", GUILayout.Width(100f));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label(dice.round.ToString(), GUILayout.Width(40f));
            GUILayout.Label(gameplayController.session.FindPlayer(dice.currentPlayer).GetName(), GUILayout.Width(100f));
            GUILayout.Label(dice.amountOfRolls.ToString(), GUILayout.Width(100f));
            GUILayout.Label(dice.last1.ToString(), GUILayout.Width(100f));
            GUILayout.Label(dice.last2.ToString(), GUILayout.Width(100f));
            GUILayout.EndHorizontal();
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void SessionTable(GameplayController gameplayController)
    {
        sessionOpen = EditorGUILayout.BeginFoldoutHeaderGroup(sessionOpen, "SESSION");
        if (sessionOpen)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Game State", GUILayout.Width(100f));
            GUILayout.Label("Game Time", GUILayout.Width(100f));
            GUILayout.Label("Amount Of Players", GUILayout.Width(100f));
            GUILayout.Label("Local Player", GUILayout.Width(100f));
            GUILayout.Label("Room Owner", GUILayout.Width(100f));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label(gameplayController.session.gameState.ToString(), GUILayout.Width(100f));
            GUILayout.Label(gameplayController.session.gameTime.ToString(), GUILayout.Width(100f));
            GUILayout.Label(gameplayController.session.playerCount.ToString(), GUILayout.Width(100f));
            GUILayout.Label(gameplayController.session.localPlayer.GetName(), GUILayout.Width(100f));
            GUILayout.Label(gameplayController.session.roomOwner.NickName, GUILayout.Width(100f));
            GUILayout.EndHorizontal();
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void Cheats(GameplayController gameplayController) 
    {
        cheatsOpen = EditorGUILayout.BeginFoldoutHeaderGroup(cheatsOpen, "\n CHEATS \n");
        if (cheatsOpen)
        {
            EditorGUILayout.LabelField("Add Money");
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Player Name");
            EditorGUILayout.LabelField("Amount");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            AddMoneyPlayerName = EditorGUILayout.TextField(AddMoneyPlayerName);
            AddMoneyAmount = EditorGUILayout.FloatField(AddMoneyAmount);

            if (GUILayout.Button("GO"))
            {
                if (gameplayController.session.FindPlayer(AddMoneyPlayerName) != null)
                {
                    gameplayController.session.FindPlayer(AddMoneyPlayerName).IncreaseMoney(AddMoneyAmount);
                }
            }
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.LabelField("Decrease Money");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Player Name");
            EditorGUILayout.LabelField("Amount");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            DecreaseMoneyPlayerName = EditorGUILayout.TextField(DecreaseMoneyPlayerName);
            DecreaseMoneyAmount = EditorGUILayout.FloatField(DecreaseMoneyAmount);

            if (GUILayout.Button("GO"))
            {
                if (gameplayController.session.FindPlayer(DecreaseMoneyPlayerName) != null)
                {
                    gameplayController.session.FindPlayer(DecreaseMoneyPlayerName).DecreaseMoney(DecreaseMoneyAmount);
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Give Building");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Player Name");
            EditorGUILayout.LabelField("Place ID");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            GiveBuildingPlayerName = EditorGUILayout.TextField(GiveBuildingPlayerName);
            GiveBuildingPlaceID = EditorGUILayout.IntField(GiveBuildingPlaceID);

            if (GUILayout.Button("GO"))
            {
                if (gameplayController.session.FindPlayer(GiveBuildingPlayerName) != null && gameplayController.board.GetOwner(GiveBuildingPlaceID) == null && GiveBuildingPlaceID < Keys.Board.PLACE_COUNT)
                {
                    gameplayController.banking.AquireBuilding(gameplayController.session.FindPlayer(GiveBuildingPlayerName), GiveBuildingPlaceID);
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Give Range Buildings");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Player Name");
            EditorGUILayout.LabelField("Place start ID");
            EditorGUILayout.LabelField("Place end ID");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            GiveRangeBuildingPlayerName = EditorGUILayout.TextField(GiveRangeBuildingPlayerName);
            GiveRangeBuildingStartPlaceID = EditorGUILayout.IntField(GiveRangeBuildingStartPlaceID);
            GiveRangeBuildingEndPlaceID = EditorGUILayout.IntField(GiveRangeBuildingEndPlaceID);

            if (GUILayout.Button("GO"))
            {
                if(gameplayController.session.FindPlayer(GiveRangeBuildingPlayerName) != null && GiveRangeBuildingStartPlaceID.IsBetween(0, Keys.Board.PLACE_COUNT) && GiveRangeBuildingEndPlaceID.IsBetween(0, Keys.Board.PLACE_COUNT) && GiveRangeBuildingStartPlaceID <= GiveRangeBuildingEndPlaceID)
                {
                    for(int i = GiveRangeBuildingStartPlaceID; i <= GiveRangeBuildingEndPlaceID; i++)
                    {
                        if(gameplayController.board.GetOwner(i) == null)
                        {
                            gameplayController.banking.AquireBuilding(gameplayController.session.FindPlayer(GiveRangeBuildingPlayerName), i);
                        }
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Warp player");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Dice Roll 1");
            EditorGUILayout.LabelField("Dice Roll 2");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            ChangeDiceRoll1 = EditorGUILayout.IntField(ChangeDiceRoll1);
            ChangeDiceRoll2 = EditorGUILayout.IntField(ChangeDiceRoll2);

            if (GUILayout.Button("GO"))
            {
                GameplayController.instance.board.dice.SetLast(ChangeDiceRoll1, ChangeDiceRoll2);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Teleport Player");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Player name");
            EditorGUILayout.LabelField("Place Id");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            TeleportPlayerName = EditorGUILayout.TextField(TeleportPlayerName);
            TeleportPlaceId = EditorGUILayout.IntField(TeleportPlaceId);

            if(GUILayout.Button("GO"))
            {
                GameSession session = GameplayController.instance.session;
                Player player = session.FindPlayer(TeleportPlayerName);
                if (player != null)
                    GameplayController.instance.board.TeleportPlayer(player, TeleportPlaceId);
            }

            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    /* NIE DZIAŁA
    private void SaveTable(GameplayController gameplayController)
    {
        saveOpen = EditorGUILayout.BeginFoldoutHeaderGroup(saveOpen, "SAVE");
        EditorGUILayout.EndFoldoutHeaderGroup();
        
        if (saveOpen)
        {
            savePlayersOpen = EditorGUILayout.BeginFoldoutHeaderGroup(savePlayersOpen, "PLAYERS");
            if (savePlayersOpen)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Nr", GUILayout.Width(20f));
                GUILayout.Label("Name", GUILayout.Width(100f));
                GUILayout.Label("PID", GUILayout.Width(30f));
                GUILayout.Label("Money", GUILayout.Width(100f));
                GUILayout.Label("FieldAmount", GUILayout.Width(100f));
                GUILayout.Label("BlinkColor", GUILayout.Width(150f));
                GUILayout.Label("MainColor", GUILayout.Width(150f));
                GUILayout.EndHorizontal();

                foreach (PlayerSettings ps in gameplayController.save.players)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(gameplayController.save.players.IndexOf(ps).ToString(), GUILayout.Width(20f));
                    GUILayout.Label(ps.nick);
                    GUILayout.Label(ps.fieldId.ToString(), GUILayout.Width(30f));
                    GUILayout.Label(ps.money.ToString(), GUILayout.Width(100f));
                    GUILayout.Label(ps.fieldList.Count.ToString(), GUILayout.Width(100f));
                    float[] b = ps.blinkColorComponents;
                    string blinkColor = "(" + Math.Round(b[0], 3) + "|" + Math.Round(b[1], 3) + "|" + Math.Round(b[2], 3) + "|" + Math.Round(b[3], 3) + ")";
                    GUILayout.Label(blinkColor, GUILayout.Width(150f));
                    float[] m = ps.mainColorComponents;
                    string mainColor = "(" + Math.Round(m[0], 3) + "|" + Math.Round(m[1], 3) + "|" + Math.Round(m[2], 3) + "|" + Math.Round(m[3], 3) + ")";
                    GUILayout.Label(mainColor, GUILayout.Width(150f));
                    GUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            savePlacesOpen = EditorGUILayout.BeginFoldoutHeaderGroup(savePlacesOpen, "PLACES");
            if (savePlacesOpen)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("PID", GUILayout.Width(20f));
                GUILayout.Label("Name", GUILayout.Width(100f));
                GUILayout.Label("Type", GUILayout.Width(100f));
                GUILayout.Label("Owner", GUILayout.Width(100f));
                GUILayout.Label("PlayerCount", GUILayout.Width(100f));
                GUILayout.Label("Tier", GUILayout.Width(40f));
                GUILayout.EndHorizontal();
                Debug.Log(gameplayController.save);
                
                foreach (KeyValuePair<int, Tuple<int, string>> keyValuePair in gameplayController.save.places)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(keyValuePair.Key.ToString(), GUILayout.Width(20f));
                    GUILayout.Label(keyValuePair.Value.Item2, GUILayout.Width(100f));
                    GUILayout.Label(gameplayController.board.GetField(keyValuePair.Value.Item2).GetType().ToString(), GUILayout.Width(100f));
                    string owner = "-";
                    foreach (PlayerSettings ps in gameplayController.save.players) 
                    {
                        if (ps.fieldList.Contains(keyValuePair.Key)) owner = ps.nick;
                    }
                    GUILayout.Label(owner, GUILayout.Width(100f));
                    int playerCount = 0;
                    for (int i = 0; i < gameplayController.save.players.Count; i++)
                    {
                        if (gameplayController.save.players[i].fieldId == keyValuePair.Key) playerCount++;
                    }
                    GUILayout.Label(playerCount.ToString(), GUILayout.Width(100f));
                    GUILayout.Label(gameplayController.save.tiers[keyValuePair.Key].ToString(), GUILayout.Width(40f));
                    GUILayout.EndHorizontal();
                }
                
                
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            saveDiceOpen = EditorGUILayout.BeginFoldoutHeaderGroup(saveDiceOpen, "DICE");
            if (saveDiceOpen)
            {
                DiceSettings dice = gameplayController.save.dice;

                GUILayout.BeginHorizontal();
                GUILayout.Label("Round", GUILayout.Width(40f));
                GUILayout.Label("Current Player", GUILayout.Width(100f));
                GUILayout.Label("Amount of rolls", GUILayout.Width(100f));
                GUILayout.Label("Last1", GUILayout.Width(100f));
                GUILayout.Label("Last2", GUILayout.Width(100f));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label(dice.round.ToString(), GUILayout.Width(40f));
                //GUILayout.Label(gameplayController.save.players[dice.currentPlayer].nick, GUILayout.Width(100f));
                GUILayout.Label(dice.amountOfRolls.ToString(), GUILayout.Width(100f));
                GUILayout.Label(dice.last1.ToString(), GUILayout.Width(100f));
                GUILayout.Label(dice.last2.ToString(), GUILayout.Width(100f));
                GUILayout.EndHorizontal();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            saveSessionOpen = EditorGUILayout.BeginFoldoutHeaderGroup(saveSessionOpen, "SESSION");
            if (saveSessionOpen)
            {
                

                GUILayout.BeginHorizontal();
                GUILayout.Label("Game State", GUILayout.Width(100f));
                GUILayout.Label("Game Time", GUILayout.Width(100f));
                GUILayout.Label("Amount Of Players", GUILayout.Width(100f));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label(gameplayController.save.gameState.ToString(), GUILayout.Width(100f));
                GUILayout.Label(gameplayController.save.gameTime.ToString(), GUILayout.Width(100f));
                GUILayout.Label(gameplayController.save.players.Count.ToString(), GUILayout.Width(100f));
                GUILayout.EndHorizontal();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
          
        }   
    }*/
}

