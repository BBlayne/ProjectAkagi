using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(Poker.NetworkManager))]
public class GameController : Photon.MonoBehaviour {
    List<Poker.Card> deck = new List<Poker.Card>();
    public List<Poker.Player> players = new List<Poker.Player>();
    List<Poker.Card> pool = new List<Poker.Card>();
    public Material[] mats;

    public int dealerID = 0;        
    public int currentPlayerID;

    public bool setCurrentPlayerManually = false;

    public GameObject dealer = null;

    public int playerCount = 0;
    int pot = 0;
    public int turnCount = 0;

    // Round Variables
    public bool hasBet = false;

    // Transforms
    public List<Transform> playerPositions = new List<Transform>();
    public List<Transform> handPositions = new List<Transform>();
    public List<Transform> flopPositions = new List<Transform>();

    public Sprite[] cardSprites;
    public GameObject card;

    string mString = "";

    Poker.NetworkManager netManager;

    // Game information
    public int minimumBindAmount = 1;
    // Round to round game state information
    int lastAmountBetOrRaised = 0;

    public bool hasDealt = false;

    // Use this for initialization
    void Start () {

        netManager = GetComponent<Poker.NetworkManager>();
        //this.photonView.RPC("InitDeck", PhotonTargets.All);
        //InitDeck();
        //Random.seed = System.DateTime.Now.Millisecond;

        // Probably better to just make the deck locally and then sync results?
        //this.photonView.RPC("Shuffle", PhotonTargets.All, deck, Random.seed);
        //Shuffle(deck);
        //DealOpeningHand();
        //DealFlop();
    }

    [PunRPC]
    public void CallHandler()
    {
        if (PhotonNetwork.isMasterClient)
        {
            Debug.Log("CallHandler");
            OnEndTurn();
        }
    }

    [PunRPC]
    public void RaiseHandler()
    {
        if (PhotonNetwork.isMasterClient)
        {
            Debug.Log("RaiseHandler");
            OnEndTurn();
        }
    }

    [PunRPC]
    public void AllInHandler()
    {
        if (PhotonNetwork.isMasterClient)
        {
            Debug.Log("AllInHandler");
            OnEndTurn();
        }
    }

    void ChooseDealer()
    {
        // Dealer is chosen randomly, big and small blind are picked after wards.
        Random.seed = System.DateTime.Now.Millisecond;
        int _dealerID = Random.Range(1, PhotonNetwork.playerList.Length + 1);
        Debug.Log("dealer id, " + _dealerID + ", players #:" + PhotonNetwork.playerList.Length);
        _dealerID = _dealerID % 6;
        Debug.Log("Chose player" + _dealerID + ", as the dealer.");
        this.photonView.RPC("SetDealer", PhotonTargets.All, _dealerID);

    }

    [PunRPC]
    void SetDealer(int _dealerID)
    {
        dealerID = _dealerID;
        // assume ID is known.
        int _slotID = (_dealerID - (PhotonNetwork.player.ID - 1)) % 6;
        // Raaaaaaging
        if (_slotID < 0)
            _slotID += 6; // max number of players.

        GameObject.Find("PlayerSlotPositions").transform.Find("slot" + _slotID).Find("ButtonPos").Find("DealerButton").gameObject.SetActive(true);

        // If we have at least 3 players, assign current player as the big blind.
        if (PhotonNetwork.playerList.Length > 2)
        {
            currentPlayerID = ((dealerID + (PhotonNetwork.playerList.Length - 1)) % PhotonNetwork.playerList.Length);
            if (currentPlayerID == 0)
                currentPlayerID = PhotonNetwork.playerList.Length;
        }            
        else
        {
            if (!setCurrentPlayerManually)
            {
                // If we're only two players just make current player the non dealer.
                currentPlayerID = (dealerID % PhotonNetwork.playerList.Length) + 1;
            }

        }

        Debug.Log("Current player is Player" + currentPlayerID);
            
    }

    void PromptPlayer()
    {
        // RPC Call to current player
    }

    [PunRPC]
    public void InitSeed()
    {
        Random.seed = System.DateTime.Now.Millisecond;
    }



    [PunRPC]
    void InitDeck()
    {
        Debug.Log("Initializing Deck...");
        deck = new List<Poker.Card>();
        for (int k = 0; k < 4; k++)
        {
            Poker.Colour myColor;
            if (k % 2 == 0)
                myColor = Poker.Colour.red;
            else
                myColor = Poker.Colour.black;

            for (int i = 1; i < 13; i++)
            {
                Poker.Card card = new Poker.Card((Poker.Suit)k, (Poker.Rank)i, myColor);
                deck.Add(card);
            }
        }
    }

    void InitPlayers()
    {

    }
	
	// Update is called once per frame
	void LateUpdate () {
	    if (Input.touchCount > 0 || Input.GetMouseButtonDown(0))
        {
            //PickupFlop();            
            //Redeal();
            //DealFlop();
        }
	}

    [PunRPC]
    public void initGame()
    {
        //this.photonView.RPC("InitDeck", PhotonTargets.All);
        //InitDeck();
        //Random.seed = System.DateTime.Now.Millisecond;
        //this.photonView.RPC("InitSeed", PhotonTargets.All);
        // Probably better to just make the deck locally and then sync results?
        //this.photonView.RPC("Shuffle", PhotonTargets.All, deck, Random.seed);
        //Shuffle(deck);
        //DealOpeningHand();
        //DealFlop();
        // Randomly determine first player, player "left" of big blind.
        //DetermineTurnOrder();

        Debug.Log("Initializing game state...");
        // Update random seed so it's different each time we start a new game.
        Random.seed = System.DateTime.Now.Millisecond;
        InitDeck();
        Shuffle(deck, Random.seed);
        ChooseDealer();
    }

    void DealOpeningHand(Poker.Player _player)
    {
        if (PhotonNetwork.player.ID == 1)
        {
            if (deck.Count > 0)
            {
                // Grab two cards from the deck
                Poker.Card card0 = Deal(deck);
                Vector3 serializableCard0 = new Vector3((int)card0.suit, (int)card0.rank, (int)card0.color);
                Poker.Card card1 = Deal(deck);
                Vector3 serializableCard1 = new Vector3((int)card1.suit, (int)card1.rank, (int)card1.color);

                _player.photonView.RPC("DealHand", PhotonTargets.All, serializableCard0, serializableCard1);
            }
            else
            {
                Debug.Log("Deck is empty?");
                Debug.Break();
            }
        }
        else
        {

        }

    }

    void DealFlop(int _num)
    {
        for (int i = 0; i < _num; i++)
        {
            Poker.Card card = Deal(deck);
            Vector3 serializableCard = new Vector3((int)card.suit, (int)card.rank, (int)card.color);

            // RPC Call, Sync to other players
            this.photonView.RPC("DealFlopRPC", PhotonTargets.Others, serializableCard);

            // Add to local pool
            pool.Add(card);
        }
        UpdateFlop();
    }

    [PunRPC]
    void DealFlopRPC(Vector3 _card)
    {
        // Add to local pool from serialized card
        Poker.Card card = new Poker.Card((Poker.Suit)_card.x, (Poker.Rank)_card.y, (Poker.Colour)_card.z);
        pool.Add(card);

        // Update local flop
        UpdateFlop();
    }


    void RoundManager()
    {

        // Each player takes a turn whether to Check / Call // Bet / Raise.

        // Deal three cards (Flop)

        // Each player takes a turn whether to Check / Call / Bet / Raise.

        // Deal one card (The Turn)

        // Each player takes a turn whether to Check / Call / Bet / Raise.

        // Deal last card (The River)

        // Final Betting Round

        // Showdown
    }

    [PunRPC]
    void Shuffle(List<Poker.Card> _deck, int seed)
    {
        Debug.Log("Shuffling the deck...");
        Random.seed = seed;
        // Shuffle needs to be changed when working for
        // real production deployment due to security concerns!
        for (int i = 0; i < _deck.Count; i++)
        {
            Poker.Card temp = _deck[i];
            int randomIndex = Random.Range(i, _deck.Count);
            _deck[i] = _deck[randomIndex];
            _deck[randomIndex] = temp;
        }
    }

    Poker.Card Deal(List<Poker.Card> _deck)
    {
        Poker.Card card = _deck[0];
        _deck.RemoveAt(0);

        return card; 
    }

    void PickupFlop()
    {
        foreach (Poker.Card card in pool)
        {
            deck.Add(card);
        }
        pool.Clear();
    }

    void Redeal()
    {
        /*
        foreach (Poker.Player player in players)
        {
            foreach (Poker.Card card in player.hand)
            {
                deck.Add(card);
            }

            player.hand.Clear();
            handPositions[player.id].FindChild("Card0").gameObject.SetActive(false);
            handPositions[player.id].FindChild("Card1").gameObject.SetActive(false);
        }

        Shuffle(deck, Random.seed);

        // All players draw initial drawing hand (two cards)
        foreach (Poker.Player player in players)
        {
            // deal two cards
            player.hand.Add(Deal(deck));
            player.hand.Add(Deal(deck));
            UpdatePlayerHand(player.id);
        }
        */

    }

    void UpdatePlayerHand(int player)
    {
        /*
        handPositions[player].FindChild("Card0").gameObject.SetActive(true);
        handPositions[player].FindChild("Card0").gameObject.GetComponent<SpriteRenderer>().sprite = 
            cardSprites[((int)players[player].hand[0].suit * 13) + (int)players[player].hand[0].rank - 1];

        handPositions[player].FindChild("Card1").gameObject.SetActive(true);
        handPositions[player].FindChild("Card1").gameObject.GetComponent<SpriteRenderer>().sprite =
            cardSprites[((int)players[player].hand[1].suit * 13) + (int)players[player].hand[1].rank - 1];
            */
    }

    void UpdateFlop()
    {
        //for (int i = 0; i < pool.Count; i++)
        //{
        int counter = 0;
        foreach (Poker.Card _card in pool)
        {
            GameObject mCard = null;
            try
            {
                mCard = flopPositions[counter].Find("PlayingCard").Find("Card").gameObject;
            }
            catch (System.Exception e)
            {
                Debug.Log("error: " + e.Message + ", " + counter);
                Debug.Break();
            }
            
            // initialize card texture
            Sprite sprite = this.cardSprites[((int)_card.suit * 13) + (int)_card.rank - 1];
            Texture2D _tex = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);

            _tex.SetPixels(sprite.texture.GetPixels((int)sprite.textureRect.x,
                                        (int)sprite.textureRect.y,
                                        (int)sprite.textureRect.width,
                                        (int)sprite.textureRect.height));
            _tex.Apply();
            mCard.GetComponent<Renderer>().materials[0].mainTexture = _tex;
            mCard.GetComponent<Renderer>().materials[0].SetTexture("_EmissionMap", _tex);
            flopPositions[counter].gameObject.SetActive(true);
            counter++;
        }

        //}
    }

    [PunRPC]
    public void Turn()
    {
        if (PhotonNetwork.isMasterClient)
        {
            if (!hasDealt)
            {
                switch (turnCount)
                {
                    case 1:
                        // The Flop
                        DealFlop(3);
                        break;
                    case 2:
                        // The Turn
                        DealFlop(1);
                        break;
                    case 3:
                        // The River
                        DealFlop(1);
                        break;
                    default:
                        break;
                }

                hasDealt = true;
            }


            players[currentPlayerID - 1].photonView.RPC("startTurn", PhotonTargets.All);
        }
    }

    void ClearHighlights()
    {
        foreach (Poker.Player _player in players)
        {
            _player.photonView.RPC("ClearTurnHighlight", PhotonTargets.All);
        }
        
    }

    public void OnEndTurn()
    {
        players[currentPlayerID - 1].hasPlayed = true;
        players[currentPlayerID - 1].photonView.RPC("toggleHasPlayed", PhotonTargets.All, true);        
        ClearHighlights();
        Next();
        Turn();
    }

    [PunRPC]
    public void Next()
    {
        if (PhotonNetwork.isMasterClient)
        {

            Debug.Log("Is master Client.");
            currentPlayerID = (currentPlayerID + 1) % PhotonNetwork.playerList.Length;
            if (currentPlayerID == 0)
                currentPlayerID = PhotonNetwork.playerList.Length;

            // aka before they start their turn
            if (players[currentPlayerID - 1].hasPlayed)
            {
                foreach (Poker.Player _player in players)
                {
                    _player.photonView.RPC("toggleHasPlayed", PhotonTargets.All, false);
                }

                turnCount++;
                hasDealt = false;
            }
        }
    }

    [PunRPC]
    public void AddPlayer()
    {
        playerCount++;
    }

    public void AddAllPlayers()
    {
        initGame();
        GameObject.Find("Canvas").transform.FindChild("btnStart").gameObject.SetActive(false);
        Debug.Log("Add all players...");
        Debug.Log("Poker.Player Count: " + PhotonNetwork.FindGameObjectsWithComponent(typeof(Poker.Player)).Count);
        this.photonView.RPC("AddAllPlayersSynced", PhotonTargets.All);
    }

    [PunRPC]
    public void AddAllPlayersSynced()
    {
        List<int> playerIDs = new List<int>();
        foreach (GameObject _player in PhotonNetwork.FindGameObjectsWithComponent(typeof(Poker.Player)))
        {
            playerIDs.Add(_player.GetComponent<Poker.Player>().id);
        }        

        foreach (GameObject _player in PhotonNetwork.FindGameObjectsWithComponent(typeof(Poker.Player)))
        {
            players.Add(_player.GetComponent<Poker.Player>());
            DealOpeningHand(_player.GetComponent<Poker.Player>());
            _player.GetComponent<Poker.Player>().photonView.RPC("DealHandTest", PhotonTargets.All, playerIDs.ToArray());        
        }

        List<Poker.Player> temp = players.OrderBy(go=>go.id).ToList();
        players = temp;

        Turn();
    }

    public void AddPlayer(Poker.Player _player)
    {
        this.players.Add(_player);
    }

    void OnGUI()
    {
        GUI.Label(new Rect(0, 120, 200, 200), "" + mString);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

    }
}
