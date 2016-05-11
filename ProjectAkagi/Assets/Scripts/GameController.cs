using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameController : MonoBehaviour {
    List<Poker.Card> deck;
    List<Poker.Card> pool;
    public List<Poker.Player> players = new List<Poker.Player>();

    public int bigBlind = 0;
    public int firstPlayer = 0;
    public Poker.Player currentPlayer;

    public int playerCount = 0;
    int pot = 0;

    // Round Variables
    public bool hasBet = false;

    // Transforms
    public List<Transform> playerPositions = new List<Transform>();
    public List<Transform> handPositions = new List<Transform>();
    public List<Transform> flopPositions = new List<Transform>();

    public Sprite[] cardSprites;
    public GameObject card;

    void OnEnable()
    {
        Poker.Player.OnClicked += OnClicked;
    }


    void OnDisable()
    {
        Poker.Player.OnClicked -= OnClicked;
    }

    void OnClicked(int id)
    {
        Debug.Log(id);
    }

    // Use this for initialization
    void Start () {
        pool = new List<Poker.Card>();
        InitDeck();
        Shuffle(deck);
        DealOpeningHand();
        DealFlop();
    }

    void InitDeck()
    {
        deck = new List<Poker.Card>();
        for (int k = 0; k < 4; k++)
        {
            Color myColor;
            if (k % 2 == 0)
                myColor = Color.red;
            else
                myColor = Color.black;

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
            PickupFlop();            
            Redeal();
            DealFlop();
        }
	}

    void DetermineTurnOrder()
    {
        bigBlind = Random.Range(0, players.Count - 1);
        // Make sure we don't exceed the number of players
        // For now we allow there to be only one player.
        firstPlayer = (bigBlind + 1) % players.Count;
    }

    void initGame()
    {
        // Randomly determine first player, player "left" of big blind.
        DetermineTurnOrder();
    }

    void DealOpeningHand()
    {
        // All players draw initial drawing hand (two cards)
        foreach (Poker.Player player in players)
        {
            // deal two cards
            player.hand.Add(Deal(deck));
            player.hand.Add(Deal(deck));
            UpdatePlayerHand(player.id);
        }
    }

    void DealFlop()
    {
        pool.Add(Deal(deck));
        pool.Add(Deal(deck));
        pool.Add(Deal(deck));
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

    void Shuffle(List<Poker.Card> _deck)
    {
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

        Shuffle(deck);

        // All players draw initial drawing hand (two cards)
        foreach (Poker.Player player in players)
        {
            // deal two cards
            player.hand.Add(Deal(deck));
            player.hand.Add(Deal(deck));
            UpdatePlayerHand(player.id);
        }


    }

    void UpdatePlayerHand(int player)
    {
        handPositions[player].FindChild("Card0").gameObject.SetActive(true);
        handPositions[player].FindChild("Card0").gameObject.GetComponent<SpriteRenderer>().sprite = 
            cardSprites[((int)players[player].hand[0].suit * 13) + (int)players[player].hand[0].rank - 1];

        handPositions[player].FindChild("Card1").gameObject.SetActive(true);
        handPositions[player].FindChild("Card1").gameObject.GetComponent<SpriteRenderer>().sprite =
            cardSprites[((int)players[player].hand[1].suit * 13) + (int)players[player].hand[1].rank - 1];

    }

    void UpdateFlop()
    {
        for (int i = 0; i < pool.Count; i++)
        {
            flopPositions[i].gameObject.SetActive(true);
            flopPositions[i].gameObject.GetComponent<SpriteRenderer>().sprite =
            cardSprites[((int)pool[i].suit * 13) + (int)pool[i].rank - 1];
        }
    }
}
