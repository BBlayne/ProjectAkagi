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
        Random.seed = System.DateTime.Now.Millisecond;
        Debug.Log("Spawning player..");
        GameObject lightHouse = PhotonNetwork.Instantiate(prefabName,
            spawn.position + new Vector3(Random.Range(0, 5), Random.Range(0, 5), Random.Range(0, 5)),
            spawn.rotation,
            0);
        lightHouse.GetComponent<NetworkPlayerTest>().enabled = true;
    }

    void OnGUI()
    {
        GUILayout.Label(PhotonNetwork.connectionStateDetailed.ToString());
    }
}
