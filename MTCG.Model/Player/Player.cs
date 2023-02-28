using MTCG.Model.Cards;

namespace MTCG.Model.Player
{
    public class Player
    {
        public List<Card> Deck
        {
            get; private set;

        }

        public string Playername { get; private set; }

        // constructor
        public Player(List<Card> playerDeck, string playername)
        {
            Deck = playerDeck;
            Playername = playername;
        }


        // Add cards to playerdeck
        public void AddToPlayerDeck(Card card)
        {
            if (card != null)
            {
                Deck.Add(card);
            }
            else
            {
                Console.WriteLine("Card could not be added to deck.");
            }
        }

        // Remove cards from playerdeck
        public void RemoveFromPlayerDeck(Card card)
        {
            if (Deck.Contains(card))
            {
                Deck.Remove(card);
            }
            else
            {
                Console.WriteLine("Given card could not be removed from deck.");
            }
        }


    }
}
