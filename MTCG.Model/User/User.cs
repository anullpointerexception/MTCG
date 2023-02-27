using MTCG.Model.Cards;

namespace MTCG.Model.User
{
    public class User
    {
        public AccountCredentials AccountCredentials { get; set; }

        public List<Card> Stack { get; set; }
        public List<Card> Deck { get; set; }

        public AccountStats AccountStats { get; set; }

        public int Coins { get; set; }

        public AccountData AccountData { get; set; }

        public User(AccountCredentials accountCredentials, AccountData accountData, AccountStats accountStats)
        {
            AccountCredentials = accountCredentials;
            AccountData = accountData;
            AccountStats = accountStats;
            Stack = new List<Card>();
            Deck = new List<Card>();
            // Coins = 20;
        }

        public User(AccountCredentials accountCredentials)
        {
            this.AccountCredentials = accountCredentials;
            AccountStats = new AccountStats();
            AccountStats.Wins = 0;
            AccountStats.Losses = 0;
            AccountStats.Elo = 0;

            Deck = new List<Card>();
            Stack = new List<Card>();

            AccountData = new AccountData();
            AccountData.Bio = "Bio";
            AccountData.Name = accountCredentials.Username;


        }



        public void ManageCards()
        {

        }

        public void BuyPackages()
        {

        }

    }
}
