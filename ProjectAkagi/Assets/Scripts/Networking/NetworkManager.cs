using UnityEngine;
using System.Collections;

namespace Poker
{
    [RequireComponent(typeof(GameController))]
    [RequireComponent(typeof(PhotonView))]
    public class NetworkManager : MonoBehaviour {
        const string VERSION = "v0.0.3";
        public string roomName = "My Room";
        public string prefabName = "poker_player";
        public GameController gameMaster = null;

        public PhotonView mPhotonView = null;
        public static int host;
        GameObject player1;

        void Awake()
        {
            if (mPhotonView == null)
                mPhotonView = GetComponent<PhotonView>();
        }

        // Use this for initialization
        void Start()
        {
            PhotonNetwork.ConnectUsingSettings(VERSION);
        }

        void OnJoinedLobby()
        {
            RoomOptions roomOptions = new RoomOptions()
            {
                isVisible = false,
                maxPlayers = 6
            };
            PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);
        }


        void OnJoinedRoom()
        {
            //Debug.Log(GameObject.FindGameObjectsWithTag("Player").Length);
            Debug.Log("My id is: " + PhotonNetwork.player.ID + ", the number of players connected is: " + PhotonNetwork.playerList.Length);
            if (PhotonNetwork.playerList.Length == 1)
            {
                // I am the first client.

            }
            else
            {
                // I am not the first client

            }
            
            // Find and cache our transform for slot{id}; from the perspective of every other client, this is
            // our position.            
            Transform slot = GameObject.Find("PlayerSlotPositions").transform.Find("slot"+ (PhotonNetwork.player.ID % 6));
            GameObject player = PhotonNetwork.Instantiate(prefabName,
                slot.position,
                slot.rotation,
                0);
            // Across network, reassign parent transforms accordingly.
            //player.GetComponent<Player>().photonView.RPC("setParent", PhotonTargets.All, PhotonNetwork.player.ID);
            player.name = "player";
            // Locally however, we are always slot1.
            // Reassign local parent to the slot1's transform reset it's position.
            player.transform.SetParent(GameObject.Find("PlayerSlotPositions").transform.Find("slot1").transform);
            player.transform.localPosition = new Vector3(0,0,0);

            //
            player.GetComponent<Renderer>().material = gameMaster.mats[PhotonNetwork.player.ID - 1];
            //player.GetComponent<Player>().enabled = true;
            gameMaster.photonView.RPC("AddPlayer", PhotonTargets.AllBuffered);            
            player.GetComponent<Player>().photonView.RPC("InitColour", PhotonTargets.AllBuffered, PhotonNetwork.player.ID);
            player.GetComponent<Player>().photonView.RPC("SetId", PhotonTargets.AllBuffered, PhotonNetwork.player.ID);
            player.GetComponent<Player>().photonView.RPC("SetUI", PhotonTargets.AllBuffered);
            
            if (PhotonNetwork.playerList.Length == 1)
            {
                host = PhotonNetwork.player.ID;
            }
            else
            {

            }
            
            
        }

        void OnPhotonPlayerConnected(PhotonPlayer other)
        {
            if (PhotonNetwork.player.ID == host)
            {
                Debug.Log("I am the host, and a player has joined.");
                Debug.Log(GameObject.FindGameObjectsWithTag("Player").Length);
                GameObject.Find("Canvas").transform.FindChild("btnStart").gameObject.SetActive(true);
            }
            else
            {

            }
            
        }

        [PunRPC]
        void AssignHost(int _host)
        {
            host = _host;
            Debug.Log("Host is player: " + host);
        }

        void OnGUI()
        {
            GUI.Label(new Rect(0, 0, 200, 200), PhotonNetwork.connectionStateDetailed.ToString());
            //GUILayout.Label(PhotonNetwork.connectionStateDetailed.ToString());
        }
    }
}

