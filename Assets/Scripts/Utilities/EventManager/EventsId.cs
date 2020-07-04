using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum EventsId
{
    /// <summary>
    /// Nie należy używać zgodnie z rekomendacją biblioteki PHOTON
    /// </summary>
    DoNotUse = 0,
    //RoomOwnerQuit,
    GameStateChange,
    PlayerQuit,
    PlayerMove,
    TurnChange,
    PlayerAquiredBuiding,
    Trade,
    Auction,
    Sync,
    PlayerUpgradeBuilding,
    Pay,
    PlayerLostGame
}
