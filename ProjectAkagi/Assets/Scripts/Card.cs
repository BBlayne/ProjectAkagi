using UnityEngine;
using System.Collections;

namespace Poker
{
    /// <summary>
    /// A "Card" represents a real world playing card.
    /// It has a Suit, a Color, and a Rank.
    /// </summary>
    public class Card {
        Suit _suit;
        Rank _rank;
        Color _color;

        bool _isFaceDown = true;

        public bool isFaceDown
        {
            get
            {
                return this._isFaceDown;
            }
            set
            {
                this._isFaceDown = value;
            }
        }

        // Setters & Getters
        public Suit suit
        {
            get
            {
                return this._suit;
            }
            set
            {
                this._suit = value;
            }
        }

        public Rank rank
        {
            get
            {
                return this._rank;
            }
            set
            {
                this._rank = value;
            }
        }

        public Color color
        {
            get
            {
                return this._color;
            }
            set
            {
                this._color = value;
            }
        }

        // default constructor, create an invalid card
        public Card()
        {
            this.suit = Suit.NUM_SUITS;
            this.rank = Rank.joker;
            this.color = Color.grey;
        }

        // parameterized constructor
        public Card(Suit mySuitType, Rank myRank, Color myColor)
        {
            this.suit = mySuitType;
            this.rank = myRank;
            this.color = myColor;
        }
    }
}
