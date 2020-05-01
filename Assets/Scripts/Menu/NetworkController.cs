using Photon.Pun;
using UnityEngine;

public class NetworkController : MonoBehaviour
{
    void Start()
    {
        if (PhotonNetwork.NetworkClientState != Photon.Realtime.ClientState.ConnectedToMasterServer)
        {
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.GameVersion = Application.version;
        }
    }
}
