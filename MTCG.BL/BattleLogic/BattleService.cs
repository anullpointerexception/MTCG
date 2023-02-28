using MTCG.DAL;
using MTCG.Model.Player;

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
    }
}
