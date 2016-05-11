using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Poker
{
    public class Player : MonoBehaviour {
        public List<Poker.Card> hand;

        public int wallet = 0;
        public int id = 0;

        // Events for turn action handling
        public delegate void IsBetting(int playerId);
        public static event IsBetting OnClicked;

        public void SendBetting()
        {
            if (OnClicked != null)
            {
                OnClicked(id);
            }
        }

	    // Use this for initialization
	    void Start () {
            hand = new List<Poker.Card>();
	    }
	
	    // Update is called once per frame
	    void Update () {
	    
	    }
    }
}

