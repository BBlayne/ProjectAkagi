using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

[RequireComponent(typeof(Poker.NetworkManager))]
public class GameController : Photon.MonoBehaviour {
    List<Poker.Card> deck = new List<Poker.Card>();
    public List<Poker.Player> players = new List<Poker.Player>();
    List<Poker.Card> pool = new List<Poker.Card>();
    public Material[] mats;
    public Slot[] playerSlots;
    public TextSlot[] playerNameElements;
    public GameObject[] betTextSlots;
    public Cards[] flopCards;
    public Text[] playerWalletTexts;
    public GameObject[] playerWallets;

    public SliderButtons sliderButtons;
    public HighlightsPostEffect mHighlighter;
    public Canvas mCanvas;
    public RectTransform mCanvasRT;
    public Transform FlopPos;
    public GameObject startButton;
    public Text mPotText;

    public GameObject winnerPanel;
    public GameObject winnderText;

    public int dealerID = 0;        
    public int currentPlayerID;

    public bool setCurrentPlayerManually = false;

    public GameObject dealer = null;

    public int playerCount = 0;
    uint pot = 0;
    public int turnCount = 0;

    private int winnerID = -1;

    // Round Variables
    public bool hasLastPlayerBet = false;
    public bool hasLastPlayerCalled = false;

    // Transforms
    public List<Transform> playerPositions = new List<Transform>();
    public List<Transform> handPositions = new List<Transform>();
    public List<Transform> flopPositions = new List<Transform>();

    public Texture2D[] cardTextures;
    public GameObject card;

    string mString = "";
    string winningHand = "";

    Poker.NetworkManager netManager;

    // Game information
    public int minimumBindAmount = 1;
    // Round to round game state information
    int lastAmountBetOrRaised = 0;

    public bool hasDealt = false;
    public bool hasCalled = false;

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
        
        ulong boardMask = HoldemHand.Hand.ParseHand("js ac as 4d 5d");
        string board = "js ac as 4d 5d";

        // Create hand masks
        ulong player1Mask = HoldemHand.Hand.ParseHand("ad jc");
        string player1Hand = "ad jc";
        ulong player2Mask = HoldemHand.Hand.ParseHand("jd kc");
        string pool = "js ac as 4d 5d 6c";

        // Create a hand value for each player
        uint playerHandValue1 = HoldemHand.Hand.Evaluate(boardMask | player1Mask, 7);
        uint playerHandValue2 = HoldemHand.Hand.Evaluate(boardMask | player2Mask, 7);

        Debug.Log("Player1 has: " + HoldemHand.Hand.HandType(playerHandValue1));
        Debug.Log("Player2 has: " + HoldemHand.Hand.HandType(playerHandValue2));
        Debug.Log("Player1 has: " + HoldemHand.Hand.MaskToString(playerHandValue1));

        //ulong totalmask = HoldemHand.Hand.ParseHand("as js ac as 4d 5d 6c");
        //string name = 
        //if (HoldemHand.Hand.BitCount(totalmask) < 5 || HoldemHand.Hand.BitCount(totalmask) > 7)
        //uint hv = HoldemHand.Hand.Evaluate(boardMask);
        ulong hand = HoldemHand.Hand.ParseHand("js ac as 4d 5d 6c ad");
        uint hv = HoldemHand.Hand.Evaluate(hand);
        // Loop through possible 5 card hands
        foreach (ulong hand5 in HoldemHand.Hand.Hands(0UL, ~hand, 5))
        {
            if (HoldemHand.Hand.Evaluate(hand5) == hv)
            {
                Debug.Log(HoldemHand.Hand.MaskToString(hand5));
            }
            else
            {
                Debug.Log("Nope");
            }
                
        }

        /*
        if (playerHandValue1 > playerHandValue2)
        {
            Debug.Log("Player1 wins with: " + HoldemHand.Hand.HandType(playerHandValue1));
        }
        else
        {
            Debug.Log("Player2 wins with: " + HoldemHand.Hand.HandType(playerHandValue2));
            HoldemHand.Hand.HandTypes.
        }
        */
    }

    [PunRPC]
    void UpdatePot(int _pot)
    {
        this.pot = (uint)_pot;
        mPotText.text = _pot.ToString();
    }

    void EndRound()
    {
        winnerID = determineWinner();
        winnerPanel.SetActive(true);
        winnderText.GetComponent<Text>().text = "The winner is player " + winnerID + ", with " + winningHand;

        foreach (Poker.Player _potentialWinner in players)
        {
            if (_potentialWinner.id == winnerID)
            {
                _potentialWinner.photonView.RPC("UpdateWallet", PhotonTargets.All, (int)pot);
            }
        }
        pot = 0;

        this.photonView.RPC("DisplayWinner", PhotonTargets.All, winnerID, winningHand);
    }

    [PunRPC]
    void DisplayWinner(int _winner, string _hand)
    {
        winningHand = _hand;
        winnerID = _winner;
        winnerPanel.SetActive(true);
        winnderText.GetComponent<Text>().text = "The winner is player " + winnerID + ", with " + _hand;
        pot = 0;
    }

    int determineWinner()
    {
        List<KeyValuePair<uint, Poker.Player>> handValues = new List<KeyValuePair<uint, Poker.Player>>();
        Debug.Log("Pool: " + convertHandToString(pool));
        ulong boardMask = HoldemHand.Hand.ParseHand(convertHandToString(pool));
        foreach (Poker.Player _player in players)
        {
            if (!_player.hasFolded)
            {
                Debug.Log("Hand of player" + _player.id + "; " + convertHandToString(_player.hand));
                ulong handMask = HoldemHand.Hand.ParseHand(convertHandToString(_player.hand));
                KeyValuePair<uint, Poker.Player> handValue = new KeyValuePair<uint, Poker.Player>(HoldemHand.Hand.Evaluate(boardMask | handMask, 7), _player);
                handValues.Add(handValue);
            }
        }

        handValues.Sort(Comparer);

        switch (HoldemHand.Hand.HandType(handValues[handValues.Count - 1].Key))
        {
            case 0:
                winningHand = "High Card";
                break;
            case 1:
                winningHand = "Pair";
                break;
            case 2:
                winningHand = "Two Pair";
                break;
            case 3:
                winningHand = "Triple";
                break;
            case 4:
                winningHand = "Straight";
                break;
            case 5:
                winningHand = "Flush";
                break;
            case 6:
                winningHand = "Full House";
                break;
            case 7:
                winningHand = "Four of a Kind";
                break;
            case 8:
                winningHand = "Straight Flush";
                break;
            default:
                winningHand = "ERROR";
                break;
        }

        return handValues[handValues.Count - 1].Value.id;
    }

    string convertHandToString(List<Poker.Card> _hand)
    {
        string hand = "";
        int count = 0;
        foreach (Poker.Card _card in _hand)
        {
            if (count > 0)
            {
                hand = hand + " ";
            }
            switch (_card.rank)
            {
                case Poker.Rank.ace:
                    hand += "a";
                    break;
                case Poker.Rank.two:
                    hand += "2";
                    break;
                case Poker.Rank.three:
                    hand += "3";
                    break;
                case Poker.Rank.four:
                    hand += "4";
                    break;
                case Poker.Rank.five:
                    hand += "5";
                    break;
                case Poker.Rank.six:
                    hand += "6";
                    break;
                case Poker.Rank.seven:
                    hand += "7";
                    break;
                case Poker.Rank.eight:
                    hand += "8";
                    break;
                case Poker.Rank.nine:
                    hand += "9";
                    break;
                case Poker.Rank.ten:
                    hand += "10";
                    break;
                case Poker.Rank.jack:
                    hand += "j";
                    break;
                case Poker.Rank.king:
                    hand += "k";
                    break;
                case Poker.Rank.queen:
                    hand += "q";                    
                    break;
            }

            switch (_card.suit)
            {
                case Poker.Suit.spades:
                    hand = hand + "s";
                    break;
                case Poker.Suit.hearts:
                    hand = hand + "h";
                    break;
                case Poker.Suit.diamonds:
                    hand = hand + "d";
                    break;
                case Poker.Suit.clubs:
                    hand = hand + "c";
                    break;
            }

            count++;
        }

        return hand;
    }

    [PunRPC]
    void HasRaisedOrBet(int _idToSkip)
    {
        foreach (Poker.Player _player in players)
        {
            if (_player.id == _idToSkip)
                continue;

            _player.hasPlayed = false;
            _player.photonView.RPC("setHasCalled", PhotonTargets.All, false);
            _player.photonView.RPC("setHasBet", PhotonTargets.All, false);
        }
    }

    [PunRPC]
    public void CallHandler()
    {
        if (PhotonNetwork.isMasterClient)
        {
            Debug.Log("CallHandler");
            pot += (uint)minimumBindAmount;
            hasLastPlayerCalled = true;
            OnEndTurn();
        }
    }

    [PunRPC]
    public void CheckHandler()
    {
        if (PhotonNetwork.isMasterClient)
        {
            Debug.Log("CheckHandler");
            OnEndTurn();
        }
    }

    [PunRPC]
    public void BetHandler()
    {
        if (PhotonNetwork.isMasterClient)
        {
            Debug.Log("BetHandler");
            hasLastPlayerBet = true;
            pot += (uint)minimumBindAmount;
            //this.photonView.RPC("HasRaisedOrBet", Photon)
            HasRaisedOrBet(currentPlayerID);
            OnEndTurn();
        }
    }

    [PunRPC]
    public void RaiseHandler(int _amount)
    {
        if (PhotonNetwork.isMasterClient)
        {
            Debug.Log("RaiseHandler");
            minimumBindAmount += _amount;
            pot += (uint)_amount;
            hasLastPlayerBet = true;
            HasRaisedOrBet(currentPlayerID);
            OnEndTurn();
        }
    }

    [PunRPC]
    public void AllInHandler(int _amount)
    {
        if (PhotonNetwork.isMasterClient)
        {
            Debug.Log("AllInHandler");
            minimumBindAmount += _amount;
            pot += (uint)_amount;
            hasLastPlayerBet = true;
            HasRaisedOrBet(currentPlayerID);
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

        //GameObject.Find("PlayerSlotPositions").transform.Find("slot" + _slotID).Find("ButtonPos").Find("DealerButton").gameObject.SetActive(true);
        this.playerSlots[_slotID].dealerButton.gameObject.SetActive(true);

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

    static int Comparer(KeyValuePair<uint, Poker.Player> pair1, KeyValuePair<uint, Poker.Player> pair2)
    {
        return pair1.Key.CompareTo(pair2.Key);
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
                if (_player.hand.Count < 2)
                {
                    _player.hand.Add(card0);
                    _player.hand.Add(card1);
                }

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

    }

    void UpdatePlayerHand(int player)
    {

    }

    void UpdateFlop()
    {
        int counter = 0;
        foreach (Poker.Card _card in pool)
        {
            GameObject mCard = this.flopCards[counter].mCard;

            // initialize card texture
            Texture2D _tex = this.cardTextures[((int)_card.suit * 13) + (int)_card.rank - 1];

            mCard.GetComponent<Renderer>().materials[0].mainTexture = _tex;
            mCard.GetComponent<Renderer>().materials[0].SetTexture("_EmissionMap", _tex);
            flopPositions[counter].gameObject.SetActive(true);
            counter++;
        }
    }

    [PunRPC]
    public void Turn()
    {
        
        if (PhotonNetwork.isMasterClient)
        {
            bool everyCalled = true;
            foreach (Poker.Player _player in players)
            {
                if (!_player.hasCalled)
                {
                    everyCalled = false;
                }
            }

            Debug.Log("Turn: {everyCalled}: " + everyCalled + ", {hasDealt}: " + hasDealt);

            if (!hasDealt && everyCalled)
            {
                hasLastPlayerBet = false;
                hasLastPlayerCalled = false;
                foreach (Poker.Player _player in players)
                {
                    _player.photonView.RPC("setHasCalled", PhotonTargets.All, false);
                    _player.photonView.RPC("setHasBet", PhotonTargets.All, false);
                }

                switch (turnCount)
                {
                    case 0:
                        // Posting binds, first player either keeps minimum bet, or raises.
                        hasLastPlayerBet = true;
                        break;
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
                    case 4:
                        // Showdown
                        EndRound();
                        break;
                    default:
                        break;
                }

                hasDealt = true;
            }

            this.photonView.RPC("UpdatePot", PhotonTargets.All, (int)pot);
            players[currentPlayerID - 1].photonView.RPC("startTurn", PhotonTargets.All, minimumBindAmount, (hasLastPlayerBet || hasLastPlayerCalled));
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
                Debug.Log("Turn Counter: " + turnCount);
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
        //mCanvas.transform.FindChild("btnStart").gameObject.SetActive(false);
        startButton.SetActive(false);
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
            Debug.Log("Hand Count: " + _player.GetComponent<Poker.Player>().hand.Count);
            _player.GetComponent<Poker.Player>().photonView.RPC("DealHandTest", PhotonTargets.All, playerIDs.ToArray());        
        }

        List<Poker.Player> temp = players.OrderBy(go=>go.id).ToList();
        players = temp;
        players[currentPlayerID - 1].photonView.RPC("PostBlind", PhotonTargets.All);
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
