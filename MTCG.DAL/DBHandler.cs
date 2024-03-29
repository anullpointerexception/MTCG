﻿using MTCG.Model.Cards;
using MTCG.Model.Deal;
using MTCG.Model.Packages;
using MTCG.Model.User;
using Npgsql;
using static MTCG.Model.Cards.Card;

namespace MTCG.DAL
{
    public class DBHandler
    {
        string connectionString = "Host=localhost:5432;Username=swe1user;Password=swe1pw;Database=mtcg";

        private static DBHandler instance = null;


        private static readonly object objectlock = new object();

        NpgsqlConnection connection;

        public string? UserToken;

        public string? currentUser = null;

        public static DBHandler Instance
        {
            get
            {
                lock (objectlock)
                {
                    if (instance == null)
                    {
                        instance = new DBHandler();
                    }
                    instance.UserToken = null;
                    return instance;
                }
            }
        }
        // CONNECTION SETUP
        public bool SetupConnection()
        {
            try
            {
                connection = new NpgsqlConnection("Host=localhost:5432;Username=swe1user;Password=swe1pw;Database=mtcg");
                connection.Open();
                Console.WriteLine("Database connection established");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }

        }


        // BEGIN: ACCOUNT OPERATIONS //



        public string? Login(string username, string password)
        {
            if (username == null || password == null)
            {
                Console.WriteLine("PW OR UN empty");
                return null;
            }

            lock (objectlock)
            {
                if (connection != null)
                {
                    NpgsqlCommand command = new NpgsqlCommand("SELECT password from accounts WHERE username = @p1;", connection);
                    command.Parameters.Add(new NpgsqlParameter("p1", System.Data.DbType.String));
                    command.Prepare();
                    command.Parameters["p1"].Value = username;

                    NpgsqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        if (password == (string)reader[0])
                        {
                            Console.WriteLine("Login success!");
                            string tkn = username + "-mtcgToken";
                            UserToken = tkn;
                            reader.Close();

                            try
                            {
                                NpgsqlCommand command2 = new NpgsqlCommand("UPDATE accounts SET token = @p1 WHERE username = @p2", connection);
                                command2.Parameters.Add(new NpgsqlParameter("p1", System.Data.DbType.String));
                                command2.Parameters.Add(new NpgsqlParameter("p2", System.Data.DbType.String));
                                command2.Parameters["p1"].Value = tkn;
                                command2.Parameters["p2"].Value = username;
                                command2.ExecuteNonQuery();
                                string saltedUsername = username;

                                return tkn;

                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                            }


                        }
                        else
                        {
                            reader.Close();
                            Console.WriteLine("Password incorrect!");
                        }
                    }
                    else
                    {
                        reader.Close();
                        Console.WriteLine("User not found!");
                    }
                }
                return null;
            }
        }


        public int Register(string username, string password)
        {
            lock (objectlock)
            {
                if (username == null || password == null)
                {
                    Console.WriteLine("PW OR UN empty");
                    return -1;
                }

                if (connection != null)
                {
                    try
                    {
                        NpgsqlCommand command = new NpgsqlCommand("INSERT INTO accounts (username, password, token) VALUES (@p1, @p2, @p3);", connection);
                        command.Parameters.Add(new NpgsqlParameter("p1", System.Data.DbType.String));
                        command.Parameters.Add(new NpgsqlParameter("p2", System.Data.DbType.String));
                        command.Parameters.Add(new NpgsqlParameter("p3", System.Data.DbType.String));

                        command.Prepare();
                        command.Parameters["p1"].Value = username;
                        command.Parameters["p2"].Value = password;


                        command.Parameters["p3"].Value = username + "-mtcgToken";


                        NpgsqlCommand command2 = new NpgsqlCommand("INSERT INTO accountdata (username, name, bio, coins, image) VALUES(@p1, null, null, 40, null);", connection);
                        command2.Parameters.Add(new NpgsqlParameter("p1", System.Data.DbType.String));
                        command2.Prepare();
                        command2.Parameters["p1"].Value = username;

                        NpgsqlCommand command3 = new NpgsqlCommand("INSERT INTO accountstats (username, elo, wins, losses) VALUES(@p1, 1000, 0, 0);", connection);
                        command3.Parameters.Add(new NpgsqlParameter("p1", System.Data.DbType.String));
                        command3.Prepare();
                        command3.Parameters["p1"].Value = username;

                        command.ExecuteNonQuery();
                        command2.ExecuteNonQuery();
                        command3.ExecuteNonQuery();


                        return 0;


                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        return 1;
                    }
                }
                else
                {
                    return -1;
                }
            }
        }

        public AccountData? getUserById(string username)
        {
            lock (objectlock)
            {
                if (connection != null)
                {
                    NpgsqlCommand command = new NpgsqlCommand("SELECT username, name, bio, image, coins FROM accountData WHERE username = @p1;", connection);
                    command.Parameters.Add(new NpgsqlParameter("p1", System.Data.DbType.String));
                    command.Prepare();
                    command.Parameters["p1"].Value = username;

                    NpgsqlDataReader reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        AccountData accountData = new AccountData();
                        accountData.Username = (string)reader[0];

                        if (reader.IsDBNull(1))
                        {
                            accountData.Name = null;
                        }
                        else
                        {
                            accountData.Name = (string)reader[1];
                        }
                        if (reader.IsDBNull(2))
                        {
                            accountData.Bio = null;
                        }
                        else
                        {
                            accountData.Bio = (string)reader[2];
                        }

                        if (reader.IsDBNull(3))
                        {
                            accountData.Image = null;
                        }
                        else
                        {
                            accountData.Image = (string)reader[3];
                        }

                        if (reader.IsDBNull(4))
                        {
                            accountData.Coins = null;
                        }
                        else
                        {
                            accountData.Coins = (int)reader[4];
                        }

                        reader.Close();
                        return accountData;

                    }
                    else
                    {
                        reader.Close();
                        return null;
                    }
                }
                else
                {
                    Console.WriteLine("DB Erroor");
                    return null;
                }
            }
        }

        public int UpdateAccount(AccountData accountData)
        {
            lock (objectlock)
            {
                if (connection != null)
                {
                    try
                    {
                        NpgsqlCommand command = new NpgsqlCommand("UPDATE accountData SET name = @p1, bio = @p2, image = @p3 WHERE username = @p4;", connection);
                        command.Parameters.Add(new NpgsqlParameter("p1", System.Data.DbType.String));
                        command.Parameters.Add(new NpgsqlParameter("p2", System.Data.DbType.String));
                        command.Parameters.Add(new NpgsqlParameter("p3", System.Data.DbType.String));
                        command.Parameters.Add(new NpgsqlParameter("p4", System.Data.DbType.String));

                        command.Prepare();

                        if (accountData.Name != null)
                        {
                            command.Parameters["p1"].Value = accountData.Name;
                        }
                        else
                        {
                            command.Parameters["p1"].Value = DBNull.Value;
                        }
                        if (accountData.Name != null)
                        {
                            command.Parameters["p2"].Value = accountData.Bio;
                        }
                        else
                        {
                            command.Parameters["p2"].Value = DBNull.Value;
                        }
                        if (accountData.Image != null)
                        {
                            command.Parameters["p3"].Value = accountData.Image;
                        }
                        else
                        {
                            command.Parameters["p3"].Value = DBNull.Value;
                        }

                        command.Parameters["p4"].Value = accountData.Username;

                        command.ExecuteNonQuery();
                        return 1;

                        // Success

                    }
                    catch (PostgresException ex)
                    {
                        // USER NOT FOUND
                        Console.WriteLine(ex.Message);
                        return 0;

                    }

                }
                else
                {
                    return -1;
                }
            }
        }

        public void UpdateStats(AccountStats accountStats)
        {
            lock (objectlock)
            {
                if (connection != null)
                {
                    try
                    {
                        NpgsqlCommand command = new NpgsqlCommand("UPDATE accountstats SET elo = @p1, wins = @p2, losses = @p3 WHERE username = @p4", connection);

                        command.Parameters.Add(new NpgsqlParameter("p1", System.Data.DbType.Int32));
                        command.Parameters.Add(new NpgsqlParameter("p2", System.Data.DbType.Int32));
                        command.Parameters.Add(new NpgsqlParameter("p3", System.Data.DbType.Int32));
                        command.Parameters.Add(new NpgsqlParameter("p4", System.Data.DbType.String));

                        command.Prepare();

                        if (accountStats.Elo <= 0)
                        {
                            command.Parameters["p1"].Value = 1;
                        }
                        else
                        {
                            command.Parameters["p1"].Value = accountStats.Elo;
                        }

                        command.Parameters["p2"].Value = accountStats.Wins;
                        command.Parameters["p3"].Value = accountStats.Losses;
                        command.Parameters["p4"].Value = accountStats.Name;

                        command.ExecuteNonQuery();

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error when updating stats");
                    }
                }
            }
        }

        public bool AuthorizedUser()
        {
            lock (objectlock)
            {
                // string usernameToken = username + "-mtcgToken";
                if (UserToken == null)
                {
                    return false;
                }

                NpgsqlCommand command = new NpgsqlCommand("SELECT username FROM accounts WHERE token = @p1;", connection);
                command.Parameters.Add(new NpgsqlParameter("p1", System.Data.DbType.String));
                command.Prepare();
                command.Parameters["p1"].Value = UserToken;

                NpgsqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    currentUser = (string)reader[0];
                    reader.Close();
                    return true;
                }
                else
                {
                    currentUser = null;
                    reader.Close();
                    return false;
                }

            }
        }



        public AccountData? getUserFromDB(string username)
        {
            lock (objectlock)
            {
                if (connection != null)
                {
                    NpgsqlCommand command = new NpgsqlCommand("SELECT username, name, bio, coins, image FROM accountData WHERE username = @p1", connection);
                    command.Parameters.Add(new NpgsqlParameter("p1", System.Data.DbType.String));
                    command.Prepare();
                    command.Parameters["p1"].Value = username;

                    NpgsqlDataReader dataReader = command.ExecuteReader();
                    if (dataReader.Read())
                    {
                        AccountData accountData = new AccountData();
                        accountData.Username = (string)dataReader[0];

                        if (dataReader.IsDBNull(1))
                        {
                            accountData.Name = null;
                        }
                        else
                        {
                            accountData.Name = (string)dataReader[1];
                        }

                        if (dataReader.IsDBNull(2))
                        {
                            accountData.Bio = null;
                        }
                        else
                        {
                            accountData.Bio = (string)dataReader[2];
                        }


                        if (dataReader.IsDBNull(3))
                        {
                            accountData.Coins = null;
                        }
                        else
                        {
                            accountData.Coins = (int)dataReader[3];
                        }
                        if (dataReader.IsDBNull(4))
                        {
                            accountData.Image = null;
                        }
                        else
                        {
                            accountData.Image = (string)dataReader[4];
                        }

                        dataReader.Close();
                        return accountData;

                    }
                    else
                    {
                        dataReader.Close();
                        return null;
                    }
                }
                else
                {
                    Console.WriteLine("DB Problem");
                    return null;
                }
            }
        }

        // USER STATS

        public AccountStats? getAccountStats(string username)
        {
            lock (objectlock)
            {

                if (connection != null && currentUser != null)
                {

                    NpgsqlCommand command = new NpgsqlCommand("SELECT username, elo, wins, losses FROM accountstats WHERE username = @p1;", connection);
                    command.Parameters.Add(new NpgsqlParameter("p1", System.Data.DbType.String));
                    command.Prepare();
                    if (username == "defined")
                    {
                        command.Parameters["p1"].Value = currentUser;

                    }
                    else
                    {
                        command.Parameters["p1"].Value = username;

                    }
                    NpgsqlDataReader reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        AccountStats accountStats = new AccountStats();
                        accountStats.Name = (string)reader[0];
                        accountStats.Elo = (int)reader[1];
                        accountStats.Wins = (int)reader[2];
                        accountStats.Losses = (int)reader[3];

                        reader.Close();
                        return accountStats;
                    }
                    else
                    {
                        reader.Close();
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
        }

        public string? getScoreboard()
        {
            lock (objectlock)
            {
                if (connection != null && currentUser != null)
                {
                    NpgsqlCommand command = new NpgsqlCommand("SELECT username, elo, wins, losses FROM accountstats ORDER BY ELO DESC;", connection);
                    command.Prepare();
                    NpgsqlDataReader reader = command.ExecuteReader();

                    List<AccountStats> accountStats = new List<AccountStats>();
                    while (reader.Read())
                    {
                        AccountStats stat = new AccountStats();
                        stat.Name = (string)reader[0];
                        stat.Elo = (int)reader[1];
                        stat.Wins = (int)reader[2];
                        stat.Losses = (int)reader[3];

                        accountStats.Add(stat);
                    }

                    reader.Close();

                    if (accountStats.Count > 0)
                    {
                        string jsonText = System.Text.Json.JsonSerializer.Serialize(new { Scoreboard = accountStats });
                        return jsonText;
                    }
                    else
                    {
                        return null;
                    }

                }
                else
                {
                    return null;
                }
            }
        }

        //


        // CARD OPERATIONS

        private void createNewCard(Guid id, string name, int type, int element, double damage, string? owner)
        {
            lock (objectlock)
            {
                if (connection != null)
                {
                    NpgsqlCommand command = new NpgsqlCommand("INSERT INTO cards (id, name, type, element, damage, owner) VALUES (@p1, @p2, @p3, @p4, @p5, @p6);", connection);
                    command.Parameters.Add(new NpgsqlParameter("p1", System.Data.DbType.Guid));
                    command.Parameters.Add(new NpgsqlParameter("p2", System.Data.DbType.String));
                    command.Parameters.Add(new NpgsqlParameter("p3", System.Data.DbType.Int32));
                    command.Parameters.Add(new NpgsqlParameter("p4", System.Data.DbType.Int32));
                    command.Parameters.Add(new NpgsqlParameter("p5", System.Data.DbType.Int32));
                    command.Parameters.Add(new NpgsqlParameter("p6", System.Data.DbType.String));

                    command.Prepare();
                    command.Parameters["p1"].Value = id;
                    command.Parameters["p2"].Value = name;
                    command.Parameters["p3"].Value = type;
                    command.Parameters["p4"].Value = element;
                    command.Parameters["p5"].Value = damage;


                    if (owner != null)
                    {
                        command.Parameters["p6"].Value = owner;
                    }
                    else
                    {
                        command.Parameters["p6"].Value = DBNull.Value;

                    }

                    command.ExecuteNonQuery();
                }
            }
        }

        public int CreateCardPackage()
        {
            lock (objectlock)
            {
                if (connection != null)
                {
                    if (currentUser == "admin")
                    {
                        Console.WriteLine("here");
                        List<Guid> currentCardIds = new List<Guid>();
                        try
                        {
                            currentCardIds.Add(Guid.NewGuid());
                            currentCardIds.Add(Guid.NewGuid());
                            currentCardIds.Add(Guid.NewGuid());
                            currentCardIds.Add(Guid.NewGuid());
                            currentCardIds.Add(Guid.NewGuid());
                            Random random = new Random();
                            foreach (Guid id in currentCardIds)
                            {
                                int element = random.Next(0, 2);
                                int type = random.Next(0, 1);
                                string cardName = "";

                                if ((CardType)type == CardType.Monster)
                                {
                                    string[] availableMonsters = { "Goblin", "Troll", "Knight", "Goblin" };
                                    int monsterIndex = random.Next(availableMonsters.Length);
                                    cardName = element + availableMonsters[monsterIndex];
                                }
                                else
                                {
                                    cardName = element + "Spell";

                                }
                                createNewCard(id, cardName, type, element, random.Next(5, 100), null);
                            }
                        }
                        catch (PostgresException)
                        {
                            return 409;
                        }
                        Console.WriteLine("yes");

                        NpgsqlCommand command = new NpgsqlCommand("INSERT into packages (card1, card2, card3, card4, card5) VALUES (@p1, @p2, @p3, @p4, @p5);", connection);
                        command.Parameters.Add(new NpgsqlParameter("p1", System.Data.DbType.Guid));
                        command.Parameters.Add(new NpgsqlParameter("p2", System.Data.DbType.Guid));
                        command.Parameters.Add(new NpgsqlParameter("p3", System.Data.DbType.Guid));
                        command.Parameters.Add(new NpgsqlParameter("p4", System.Data.DbType.Guid));
                        command.Parameters.Add(new NpgsqlParameter("p5", System.Data.DbType.Guid));

                        command.Prepare();
                        command.Parameters["p1"].Value = currentCardIds[0];
                        command.Parameters["p2"].Value = currentCardIds[1];
                        command.Parameters["p3"].Value = currentCardIds[2];
                        command.Parameters["p4"].Value = currentCardIds[3];
                        command.Parameters["p5"].Value = currentCardIds[4];


                        command.ExecuteNonQuery();
                        Console.WriteLine("yes worked");

                        return 201;


                    }
                    else
                    {
                        return 403;
                    }
                }
                else
                {
                    return 400;
                }
            }
        }

        public string? BuyPackage()
        {
            lock (objectlock)
            {
                if (connection != null && currentUser != null)
                {
                    int budget = getBudget(currentUser);
                    Console.WriteLine($"Budget {budget}");
                    if (budget < 0)
                    {
                        // budget seems smaller than zero.
                        return null;
                    }
                    NpgsqlCommand command = new NpgsqlCommand("SELECT c.name, c.type, c.element, c.damage, c.id, p.id FROM packages p, cards c WHERE c.id IN(p.card1, p.card2, p.card3, p.card4, p.card5);", connection);
                    command.Prepare();
                    NpgsqlDataReader reader = command.ExecuteReader();
                    CardPackage package = new CardPackage();
                    List<Guid> packageCardIds = new List<Guid>();
                    int packageID = 0;
                    while (reader.Read())
                    {
                        CardType cardType = (CardType)(Int32)reader[1];
                        ElementType elementType = (ElementType)(Int32)reader[2];
                        package.PackageCards.Add(new Card((string)reader[0], (Int32)reader[3], elementType, cardType));
                        packageCardIds.Add((Guid)reader[4]);
                        package.PackageCards.Last().cardId = (Guid)reader[4];
                        packageID = (int)reader[5];
                    }
                    reader.Close();
                    if (budget < 5)
                    {
                        return "403";
                    }

                    if (package.PackageCards.Count == 0)
                    {
                        return "404";

                    }
                    if (updateBudget(currentUser, budget - 5) != 0)
                    {
                        return null;
                    }

                    foreach (Guid id in packageCardIds)
                    {
                        changeCardOwner(currentUser, id);
                    }

                    Console.WriteLine("Works till here");

                    deletePack(packageID);

                    string jsonText = System.Text.Json.JsonSerializer.Serialize(new { cards = package.PackageCards });
                    return jsonText;

                }
                else
                {
                    return null;
                }

            }
        }


        // GET CARD FROM DB
        public string? FetchCardsFromDataBase()
        {
            lock (objectlock)
            {
                if (connection != null || currentUser == null)
                {
                    NpgsqlCommand command = new NpgsqlCommand("SELECT type, name, element, damage, id FROM cards WHERE owner = @p1;", connection);
                    command.Parameters.Add(new NpgsqlParameter("p1", System.Data.DbType.String));
                    command.Prepare();
                    // Console.WriteLine($"CurrentUSer = {currentUser}");
                    command.Parameters["p1"].Value = currentUser;

                    NpgsqlDataReader reader = command.ExecuteReader();

                    return FetchJSONCard(reader);

                }
                else
                {
                    return null;
                }
            }
        }

        // GET DECK FROM DB
        public bool SetupDeck(List<string> cards)
        {
            lock (objectlock)
            {
                if (connection != null || currentUser == null)
                {
                    NpgsqlCommand command = new NpgsqlCommand("UPDATE decks SET c1=@p1, c2=@p2, c3=@p3, c4=@p4 WHERE owner=@p5", connection);
                    command.Parameters.Add(new NpgsqlParameter("p1", System.Data.DbType.Guid));
                    command.Parameters.Add(new NpgsqlParameter("p2", System.Data.DbType.Guid));
                    command.Parameters.Add(new NpgsqlParameter("p3", System.Data.DbType.Guid));
                    command.Parameters.Add(new NpgsqlParameter("p4", System.Data.DbType.Guid));
                    command.Parameters.Add(new NpgsqlParameter("p5", System.Data.DbType.String));
                    command.Prepare();
                    try
                    {
                        command.Parameters["p1"].Value = Guid.Parse(cards[0]);
                        command.Parameters["p2"].Value = Guid.Parse(cards[1]);
                        command.Parameters["p3"].Value = Guid.Parse(cards[2]);
                        command.Parameters["p4"].Value = Guid.Parse(cards[3]);

                    }
                    catch (Exception ex)
                    {
                        return false;
                    }

                    command.Parameters["p5"].Value = currentUser;

                    command.ExecuteNonQuery();

                    return true;
                }
                else
                {
                    return false;
                }

            }
        }

        public List<Card>? GetDeckFromDBAsList()
        {
            lock (objectlock)
            {
                if (connection != null || currentUser == null)
                {
                    NpgsqlCommand command = new NpgsqlCommand("SELECT c1, c2, c3, c4 FROM decks WHERE owner = @p1", connection);
                    command.Parameters.Add(new NpgsqlParameter("p1", System.Data.DbType.String));
                    command.Prepare();

                    command.Parameters["p1"].Value = currentUser;
                    NpgsqlDataReader reader = command.ExecuteReader();
                    List<Guid> currentCardIDs = new List<Guid>();


                    // Console.WriteLine("works till here");

                    if (reader.Read())
                    {
                        if (reader.IsDBNull(0) || reader.IsDBNull(1) || reader.IsDBNull(2) || reader.IsDBNull(3))
                        {
                            Console.WriteLine("db null?");

                            reader.Close();
                            return null;
                        }

                        Console.WriteLine("no db null");

                        currentCardIDs.Add((Guid)reader[0]);
                        currentCardIDs.Add((Guid)reader[1]);
                        currentCardIDs.Add((Guid)reader[2]);
                        currentCardIDs.Add((Guid)reader[3]);

                        reader.Close();

                    }
                    else
                    {
                        // Console.WriteLine("no reader read");

                        reader.Close();
                        return null;
                    }

                    Console.WriteLine("weiter");
                    Console.WriteLine(currentCardIDs[1]);

                    NpgsqlCommand command2 = new NpgsqlCommand("SELECT type, name, element, damage, id FROM cards WHERE id=@p1 OR id=@p2 OR id=@p3 OR id=@p3 OR id=@p4", connection);
                    command2.Parameters.Add(new NpgsqlParameter("p1", System.Data.DbType.Guid));
                    command2.Parameters.Add(new NpgsqlParameter("p2", System.Data.DbType.Guid));
                    command2.Parameters.Add(new NpgsqlParameter("p3", System.Data.DbType.Guid));
                    command2.Parameters.Add(new NpgsqlParameter("p4", System.Data.DbType.Guid));

                    command2.Prepare();

                    command2.Parameters["p1"].Value = currentCardIDs[0];
                    command2.Parameters["p2"].Value = currentCardIDs[1];
                    command2.Parameters["p3"].Value = currentCardIDs[2];
                    command2.Parameters["p4"].Value = currentCardIDs[3];

                    NpgsqlDataReader reader2 = command2.ExecuteReader();

                    List<Card> cards = new List<Card>();
                    while (reader2.Read())
                    {
                        CardType cardType = (CardType)reader[0];
                        ElementType elementType = (ElementType)reader2[2];
                        cards.Add(new Card((string)reader[1], (Int32)reader[3], elementType, cardType));
                    }
                    reader2.Close();

                    Console.WriteLine($"Card count {cards.Count}");

                    if (cards.Count > 0)
                    {
                        return cards;
                    }
                    else
                    {
                        return null;
                    }

                }
                else
                {
                    Console.WriteLine("DB error!");
                    return null;
                }
            }

        }
        public List<Card>? GetDeckFromDB(string type = "json")
        {
            lock (objectlock)
            {
                if (connection != null || currentUser == null)
                {
                    NpgsqlCommand command = new NpgsqlCommand("SELECT c1, c2, c3, c4 FROM decks WHERE owner = @p1", connection);
                    command.Parameters.Add(new NpgsqlParameter("p1", System.Data.DbType.String));
                    command.Prepare();

                    command.Parameters["p1"].Value = currentUser;
                    NpgsqlDataReader reader = command.ExecuteReader();
                    List<Guid> currentCardIDs = new List<Guid>();


                    // Console.WriteLine("works till here");

                    if (reader.Read())
                    {
                        if (reader.IsDBNull(0) || reader.IsDBNull(1) || reader.IsDBNull(2) || reader.IsDBNull(3))
                        {
                            Console.WriteLine("db null?");

                            reader.Close();
                            return null;
                        }

                        Console.WriteLine("no db null");

                        currentCardIDs.Add((Guid)reader[0]);
                        currentCardIDs.Add((Guid)reader[1]);
                        currentCardIDs.Add((Guid)reader[2]);
                        currentCardIDs.Add((Guid)reader[3]);

                        reader.Close();

                    }
                    else
                    {
                        // Console.WriteLine("no reader read");

                        reader.Close();
                        return null;
                    }

                    Console.WriteLine("weiter");

                    NpgsqlCommand command2 = new NpgsqlCommand("SELECT type, name, element, damage, id FROM cards WHERE id in(@p1, @p2, @p3, @p4);", connection);
                    command2.Parameters.Add(new NpgsqlParameter("p1", System.Data.DbType.Guid));
                    command2.Parameters.Add(new NpgsqlParameter("p2", System.Data.DbType.Guid));
                    command2.Parameters.Add(new NpgsqlParameter("p3", System.Data.DbType.Guid));
                    command2.Parameters.Add(new NpgsqlParameter("p4", System.Data.DbType.Guid));

                    command2.Prepare();

                    command2.Parameters["p1"].Value = currentCardIDs[0];
                    command2.Parameters["p2"].Value = currentCardIDs[1];
                    command2.Parameters["p3"].Value = currentCardIDs[2];
                    command2.Parameters["p4"].Value = currentCardIDs[3];

                    NpgsqlDataReader reader2 = command2.ExecuteReader();

                    List<Card> cards = new List<Card>();
                    while (reader2.Read())
                    {
                        CardType cardType = (CardType)reader[0];
                        ElementType elementType = (ElementType)reader2[2];
                        cards.Add(new Card((string)reader[1], (Int32)reader[3], elementType, cardType));
                    }
                    reader2.Close();

                    if (cards.Count > 0)
                    {
                        return cards;
                    }
                    else
                    {
                        return null;
                    }

                }
                else
                {
                    Console.WriteLine("DB error!");
                    return null;
                }
            }
        }

        // Change Ownership



        private void changeCardOwner(string username, Guid givenCardId)
        {
            lock (objectlock)
            {
                if (connection != null)
                {
                    NpgsqlCommand command = new NpgsqlCommand("UPDATE cards SET owner = @p1 WHERE id=@p2", connection);
                    command.Parameters.Add(new NpgsqlParameter("p1", System.Data.DbType.String));
                    command.Parameters.Add(new NpgsqlParameter("p2", System.Data.DbType.Guid));
                    command.Prepare();
                    command.Parameters["p1"].Value = username;
                    command.Parameters["p2"].Value = givenCardId;
                    command.ExecuteNonQuery();
                }
            }
        }
        // Delete Pack
        private void deletePack(int id)
        {
            lock (objectlock)
            {
                if (connection != null)
                {
                    NpgsqlCommand command = new NpgsqlCommand("DELETE FROM packages WHERE id=@p1", connection);
                    command.Parameters.Add(new NpgsqlParameter("p1", System.Data.DbType.Int32));
                    command.Prepare();
                    command.Parameters["p1"].Value = id;
                    command.ExecuteNonQuery();
                }
            }
        }

        // Card Conversions
        private string? FetchJSONCard(NpgsqlDataReader reader)
        {
            List<Card> cards = new List<Card>();
            while (reader.Read())
            {
                CardType cardType = (CardType)(Int32)reader[0];
                ElementType elementType = (ElementType)(Int32)reader[2];
                Card card = new Card((string)reader[1], (Int32)reader[3], elementType, cardType);
                card.cardId = (Guid)reader[4];
                cards.Add(card);

            }
            reader.Close();

            if (cards.Count > 0)
            {
                string jsonText = System.Text.Json.JsonSerializer.Serialize(new { cards = cards });
                return jsonText;
            }
            else
            {
                return null;
            }

        }

        private string? FetchPlainCard(NpgsqlDataReader reader)
        {
            List<Card> currentCards = new List<Card>();
            while (reader.Read())
            {
                // SELECT type, name, element, damage, id FROM cards WHERE id in(@p1, @p2, @p3, @p4
                CardType cardType = (CardType)(Int32)reader[0];
                ElementType elementType = (ElementType)(Int32)reader[2];
                Card card = new Card((string)reader[1], (Int32)reader[3], elementType, cardType);
                card.cardId = (Guid)reader[4];
                currentCards.Add(card);
            }
            reader.Close();
            if (currentCards.Count > 0)
            {
                string plainText = "";
                int i = 1;
                foreach (Card card in currentCards)
                {
                    plainText = plainText + "Card " + i + ": " + card.ToString();
                    ++i;
                }
                return plainText;
            }
            else
            {
                return null;
            }
        }

        // User economy
        private int getBudget(string username)
        {
            lock (objectlock)
            {
                if (connection != null)
                {
                    NpgsqlCommand command = new NpgsqlCommand("SELECT coins from accountData WHERE username = @p1", connection);
                    command.Parameters.Add(new NpgsqlParameter("p1", System.Data.DbType.String));
                    command.Prepare();
                    command.Parameters["p1"].Value = username;

                    NpgsqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        int currentCoins = (int)reader[0];
                        reader.Close();
                        return currentCoins;
                    }
                    else
                    {
                        reader.Close();
                        return -1;
                    }
                }
                else
                {
                    return -1;
                }
            }
        }

        private int updateBudget(string username, int coins)
        {
            lock (objectlock)
            {
                if (connection != null)
                {
                    NpgsqlCommand command = new NpgsqlCommand("UPDATE accountData SET coins = @p1 WHERE username = @p2", connection);
                    command.Parameters.Add(new NpgsqlParameter("p1", System.Data.DbType.Int32));
                    command.Parameters.Add(new NpgsqlParameter("p2", System.Data.DbType.String));
                    command.Prepare();
                    command.Parameters["p1"].Value = coins;
                    command.Parameters["p2"].Value = username;

                    command.ExecuteNonQuery();
                    return 0;
                }
                else
                {
                    return -1;
                }
            }
        }

        //TRADING



        public string? GetTradingDeal()
        {
            lock (objectlock)
            {
                if (connection != null && currentUser != null)
                {
                    NpgsqlCommand command = new NpgsqlCommand("SELECT c.id, d.id, d.type, d.minDamage FROM cards c, deals d WHERE c.id = d.cardid", connection);
                    command.Prepare();
                    NpgsqlDataReader reader = command.ExecuteReader();

                    List<Deal> deals = new List<Deal>();
                    while (reader.Read())
                    {
                        Deal deal = new Deal();
                        deal.CardId = (Guid)reader[0];
                        deal.Id = (Int32)reader[1];
                        deal.Type = (CardType)reader[2];
                        deal.MinDamage = (Int32)reader[3];
                        deals.Add(deal);


                    }
                    reader.Close();
                    if (deals.Count > 0)
                    {
                        string jsonText = System.Text.Json.JsonSerializer.Serialize(new { TradingDeals = deals });
                        return jsonText;
                    }
                    else
                    {
                        return "204";
                    }

                }
                else
                {
                    Console.WriteLine("DB Eerror");
                    return null;
                }
            }
        }

        public int CreateDeal(Deal deal)
        {
            lock (objectlock)
            {
                if (connection != null)
                {
                    NpgsqlCommand command = new NpgsqlCommand("SELECT * FROM cards WHERE owner = @p1 AND id = @p2;", connection);
                    command.Parameters.Add(new NpgsqlParameter("p1", System.Data.DbType.String));
                    command.Parameters.Add(new NpgsqlParameter("p2", System.Data.DbType.Guid));
                    command.Prepare();
                    command.Parameters["p1"].Value = currentUser;
                    command.Parameters["p2"].Value = deal.CardId;
                    NpgsqlDataReader reader = command.ExecuteReader();

                    if (!reader.HasRows)
                    {
                        reader.Close();
                        return 403;
                    }
                    reader.Close();

                    NpgsqlCommand command2 = new NpgsqlCommand("INSERT INTO deals (cardid, type, mindamage) VALUES (@p1, @p2, @p3);", connection);
                    command2.Parameters.Add(new NpgsqlParameter("p1", System.Data.DbType.Guid));

                    command2.Parameters.Add(new NpgsqlParameter("p2", System.Data.DbType.Int32));
                    command2.Parameters.Add(new NpgsqlParameter("p3", System.Data.DbType.Int32));

                    command2.Prepare();
                    command2.Parameters["p1"].Value = deal.CardId;
                    command2.Parameters["p2"].Value = (Int32)deal.Type;
                    command2.Parameters["p3"].Value = deal.MinDamage;

                    command2.ExecuteNonQuery();

                    return 200;



                }
                else
                {
                    return 400;
                }
            }
        }

        public int ExecuteTrading(int dealid, Guid cardid)
        {
            lock (objectlock)
            {
                if (connection != null)
                {
                    NpgsqlCommand command = new NpgsqlCommand("SELECT c.type, d.type, c.damage, d.mindamage, c.username FROM cards c, deals d WHERE d.id = @p1 AND c.id = @p2", connection);
                    command.Parameters.Add(new NpgsqlParameter("p1", System.Data.DbType.Int32));
                    command.Parameters.Add(new NpgsqlParameter("p2", System.Data.DbType.Guid));

                    command.Prepare();
                    command.Parameters["p1"].Value = dealid;
                    command.Parameters["p2"].Value = cardid;

                    NpgsqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        if ((Int32)reader[0] != (Int32)reader[1])
                        {
                            reader.Close();
                            return 403;
                        }

                        if ((Int32)reader[2] < (Int32)reader[3])
                        {
                            reader.Close();
                            return 403;

                        }

                        if (reader.IsDBNull(4))
                        {
                            return 400;
                        }

                        else if ((string)reader[4] != currentUser)
                        {
                            return 403;
                        }

                        reader.Close();
                    }
                    else
                    {
                        reader.Close();
                        return 404;
                    }

                    NpgsqlCommand command2 = new NpgsqlCommand("SELECT c.owner, c.id FROM cards, deals d WHERE d.id = @p1 AND c.id = d.cardid", connection);
                    command2.Parameters.Add(new NpgsqlParameter("p1", System.Data.DbType.Int32));

                    command2.Prepare();
                    command2.Parameters["p1"].Value = dealid;

                    NpgsqlDataReader reader2 = command2.ExecuteReader();
                    string offerer;
                    Guid cardidOfferer;
                    if (reader2.Read())
                    {
                        if (reader2.IsDBNull(0))
                        {
                            reader2.Close();
                            return 400;
                        }
                        offerer = (string)reader2[0];
                        cardidOfferer = (Guid)reader2[1];
                        reader2.Close();
                    }
                    else
                    {
                        reader2.Close();
                        return 400;
                    }

                    if (offerer != currentUser)
                    {
                        RemoveDeal(dealid, offerer);
                        changeCardOwner(offerer, cardid);
                        changeCardOwner(currentUser, cardidOfferer);
                        reader2.Close();
                        return 200;

                    }
                    else
                    {
                        reader2.Close();
                        return 400;
                    }
                }
                else
                {
                    return 400;
                }
            }
        }

        public int RemoveDeal(int id, string username)
        {
            lock (objectlock)
            {
                if (connection != null)
                {
                    NpgsqlCommand command = new NpgsqlCommand("SELECT c.owner FROM cards c, deals d WHERE d.cardid = c.id AND d.id = @p1 AND c.owner = @p2", connection);
                    command.Parameters.Add(new NpgsqlParameter("p1", System.Data.DbType.Int32));
                    command.Parameters.Add(new NpgsqlParameter("p2", System.Data.DbType.String));
                    command.Prepare();
                    command.Parameters["p1"].Value = id;
                    command.Parameters["p2"].Value = username;
                    NpgsqlDataReader reader = command.ExecuteReader();

                    if (!reader.HasRows)
                    {
                        reader.Close();
                        return 403;
                    }
                    reader.Close();

                    NpgsqlCommand command2 = new NpgsqlCommand("DELETE FROM deals WHERE id = @p1", connection);
                    command2.Parameters.Add(new NpgsqlParameter("p1", System.Data.DbType.Int32));
                    command2.Prepare();
                    command2.Parameters["p1"].Value = id;

                    command.ExecuteNonQuery();
                    return 200;


                }
                else
                {
                    return 400;
                }
            }
        }

    }


}