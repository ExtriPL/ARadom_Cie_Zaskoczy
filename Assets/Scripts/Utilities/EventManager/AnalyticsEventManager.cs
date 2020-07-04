using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public class AnalyticsEventManager : MonoBehaviour
{
    public static AnalyticsEventManager instance;

    private void Start()
    {
        if (instance == null) instance = this;
        else if (instance != this) Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }
    public void SendOnStartGame()
    { 
        Analytics.CustomEvent("GameStarted", new Dictionary<string, object>
        {
            { "Player count", GameplayController.instance.session.playerCount },
            { "Time", DateTime.Now }
        }
        );
    }
}
