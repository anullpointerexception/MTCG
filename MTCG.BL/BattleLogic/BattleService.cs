using MTCG.DAL;
using MTCG.Model.Player;
using MTCG.Model.User;

namespace MTCG.BL.BattleLogic
{
    public class BattleService
    {
        private static BattleService instance;
        private static readonly object objectlock = new object();

        DBHandler dBHandler = DBHandler.Instance;

        public static BattleService Instance
        {
            get { lock (objectlock) { if (instance == null) { instance = new BattleService(); } return instance; } }
        }


        // Battle List
        List<Battle> battleList = new List<Battle>();

        // List current battles
        private Battle? listCurrentBattles()
        {
            foreach (Battle battle in battleList)
            {
                if (battle.CurrentPlayers.Count < 2)
                {
                    return battle;
                }
            }
            return null;
        }

        // Join Lobby
        public List<string>? JoinPlayerLobby(Player player)
        {
            if (player == null)
            {
                return null;
            }

            Battle? battle = listCurrentBattles();

            if (battle == null)
            {
                battleList.Add(new Battle());
                battle = battleList.Last();
            }

            battle.AddPlayertoBattle(player);


            // Search for players
            while (true)
            {
                if (battle.CurrentPlayers.Count == 2)
                {
                    lock (objectlock)
                    {
                        if (!battle.IsPlaying)
                        {
                            battle.Start();
                            battleList.Remove(battle);
                            // TO-DO: update player stats

                            break;
                        }
                    }

                    if (battle.Finished)
                    {
                        break;
                    }
                }

            }
            return battle.Log;
        }

        // Update User Stats
        private void updateUserStats(string? winner, string? loser)
        {
            if (winner == null || loser == null)
            {
                Console.WriteLine("Battle ended in a draw!");
                return;
            }

            AccountStats? winnerStats = dBHandler.getAccountStats(winner);
            AccountStats? loserStats = dBHandler.getAccountStats(loser);

            if (winnerStats != null && loserStats != null)
            {
                int newElo = (int)((double)loserStats.Elo / (double)winnerStats.Elo * 20);
                winnerStats.Wins++;
                winnerStats.Losses++;
                if (newElo > 30)
                {
                    newElo = 30;
                }
                else if (newElo < 10)
                {
                    newElo = 10;
                }
                winnerStats.Elo = winnerStats.Elo + newElo;
                loserStats.Elo = loserStats.Elo - newElo;
                dBHandler.UpdateStats(winnerStats);
                dBHandler.UpdateStats(loserStats);



            }
            else
            {
                Console.WriteLine("Error when updating stats.");
            }

        }
    }
}
