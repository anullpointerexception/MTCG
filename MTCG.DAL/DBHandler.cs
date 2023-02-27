using Npgsql;

namespace MTCG.DAL
{
    public class DBHandler
    {
        string connectionString;

        NpgsqlConnection connection;
        public DBHandler(string connectionString)
        {
            this.connectionString = connectionString;
        }


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



    }


}