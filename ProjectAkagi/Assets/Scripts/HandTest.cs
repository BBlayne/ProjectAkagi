using UnityEngine;
using System.Collections;
using System;
using HoldemHand;

public class HandTest : MonoBehaviour {

	// Use this for initialization
	void Start () {
        ulong playerMask = Hand.ParseHand("as ks"); // Player Pocket Cards
        ulong board = Hand.ParseHand("Ts Qs 2d");   // Partial Board
        // Calculate values for each hand type
        double[] playerWins = { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 };
        double[] opponentWins = { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 };
        // Count of total hands examined.
        long count = 0;

        // Iterate through all possible opponent hands
        foreach (ulong opponentMask in Hand.Hands(0UL,
                             board | playerMask, 2))
        {
            // Iterate through all possible boards
            foreach (ulong boardMask in Hand.Hands(board,
                           opponentMask | playerMask, 5))
            {
                // Create a hand value for each player
                uint playerHandValue =
                       Hand.Evaluate(boardMask | playerMask, 7);
                uint opponentHandValue =
                       Hand.Evaluate(boardMask | opponentMask, 7);

                // Calculate Winners
                if (playerHandValue > opponentHandValue)
                {
                    // Player Win
                    playerWins[Hand.HandType(playerHandValue)] += 1.0;
                }
                else if (playerHandValue < opponentHandValue)
                {
                    // Opponent Win
                    opponentWins[Hand.HandType(opponentHandValue)] += 1.0;
                }
                else if (playerHandValue == opponentHandValue)
                {
                    // Give half credit for ties.
                    playerWins[Hand.HandType(playerHandValue)] += 0.5;
                    opponentWins[Hand.HandType(opponentHandValue)] += 0.5;
                }
                count++;
            }
        }

        // Print results
        Debug.Log(String.Format("Player Results"));
        Debug.Log(String.Format("High Card:\t{0:0.0}%",
          playerWins[(int)Hand.HandTypes.HighCard] / ((double)count) * 100.0));
        Debug.Log(String.Format("Pair:\t{0:0.0}%",
          playerWins[(int)Hand.HandTypes.Pair] / ((double)count) * 100.0));
        Debug.Log(String.Format("Two Pair:\t{0:0.0}%",
          playerWins[(int)Hand.HandTypes.TwoPair] / ((double)count) * 100.0));
        Debug.Log(String.Format("Three of Kind:\t{0:0.0}%",
          playerWins[(int)Hand.HandTypes.Trips] / ((double)count) * 100.0));
        Debug.Log(String.Format("Straight:\t{0:0.0}%",
          playerWins[(int)Hand.HandTypes.Straight] / ((double)count) * 100.0));
        Debug.Log(String.Format("Flush:\t{0:0.0}%",
          playerWins[(int)Hand.HandTypes.Flush] / ((double)count) * 100.0));
        Debug.Log(String.Format("Fullhouse:\t{0:0.0}%",
          playerWins[(int)Hand.HandTypes.FullHouse] / ((double)count) * 100.0));
        Debug.Log(String.Format("Four of a Kind:\t{0:0.0}%",
          playerWins[(int)Hand.HandTypes.FourOfAKind] / ((double)count) * 100.0));
        Debug.Log(String.Format("Straight Flush:\t{0:0.0}%",
          playerWins[(int)Hand.HandTypes.StraightFlush] / ((double)count) * 100.0));

        Debug.Log(String.Format("Opponent Results"));
        Debug.Log(String.Format("High Card:\t{0:0.0}%",
          opponentWins[(int)Hand.HandTypes.HighCard] / ((double)count) * 100.0));
        Debug.Log(String.Format("Pair:\t{0:0.0}%",
          opponentWins[(int)Hand.HandTypes.Pair] / ((double)count) * 100.0));
        Debug.Log(String.Format("Two Pair:\t{0:0.0}%",
          opponentWins[(int)Hand.HandTypes.TwoPair] / ((double)count) * 100.0));
        Debug.Log(String.Format("Three of Kind:\t{0:0.0}%",
          opponentWins[(int)Hand.HandTypes.Trips] / ((double)count) * 100.0));
        Debug.Log(String.Format("Straight:\t{0:0.0}%",
          opponentWins[(int)Hand.HandTypes.Straight] / ((double)count) * 100.0));
        Debug.Log(String.Format("Flush:\t{0:0.0}%",
          opponentWins[(int)Hand.HandTypes.Flush] / ((double)count) * 100.0));
        Debug.Log(String.Format("Fullhouse:\t{0:0.0}%",
          opponentWins[(int)Hand.HandTypes.FullHouse] / ((double)count) * 100.0));
        Debug.Log(String.Format("Four of a Kind:\t{0:0.0}%",
          opponentWins[(int)Hand.HandTypes.FourOfAKind] / ((double)count) * 100.0));
        Debug.Log(String.Format("Straight Flush:\t{0:0.0}%",
          opponentWins[(int)Hand.HandTypes.StraightFlush] / ((double)count) * 100.0));
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
