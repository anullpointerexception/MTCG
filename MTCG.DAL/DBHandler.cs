using MTCG.Model.User;
using Npgsql;

namespace MTCG.DAL
{
    public class DBHandler
    {
        string connectionString = "Host=localhost:5432;Username=swe1user;Password=swe1pw;Database=mtcg";

        private static DBHandler instance = null;

        private static readonly object objectlock = new object();

        NpgsqlConnection connection;

        private string? UserToken;

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
                    return instance;
                }
            }
        }

        public bool SetupConnection()
        {
            try
            {
                connection = new NpgsqlConnection(connectionString);
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
            if (username == null || password == null)
            {
                Console.WriteLine("PW OR UN empty");
                return -1;
            }

            if (connection != null)
            {
                try
                {
                    NpgsqlCommand command = new NpgsqlCommand("INSERT INTO accounts (username, password) VALUES (@p1, @p2);", connection);
                    command.Parameters.Add(new NpgsqlParameter("p1", System.Data.DbType.String));
                    command.Parameters.Add(new NpgsqlParameter("p2", System.Data.DbType.String));
                    command.Prepare();
                    command.Parameters["p1"].Value = username;
                    command.Parameters["p2"].Value = password;

                    NpgsqlCommand command2 = new NpgsqlCommand("INSERT INTO accountdata (username, name, bio, coins) VALUES(@p1, null, null, 40);", connection);
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

        public bool AuthorizedUserToken(string username)
        {
            lock (objectlock)
            {
                string usernameToken = username + "-mtcgToken";
                NpgsqlCommand command = new NpgsqlCommand("SELECT username FROM accounts WHERE token = @p1;", connection);
                command.Parameters.Add(new NpgsqlParameter("p1", System.Data.DbType.String));
                command.Prepare();
                command.Parameters["p1"].Value = usernameToken;

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
            if (connection != null)
            {
                NpgsqlCommand command = new NpgsqlCommand("SELECT username, name, bio, coins FROM accountData WHERE username = @p1", connection);
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


}