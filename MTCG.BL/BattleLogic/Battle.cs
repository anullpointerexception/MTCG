using MTCG.Model.Cards;
using MTCG.Model.Player;
using static MTCG.Model.Cards.Card;

namespace MTCG.BL.BattleLogic
{
    public class Battle
    {
        private const int MaxRounds = 100;
        // private List<Card> player1Deck;
        // private List<Card> player2Deck;

        public List<Player> CurrentPlayers { get; set; }

        public bool IsPlaying { get; set; }
        public bool Finished { get; set; }

        public string? Winner { get; set; }

        public string? Loser { get; set; }

        public Battle()
        {
            CurrentPlayers = new List<Player>(2); // set to 2 since 2 players is max size
            IsPlaying = false;
            Finished = false;


        }

        public List<string> Log { get; private set; }

        public void AddPlayertoBattle(Player player)
        {
            if (player == null)
            {
                Console.WriteLine("Given player not valid!");
            }
            else if (CurrentPlayers.Count < 2)
            {
                CurrentPlayers.Add(player);
            }
            else
            {
                Console.WriteLine("Battle has already 2 Players.");
            }
        }



        public void Start()
        {
            // var log = new List<string>();
            var roundCount = 1;
            IsPlaying = true;

            while (CurrentPlayers[0].Deck.Count > 0 && CurrentPlayers[1].Deck.Count > 0 && roundCount <= MaxRounds)
            {
                var player1Card = ChooseRandomCard(CurrentPlayers[0].Deck);
                var player2Card = ChooseRandomCard(CurrentPlayers[1].Deck);

                Log.Add($"Round {roundCount}: {player1Card.Name} vs {player2Card.Name}");

                if (player1Card.cardType == Card.CardType.Monster && player2Card.cardType == Card.CardType.Monster)
                {
                    // pure monster fight, no element type effect
                    var winner = GetWinner(player1Card, player2Card);
                    if (winner == player1Card)
                    {
                        CurrentPlayers[1].Deck.Remove(player2Card);
                        CurrentPlayers[0].Deck.Add(player2Card);
                    }
                    else if (winner == player2Card)
                    {
                        CurrentPlayers[0].Deck.Remove(player1Card);
                        CurrentPlayers[1].Deck.Add(player1Card);
                    }
                    else
                    {
                    }

                }
                else
                {

                    var winner = GetWinnerWithSpell(player2Card, player1Card);
                    if (winner == player1Card)
                    {
                        CurrentPlayers[1].Deck.Remove(player2Card);
                        CurrentPlayers[0].Deck.Add(player2Card);
                    }
                    else if (winner == null)
                    {

                    }
                    else
                    {
                        CurrentPlayers[0].Deck.Remove(player1Card);
                        CurrentPlayers[1].Deck.Add(player1Card);
                    }


                }
                // Log.Add($"Kartenanzahl: Player1: {CurrentPlayers[0].Deck.Count} Player2: {CurrentPlayers[1].Deck.Count}");
                roundCount++;
            }

            Log.Add("Battle has ended.");
            Finished = true;
            if (CurrentPlayers[0].Deck.Count == 0)
            {
                Log.Add($"Player {CurrentPlayers[0].Playername} won!");
                Winner = CurrentPlayers[0].Playername;
                Loser = CurrentPlayers[1].Playername;
            }
            else if (CurrentPlayers[1].Deck.Count == 0)
            {
                Log.Add($"Player {CurrentPlayers[1].Playername} won!");
                Winner = CurrentPlayers[1].Playername;
                Loser = CurrentPlayers[0].Playername;
            }
            else if (roundCount >= MaxRounds)
            {
                Log.Add($"Draw!");
                Winner = null;
                Loser = null;

            }


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
