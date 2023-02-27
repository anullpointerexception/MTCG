namespace MTCG.Model.User
{
    public class AccountCredentials
    {
        public string Username { get; set; }

        public string Password { get; set; }

        public AccountCredentials(string username, string password)
        {
            Username = username;
            Password = password;
        }
    }
}
