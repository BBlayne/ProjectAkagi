using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Poker
{
    public class Player : Photon.MonoBehaviour {
        public List<Poker.Card> hand = new List<Card>();

        public int wallet = 0;      
        public static int id;

        // Events for turn action handling
        public delegate void IsBetting(int playerId);
        public static event IsBetting OnClicked;
        public GameController gameMaster = null;

        public void SendBetting()
        {
            if (OnClicked != null)
            {
                OnClicked(id);
            }
        }

        [PunRPC]
        void SetId(int _id)
        {            
            id = _id;
        }

        [PunRPC]
        void InitColour(int id)
        {
            Debug.Log("InitColour " + id);
            //this.enabled = true;
            if (PhotonNetwork.player.ID == id)
            {
                //this.GetComponent<Renderer>().material = gameMaster.mats[id - 1];
            }
            else
            {

            }

            this.GetComponent<Renderer>().material = gameMaster.mats[id - 1];
        }

        void Awake()
        {
            Debug.Log("Awake");
            if (gameMaster == null)
            {
                gameMaster = GameObject.Find("GameController").GetComponent<GameController>();
            }
        }

        // Use this for initialization
        void Start () {
            if (gameMaster == null)
            {
                GameObject.Find("GameController").GetComponent<GameController>();
            }
	    }
	
	    // Update is called once per frame
	    void Update () {

        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {

        }
    }
}

