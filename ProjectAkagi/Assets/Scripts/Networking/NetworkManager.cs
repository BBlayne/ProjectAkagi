using UnityEngine;
using System.Collections;

namespace Poker
{
    [RequireComponent(typeof(GameController))]
    [RequireComponent(typeof(PhotonView))]
    public class NetworkManager : MonoBehaviour {
        const string VERSION = "v0.0.1";
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
                maxPlayers = 8
            };
            PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);
        }


        void OnJoinedRoom()
        {

            Debug.Log("Spawning new poker player..");
            GameObject player = PhotonNetwork.Instantiate(prefabName,
                gameMaster.playerPositions[PhotonNetwork.player.ID - 1].position,
                gameMaster.playerPositions[PhotonNetwork.player.ID - 1].rotation,
                0);
                
            player.GetComponent<Renderer>().material = gameMaster.mats[PhotonNetwork.player.ID - 1];
            player.GetComponent<Player>().enabled = true;
            gameMaster.photonView.RPC("AddPlayer", PhotonTargets.AllBuffered);
            player.GetComponent<Player>().photonView.RPC("InitColour", PhotonTargets.AllBuffered, PhotonNetwork.player.ID);

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
                GameObject.Find("Canvas").transform.FindChild("btnStart").gameObject.SetActive(true);
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

