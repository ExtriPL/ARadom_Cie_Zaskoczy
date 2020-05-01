using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyController : MonoBehaviourPunCallbacks
{
    //Przyciski
    [SerializeField] private Button createRoomButton;

    #region Panele
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject lobbyPanel;         //panel lobby
    [SerializeField] private GameObject creatingRoomPanel;  //panel tworzenia pokoju
    [SerializeField] private GameObject joiningLobbyPanel;  //panel łączenia z lobby
    [SerializeField] private GameObject leavingLobbyPanel;  //panel łączenia z lobby
    #endregion

    private string roomName;    //trzyma nazwe pokoju
    private int roomSize;       //trzyma ilość graczy

    private List<RoomInfo> RoomList = new List<RoomInfo>();    //lista pokoi
    [SerializeField] private Transform roomsContainer;  //container do trzymania pokoi
    [SerializeField] private GameObject roomListingPrefab; //prefab pokoju


    #region Funkcje PHOTON
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {    
        int tempIndex;
        foreach (RoomInfo room in roomList.ToArray()) 
        {
            if (roomList != null) 
            {
                tempIndex = RoomList.FindIndex(x => x.Name == room.Name);  //index pokoju
                removeOldRoom(room);        
            }
            else 
            {
                tempIndex = -1;
            }

            if (room.RemovedFromList) 
            {        
                if (roomsContainer.childCount > 0)
                {
                    GameObject gobj = roomsContainer.GetChild(tempIndex).gameObject;
                    RoomList.Remove(room);
                    Destroy(gobj);
                }
            }
            else
            {
                
                RoomList.Add(room);
            }
        }        
        ClearRoomListing();
        ListRooms();
        CheckRoomInputs();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("Błąd podczas tworzenia pokoju");
    }

    public override void OnJoinedLobby()
    {
        ClearRoomListing();
        ListRooms();

        StartCoroutine(SwitchAfterTime(0.5f, joiningLobbyPanel, lobbyPanel));
    }

    public override void OnLeftLobby()
    {
        StartCoroutine(SwitchAfterTime(0.5f, leavingLobbyPanel, menuPanel));
    }
    #endregion

    #region Inne Funkcje
    private bool removeOldRoom(RoomInfo room)
    {
        RoomInfo oldRoom = RoomList.Find(x => x.Name == room.Name);
        if (oldRoom != null)
        {
            RoomList.Remove(oldRoom);
            return true;
        }

        return false;
    }

    public void ClearRoomListing()
    {
        if (roomsContainer != null)
        {
            for (int i = roomsContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(roomsContainer.GetChild(i).gameObject);
            }
        }
    }
   
    public void ListRooms()
    {
        foreach (RoomInfo room in RoomList)
        {
            GameObject tempList = Instantiate(roomListingPrefab, roomsContainer);
            RoomButton button = tempList.GetComponent<RoomButton>();
            button.SetRoom(room.Name, room.MaxPlayers, room.PlayerCount);
        }
    }

     

    private void CheckRoomInputs()
    {
        RoomInfo existingRoom = RoomList.Find(x => x.Name == roomName);
        if (roomName != "" && !RoomList.Contains(existingRoom) && roomSize > 1)
        {
            createRoomButton.interactable = true;
        }
        else
        {
            createRoomButton.interactable = false;
        }

        if (RoomList.Contains(existingRoom))
        {
            createRoomButton.GetComponentInChildren<Text>().text = "Pokój już istnieje";
        }
        else
        {
            createRoomButton.GetComponentInChildren<Text>().text = "Dodaj pokój";
        }
    }

    public void OnRoomNameChanged(string nameIn)
    {
        roomName = nameIn;
        CheckRoomInputs();
    }
    public void OnRoomSizeChanged(string sizeIn)
    {
        if (sizeIn != "")
        {
            roomSize = int.Parse(sizeIn);
        }
        else
        {
            roomSize = 0;
        }
        CheckRoomInputs();
    }
    #endregion

    #region Przyciski
    public void CreateRoom()    //CreateRoom Button
    {
        Debug.Log("Created room");
        RoomOptions roomOptions = new RoomOptions() { IsVisible = true, IsOpen = true, MaxPlayers = (byte)roomSize };
        PhotonNetwork.CreateRoom(roomName, roomOptions);

        lobbyPanel.SetActive(false);
        creatingRoomPanel.SetActive(true);
    }

    public void MatchmakingCancel()
    {
        PhotonNetwork.LeaveLobby();

        lobbyPanel.SetActive(false);
        leavingLobbyPanel.SetActive(true);
    }
    #endregion

    IEnumerator SwitchAfterTime(float time, GameObject currentPanel, GameObject nextPanel)
    {
        yield return new WaitForSeconds(time);

        currentPanel.SetActive(false);
        nextPanel.SetActive(true);
    }
}