using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Jetstream;

// copyright notice from jetstream benchmark:

/*
 * Copyright (C) 2017 Apple Inc. All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 *
 * THIS SOFTWARE IS PROVIDED BY APPLE INC. ``AS IS'' AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
 * PURPOSE ARE DISCLAIMED.  IN NO EVENT SHALL APPLE INC. OR
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
 * EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
 * PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY
 * OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
 * OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE. 
 */

public class CardDeck
{
    public static readonly string[] _newDeck = new[] {
        // Spades
        "\x1f0a1", "\x1f0a2",  "\x1f0a3",  "\x1f0a4",  "\x1f0a5",
        "\x1f0a6", "\x1f0a7",  "\x1f0a8",  "\x1f0a9",  "\x1f0aa",
        "\x1f0ab", "\x1f0ad",  "\x1f0ae",
        // Hearts
        "\x1f0b1", "\x1f0b2",  "\x1f0b3",  "\x1f0b4",  "\x1f0b5",
        "\x1f0b6", "\x1f0b7",  "\x1f0b8",  "\x1f0b9",  "\x1f0ba",
        "\x1f0bb", "\x1f0bd",  "\x1f0be",
        // Clubs
        "\x1f0d1", "\x1f0d2",  "\x1f0d3",  "\x1f0d4",  "\x1f0d5",
        "\x1f0d6", "\x1f0d7",  "\x1f0d8",  "\x1f0d9",  "\x1f0da",
        "\x1f0db", "\x1f0dd",  "\x1f0de",
        // Diamonds
        "\x1f0c1", "\x1f0c2",  "\x1f0c3",  "\x1f0c4",  "\x1f0c5",
        "\x1f0c6", "\x1f0c7",  "\x1f0c8",  "\x1f0c9",  "\x1f0ca",
        "\x1f0cb", "\x1f0cd",  "\x1f0ce"
    };

    public static readonly string[] _rankNames = new[] {
        "", "Ace", "2", "3", "4", "5", "6", "7", "8", "9", "10", "Jack", "", "Queen", "King"
    };

    public List<string> _cards;

    public CardDeck ()
    {
        this.newDeck();
    }

    public void newDeck()
    {
        // Make a shallow copy of a new deck
        this._cards = new List<string>(_newDeck);
    }

    public void shuffle()
    {
        this.newDeck();

        for (int index = 52; index != 0;) {
            // Select a random card
            var randomIndex = (int)Math.Floor(Compat.random() * index);
            index--;

            // Swap the current card with the random card
            var tempCard = this._cards[index];
            this._cards[index] = this._cards[randomIndex];
            this._cards[randomIndex] = tempCard;
        }
    }

    public string dealOneCard ()
    {
        return this._cards.shift();
    }

    public static int cardRank (string card)
    {
        // This returns a numeric value for a card.
        // Ace is highest.

        var rankOfCard = card.codePointAt(0) & 0xf;
        if (rankOfCard == 0x1) // Make Aces higher than Kings
            rankOfCard = 0xf;

        return rankOfCard;
    }

    public static string cardName(string card)
    {
        return cardName(card.codePointAt(0));
    }

    public static string cardName(int card)
    {
        return _rankNames[card & 0xf];
    }
}

public class Hand
{
    public static readonly Regex FlushRegExp = new Regex(
            "([\x1f0a1-\x1f0ae]{5})|([\x1f0b1-\x1f0be]{5})|([\x1f0c1-\x1f0ce]{5})|([\x1f0d1-\x1f0de]{5})", 
            RegexOptions.ECMAScript | RegexOptions.Compiled
        ),
        StraightRegExp = new Regex(
            "([\x1f0a1\x1f0b1\x1f0d1\x1f0c1][\x1f0ae\x1f0be\x1f0de\x1f0ce][\x1f0ad\x1f0bd\x1f0dd\x1f0cd][\x1f0ab\x1f0bb\x1f0db\x1f0cb][\x1f0aa\x1f0ba\x1f0da\x1f0ca])|[\x1f0ae\x1f0be\x1f0de\x1f0ce][\x1f0ad\x1f0bd\x1f0dd\x1f0cd][\x1f0ab\x1f0bb\x1f0db\x1f0cb][\x1f0aa\x1f0ba\x1f0da\x1f0ca][\x1f0a9\x1f0b9\x1f0d9\x1f0c9]|[\x1f0ad\x1f0bd\x1f0dd\x1f0cd][\x1f0ab\x1f0bb\x1f0db\x1f0cb][\x1f0aa\x1f0ba\x1f0da\x1f0ca][\x1f0a9\x1f0b9\x1f0d9\x1f0c9][\x1f0a8\x1f0b8\x1f0d8\x1f0c8]|[\x1f0ab\x1f0bb\x1f0db\x1f0cb][\x1f0aa\x1f0ba\x1f0da\x1f0ca][\x1f0a9\x1f0b9\x1f0d9\x1f0c9][\x1f0a8\x1f0b8\x1f0d8\x1f0c8][\x1f0a7\x1f0b7\x1f0d7\x1f0c7]|[\x1f0aa\x1f0ba\x1f0da\x1f0ca][\x1f0a9\x1f0b9\x1f0d9\x1f0c9][\x1f0a8\x1f0b8\x1f0d8\x1f0c8][\x1f0a7\x1f0b7\x1f0d7\x1f0c7][\x1f0a6\x1f0b6\x1f0d6\x1f0c6]|[\x1f0a9\x1f0b9\x1f0d9\x1f0c9][\x1f0a8\x1f0b8\x1f0d8\x1f0c8][\x1f0a7\x1f0b7\x1f0d7\x1f0c7][\x1f0a6\x1f0b6\x1f0d6\x1f0c6][\x1f0a5\x1f0b5\x1f0d5\x1f0c5]|[\x1f0a8\x1f0b8\x1f0d8\x1f0c8][\x1f0a7\x1f0b7\x1f0d7\x1f0c7][\x1f0a6\x1f0b6\x1f0d6\x1f0c6][\x1f0a5\x1f0b5\x1f0d5\x1f0c5][\x1f0a4\x1f0b4\x1f0d4\x1f0c4]|[\x1f0a7\x1f0b7\x1f0d7\x1f0c7][\x1f0a6\x1f0b6\x1f0d6\x1f0c6][\x1f0a5\x1f0b5\x1f0d5\x1f0c5][\x1f0a4\x1f0b4\x1f0d4\x1f0c4][\x1f0a3\x1f0b3\x1f0d3\x1f0c3]|[\x1f0a6\x1f0b6\x1f0d6\x1f0c6][\x1f0a5\x1f0b5\x1f0d5\x1f0c5][\x1f0a4\x1f0b4\x1f0d4\x1f0c4][\x1f0a3\x1f0b3\x1f0d3\x1f0c3][\x1f0a2\x1f0b2\x1f0d2\x1f0c2]|[\x1f0a1\x1f0b1\x1f0d1\x1f0c1][\x1f0a5\x1f0b5\x1f0d5\x1f0c5][\x1f0a4\x1f0b4\x1f0d4\x1f0c4][\x1f0a3\x1f0b3\x1f0d3\x1f0c3][\x1f0a2\x1f0b2\x1f0d2\x1f0c2]", 
            RegexOptions.ECMAScript | RegexOptions.Compiled
        ),
        OfAKindRegExp = new Regex(
            "(?:[\x1f0a1\x1f0b1\x1f0d1\x1f0c1]{2,4})|(?:[\x1f0ae\x1f0be\x1f0de\x1f0ce]{2,4})|(?:[\x1f0ad\x1f0bd\x1f0dd\x1f0cd]{2,4})|(?:[\x1f0ab\x1f0bb\x1f0db\x1f0cb]{2,4})|(?:[\x1f0aa\x1f0ba\x1f0da\x1f0ca]{2,4})|(?:[\x1f0a9\x1f0b9\x1f0d9\x1f0c9]{2,4})|(?:[\x1f0a8\x1f0b8\x1f0d8\x1f0c8]{2,4})|(?:[\x1f0a7\x1f0b7\x1f0d7\x1f0c7]{2,4})|(?:[\x1f0a6\x1f0b6\x1f0d6\x1f0c6]{2,4})|(?:[\x1f0a5\x1f0b5\x1f0d5\x1f0c5]{2,4})|(?:[\x1f0a4\x1f0b4\x1f0d4\x1f0c4]{2,4})|(?:[\x1f0a3\x1f0b3\x1f0d3\x1f0c3]{2,4})|(?:[\x1f0a2\x1f0b2\x1f0d2\x1f0c2]{2,4})", 
            RegexOptions.ECMAScript | RegexOptions.Compiled
        );

    public const int RoyalFlush = 0x900000;
    public const int StraightFlush = 0x800000;
    public const int FourOfAKind = 0x700000;
    public const int FullHouse = 0x600000;
    public const int Flush = 0x500000;
    public const int Straight = 0x400000;
    public const int ThreeOfAKind = 0x300000;
    public const int TwoPair = 0x200000;
    public const int Pair = 0x100000;

    public List<string> _cards = new List<string>();
    public int _rank;

    public Hand () {
        this.clear();
    }

    public void clear()
    {
        this._cards.Clear();
        this._rank = 0;
    }

    public void takeCard(string card)
    {
        this._cards.push(card);
    }

    public void score()
    {
        // Sort highest rank to lowest
        this._cards.Sort((a, b) => {
            return CardDeck.cardRank(b) - CardDeck.cardRank(a);
        });

        var handString = this._cards.join("");

        var flushResult = handString.match(Hand.FlushRegExp);
        var straightResult = handString.match(Hand.StraightRegExp);
        var ofAKindResult = handString.match(Hand.OfAKindRegExp);

        if (flushResult.Success) {
            if (straightResult.Success) {
                if (straightResult.Groups[1].Success)
                    this._rank = Hand.RoyalFlush;
                else
                    this._rank = Hand.StraightFlush;
            } else
                this._rank = Hand.Flush;

            this._rank |= CardDeck.cardRank(this._cards[0]) << 16 | CardDeck.cardRank(this._cards[1]) << 12;
        } else if (straightResult.Success)
            this._rank = Hand.Straight | CardDeck.cardRank(this._cards[0]) << 16 | CardDeck.cardRank(this._cards[1]) << 12;
        else if (ofAKindResult.Success) {
            // When comparing lengths, a matched unicode character has a length of 2.
            // Therefore expected lengths are doubled, e.g a pair will have a match length of 4.
            if (ofAKindResult.Groups[0].Length == 8)
                this._rank = Hand.FourOfAKind | CardDeck.cardRank(this._cards[0]);
            else {
                // Found pair or three of a kind.  Check for two pair or full house.
                var firstOfAKind = ofAKindResult.Groups[0].Value;
                var remainingCardsIndex = handString.IndexOf(firstOfAKind) + firstOfAKind.Length;
                Match secondOfAKindResult = null;
                if (remainingCardsIndex <= 6
                    && (secondOfAKindResult = Hand.OfAKindRegExp.Match(handString.slice(remainingCardsIndex))).Success) {
                    if ((firstOfAKind.Length == 6 && secondOfAKindResult.Groups[0].Length == 4)
                        || (firstOfAKind.Length == 4 && secondOfAKindResult.Groups[0].Length == 6)) {
                        int threeOfAKindCardRank, twoOfAKindCardRank;
                        if (firstOfAKind.Length == 6) {
                            threeOfAKindCardRank = CardDeck.cardRank(firstOfAKind.slice(0,2));
                            twoOfAKindCardRank = CardDeck.cardRank(secondOfAKindResult.Groups[0].Value.slice(0,2));
                        } else {
                            threeOfAKindCardRank = CardDeck.cardRank(secondOfAKindResult.Groups[0].Value.slice(0,2));
                            twoOfAKindCardRank = CardDeck.cardRank(firstOfAKind.slice(0,2));
                        }
                        // Nice typo, jetstream
                        this._rank = Hand.FullHouse | threeOfAKindCardRank << 16 | ((threeOfAKindCardRank < 12) ? 1 : 0) | threeOfAKindCardRank << 8 | twoOfAKindCardRank << 4 | twoOfAKindCardRank;
                    } else if (firstOfAKind.Length == 4 && secondOfAKindResult.Groups[0].Length == 4) {
                        var firstPairCardRank = CardDeck.cardRank(firstOfAKind.slice(0,2));
                        var SecondPairCardRank = CardDeck.cardRank(secondOfAKindResult.Groups[0].Value.slice(0,2));
                        int otherCardRank = 0;
                        // Due to sorting, the other card is at index 0, 4 or 8
                        if (firstOfAKind.codePointAt(0) == handString.codePointAt(0)) {
                            if (secondOfAKindResult.Groups[0].Value.codePointAt(0) == handString.codePointAt(4))
                                otherCardRank = CardDeck.cardRank(handString.slice(8,10));
                            else
                                otherCardRank = CardDeck.cardRank(handString.slice(4,6));
                        } else
                            otherCardRank = CardDeck.cardRank(handString.slice(0,2));

                        this._rank = Hand.TwoPair | firstPairCardRank << 16 | firstPairCardRank << 12 | SecondPairCardRank << 8 | SecondPairCardRank << 4 | otherCardRank;
                    }
                } else {
                    var ofAKindCardRank = CardDeck.cardRank(firstOfAKind.slice(0,2));
                    var otherCardsRank = 0;
                    foreach (var card in this._cards) {
                        var cardRank = CardDeck.cardRank(card);
                        if (cardRank != ofAKindCardRank)
                            otherCardsRank = (otherCardsRank << 4) | cardRank;
                    }

                    if (firstOfAKind.Length == 6)
                        this._rank = Hand.ThreeOfAKind | ofAKindCardRank << 16 | ofAKindCardRank << 12 | ofAKindCardRank << 8 | otherCardsRank;
                    else
                        this._rank = Hand.Pair | ofAKindCardRank << 16 | ofAKindCardRank << 12 | otherCardsRank;
                }
            }
        } else {
            this._rank = 0;
            foreach (var card in _cards) {
                var cardRank = CardDeck.cardRank(card);
                this._rank = (this._rank << 4) | cardRank;
            }
        }
    }

    public int rank {
        get {
            return _rank;
        }
    }

    public override string ToString () {
        return this._cards.join("");
    }
}

public class Player : Hand {
    public string _name;
    public int _wins;
    public int[] _handTypeCounts;

    public Player (string name)
        : base () {
        this._name = name;
        this._wins = 0;
        this._handTypeCounts = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    }

    public void scoreHand() {
        this.score();
        var handType = this.rank >> 20;
        this._handTypeCounts[handType]++;
    }

    public void wonHand() {
        this._wins++;
    }

    public string name {
        get {
            return this._name;
        }
    }

    public string hand {
        get {
            return base.ToString();
        }
    }

    public int wins {
        get {
            return this._wins;
        }
    }

    public int[] handTypeCounts {
        get {
            return this._handTypeCounts;
        }
    }
}

public static class Hands {
    public static void playHands (List<Player> players)
    {
        var cardDeck = new CardDeck();
        var handsPlayed = 0;
        var highestRank = 0;

        do {
            cardDeck.shuffle();

            foreach (var player in players)
                player.clear();

            for (var i = 0; i < 5; i++) {
                foreach (var player in players)
                    player.takeCard(cardDeck.dealOneCard());
            }

            foreach (var player in players)
                player.scoreHand();

            handsPlayed++;

            highestRank = 0;

            foreach (var player in players)
                if (player.rank > highestRank)
                    highestRank = player.rank;

            foreach (var player in players)
                // We count ties as wins for each player.
                if (player.rank == highestRank)
                    player.wonHand();

        } while (handsPlayed < 2000);
    }
}

public class PlayerExpectation
{
    public static readonly string[] _handTypes = new [] {
        "High Cards",
        "Pairs",
        "Two Pairs",
        "Three of a Kinds",
        "Straights",
        "Flushes",
        "Full Houses",
        "Four of a Kinds",
        "Straight Flushes",
        "Royal Flushes"
    };

    public int _wins;
    public int[] _handTypeCounts;

    public PlayerExpectation (int wins, int[] handTypeCounts)
    {
        this._wins = wins;
        this._handTypeCounts = handTypeCounts;
    }

    public void validate (Player player)
    {
        if (player.wins != this._wins)
            throw new Exception("Expected " + player.name + " to have " + this._wins + ", but they have " + player.wins);

        var actualHandTypeCounts = player.handTypeCounts;
        if (this._handTypeCounts.Length != actualHandTypeCounts.Length)
            throw new Exception("Expected " + player.name + " to have " + this._handTypeCounts.Length + " hand types, but they have " + actualHandTypeCounts.Length);

        for (var handTypeIdx = 0; handTypeIdx < this._handTypeCounts.Length; handTypeIdx++) {
            if (this._handTypeCounts[handTypeIdx] != actualHandTypeCounts[handTypeIdx]) {
                throw new Exception("Expected " + player.name + " to have " + this._handTypeCounts[handTypeIdx] + " " + PlayerExpectation._handTypes[handTypeIdx] + " hands, but they have " + actualHandTypeCounts[handTypeIdx]);
            }

        }
    }
}



public partial class Benchmark {
    public const int WarmingIterationCount = 3;
    public const int IterationCount = 10;
    public const int InnerIterationCount = 1;

    public List<Player> _players = new List<Player> {
        new Player("Player 1"),
        new Player("Player 2"),
        new Player("Player 3"),
        new Player("Player 4")
    };

    public static readonly PlayerExpectation[] playerExpectations = new[] {
        new PlayerExpectation(59864, new[] { 120476, 101226, 11359, 5083, 982, 456, 370, 45, 3, 0 }),
        new PlayerExpectation(60020, new[] { 120166, 101440, 11452, 5096, 942, 496, 333, 67, 8, 0 }),
        new PlayerExpectation(60065, new[] { 120262, 101345, 11473, 5093, 941, 472, 335, 76, 3, 0 }),
        new PlayerExpectation(60064, new[] { 120463, 101218, 11445, 5065, 938, 446, 364, 58, 3, 0 })
    };

    public Benchmark () {
    }

    public void runIteration()
    {
        Hands.playHands(this._players);
    }

    public void validate()
    {
        if (this._players.Count != playerExpectations.Length)
            throw new Exception("Expect " + playerExpectations.Length + ", but actually have " + this._players.Count);

        for (var playerIdx = 0; playerIdx < playerExpectations.Length; playerIdx++)
            playerExpectations[playerIdx].validate(this._players[playerIdx]);
    }

    public void reset()
    {
    }
}