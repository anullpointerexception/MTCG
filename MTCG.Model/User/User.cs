using MTCG.Model.Cards;

namespace MTCG.Model.User
{
    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; }

        public List<Card> Stack { get; set; }
        public List<Card> Deck { get; set; }

        public int Coins { get; set; }

        public User(string username, string password)
        {
            Username = username;
            Password = password;
            Stack = new List<Card>();
            Deck = new List<Card>();
            Coins = 20;
        }

        public void ManageCards()
        {

        }

        public void BuyPackages()
        {

        }

    }
}
