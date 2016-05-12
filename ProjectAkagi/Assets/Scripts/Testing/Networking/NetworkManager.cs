using UnityEngine;
using System.Collections;

public class NetworkManager : MonoBehaviour {
    const string VERSION = "v0.0.1";
    public string roomName = "My Room";
    public string prefabName = "playerTest";
    public Transform spawn;

	// Use this for initialization
	void Start ()
    {        
        PhotonNetwork.ConnectUsingSettings(VERSION);
	}
	
	void OnJoinedLobby()
    {
        RoomOptions roomOptions = new RoomOptions()
        {
            isVisible = false,
            maxPlayers = 4
        };
        PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);
    }


    void OnJoinedRoom()
    {
        Debug.Log("Spawning player..");
        PhotonNetwork.Instantiate(prefabName,
            spawn.position,
            spawn.rotation,
            0);
    }
}
