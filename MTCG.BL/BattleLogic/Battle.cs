using MTCG.Model.Cards;
using static MTCG.Model.Cards.Card;

namespace MTCG.BL.BattleLogic
{
    public class Battle
    {
        private const int MaxRounds = 100;
        private List<Card> player1Deck;
        private List<Card> player2Deck;

        public Battle(List<Card> player1Deck, List<Card> player2Deck)
        {
            this.player1Deck = player1Deck;
            this.player2Deck = player2Deck;
        }


        public List<string> Start()
        {
            var log = new List<string>();
            var roundCount = 1;

            while (player1Deck.Count > 0 && player2Deck.Count > 0 && roundCount <= MaxRounds)
            {
                var player1Card = ChooseRandomCard(player1Deck);
                var player2Card = ChooseRandomCard(player2Deck);

                log.Add($"Round {roundCount}: {player1Card.Name} vs {player2Card.Name}");

                if (player1Card.cardType == Card.CardType.Monster && player2Card.cardType == Card.CardType.Monster)
                {
                    // pure monster fight, no element type effect
                    var winner = GetWinner(player1Card, player2Card);
                    if (winner == player1Card)
                    {
                        player2Deck.Remove(player2Card);
                        player1Deck.Add(player2Card);
                        log.Add($"Winner: {player1Card.Name}");
                    }
                    else if (winner == player2Card)
                    {
                        player1Deck.Remove(player1Card);
                        player2Deck.Add(player1Card);
                        log.Add($"Winner: {player2Card.Name}");
                    }
                    else
                    {
                        log.Add("Draw");
                    }

                }
                else
                {
                    // At least one spell card, element type effect applies
                    var player1Effective = GetElementEffectiveness(player1Card.elementType, player2Card.elementType);

                    var player2Effective = GetElementEffectiveness(player2Card.elementType, player1Card.elementType);

                    if (player1Card.cardType == Card.CardType.Monster && player2Card.cardType == Card.CardType.Spell)
                    {
                        var winner = GetWinnerWithSpell(player2Card, player1Card);
                        if (winner == player2Card)
                        {
                            player1Deck.Remove(player1Card);
                            player2Deck.Add(player1Card);
                        }
                        else
                        {
                            player2Deck.Remove(player2Card);
                            player1Deck.Add(player2Card);
                        }

                        log.Add($"Winner: {winner.Name}");
                    }
                    else if (player1Card.cardType == Card.CardType.Spell && player2Card.cardType == Card.CardType.Monster)
                    {
                        var winner = GetWinnerWithSpell(player1Card, player2Card);
                        if (winner == player1Card)
                        {
                            player2Deck.Remove(player2Card);
                            player1Deck.Add(player2Card);

                        }
                        else
                        {
                            player1Deck.Remove(player1Card);
                            player2Deck.Add(player1Card);
                        }

                        log.Add($"Winner: {winner.Name}");
                    }
                    else
                    {

                        var winner = GetWinnerWithSpell(player2Card, player1Card);
                        if (winner == player1Card)
                        {
                            player2Deck.Remove(player2Card);
                            player1Deck.Add(player2Card);

                        }
                        else if (winner == null)
                        {
                            log.Add("Draw");

                        }
                        else
                        {
                            player1Deck.Remove(player1Card);
                            player2Deck.Add(player1Card);
                        }

                        log.Add($"Winner: {winner.Name}");
                    }

                }
                roundCount++;
            }

            return log;
        }


        public Card ChooseRandomCard(List<Card> deck)
        {
            var randomIndex = new Random().Next(deck.Count);
            return deck[randomIndex];
        }


        public double GetElementEffectiveness(ElementType attackType, ElementType defenseType)
        {
            if ((attackType == ElementType.Water && defenseType == ElementType.Fire) || (attackType == ElementType.Fire && defenseType == ElementType.Normal) || (attackType == ElementType.Normal && defenseType == ElementType.Water))
            {
                return 2.0;
            }
            else if ((attackType == ElementType.Fire && defenseType == ElementType.Water) || (attackType == ElementType.Normal && defenseType == ElementType.Fire) || (attackType == ElementType.Water && defenseType == ElementType.Normal))
            {
                return 0.5;
            }
            else
            {
                return 1.0;
            }
        }

        public Card GetWinnerWithSpell(Card attackCard, Card defendCard)
        {
            var damageAttacker = attackCard.Damage * GetElementEffectiveness(attackCard.elementType, defendCard.elementType);
            var damageDefender = defendCard.Damage * GetElementEffectiveness(defendCard.elementType, attackCard.elementType);

            if (damageAttacker > damageDefender)
            {
                return attackCard;
            }
            else if (damageAttacker == damageDefender)
            {
                return null;
            }
            else
            {
                return defendCard;
            }

        }

        public Card GetWinner(Card card1, Card card2)
        {
            var card1Damage = card1.Damage;
            var card2Damage = card2.Damage;

            return card1Damage >= card2Damage ? card1 : card2;
        }

    }
}
