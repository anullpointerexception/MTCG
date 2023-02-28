using MTCG.BL.BattleLogic;
using MTCG.DAL;
using MTCG.Model.Cards;
using MTCG.Model.Player;
using MTCG.Model.User;
using System.Net.Sockets;

namespace MTCG.BL.HttpService
{
    public class Handler
    {
        DBHandler dbHandler = DBHandler.Instance;

        // Setting up connection!

        public Handler()
        {
            dbHandler.SetupConnection();
        }


        public void HandleREQ(TcpClient sock)
        {
            try
            {
                StreamReader reader = new StreamReader(sock.GetStream());
                Request request = new Request(sock, reader);

                // USER BLOCK //

                // POST METHODS
                if (request.Method == Method.POST)
                {
                    Console.WriteLine("POST REQ");

                    // SIGNUP
                    if (request.Path == "/users")
                    {
                        Console.WriteLine(request.Content);

                        if (request.BodyMessage.ContainsKey("Username") && request.BodyMessage.ContainsKey("Password"))
                        {
                            int result = dbHandler.Register(request.BodyMessage["Username"], request.BodyMessage["Password"]);
                            if (result == 0)
                            {
                                sendRES(sock, 201, "OK", "User successfully created!");

                            }
                            else if (result == 1)
                            {
                                sendRES(sock, 409, "Conflict", "User with same name already registered!");
                            }
                            else
                            {
                                sendRES(sock, 500, "Internal Server Error", "The request was not completed. The server met an unexpected condition.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Seems like no username or pw");
                        }
                    }
                    else if (request.Path == "/sessions")
                    {
                        if (request.BodyMessage.ContainsKey("Username") && request.BodyMessage.ContainsKey("Password"))
                        {
                            string? token = dbHandler.Login(request.BodyMessage["Username"], request.BodyMessage["Password"]);
                            if (token != null)
                            {
                                sendRES(sock, 200, "OK", "{\"authToken\":\"" + token.ToString() + "\"}");
                                dbHandler.currentUser = request.BodyMessage["Username"];
                            }
                            else
                            {
                                sendRES(sock, 401, "Unauthorized", "Invalid Username/Password");
                            }
                        }
                    }
                    else if (request.Path == "/battles")
                    {
                        if (dbHandler.AuthorizedUser())
                        {
                            List<Card>? currentDeck = dbHandler.GetDeckFromDB();
                            if (currentDeck != null && dbHandler.currentUser != null)
                            {
                                List<string>? response = BattleService.Instance.JoinPlayerLobby(new Player(currentDeck, dbHandler.currentUser));
                                if (response != null)
                                {
                                    string sendText = String.Join("\n", response);
                                    sendRES(sock, 200, "OK", sendText, "text/plain");
                                }
                                else
                                {
                                    Console.WriteLine("Response null");
                                    sendRES(sock, 400, "Bad Request", "The server did not understand the request.");
                                }
                            }
                            else
                            {
                                sendRES(sock, 400, "Bad Request", "The server did not understand the request.");
                            }

                        }
                        else
                        {
                            sendRES(sock, 401, "Unauthorized", "Access token is missing or invalid.");
                        }
                    }
                    else if (request.Path == "/packages")
                    {
                        Console.WriteLine("pack");

                        Console.WriteLine($"User Token: {dbHandler.UserToken}");

                        if (dbHandler.AuthorizedUser())
                        {
                            Console.WriteLine("auth yes");

                            int response = dbHandler.CreateCardPackage();
                            Console.WriteLine("auth no");

                            if (response == 201)
                            {
                                sendRES(sock, response, "Created", "Package and cards successfully created");
                            }
                            else if (response == 400)
                            {
                                sendRES(sock, 400, "Bad Request", "The server did not understand the request.");
                            }
                            else if (response == 403)
                            {
                                sendRES(sock, response, "Forbidden", "Provided user is not 'admin'");

                            }
                            else if (response == 409)
                            {
                                sendRES(sock, response, "Conflict", "At least one card in the package already existed!");
                            }

                        }
                        else
                        {
                            sendRES(sock, 401, "Unauthorized", "Access token is missing or invalid.");
                        }
                    }
                    else if (request.Path == "/transactions/packages")
                    {
                        Console.WriteLine("/transactions/packages");
                        // Console.WriteLine(request.headers["Authorization"]);
                        string input = request.headers["Authorization"].Substring(0, request.headers["Authorization"].IndexOf("-"));
                        // Console.WriteLine(input);
                        dbHandler.UserToken = request.headers["Authorization"];

                        // TO-DO /transcation/packages Endpoint
                        if (dbHandler.AuthorizedUser())
                        {
                            string? response = dbHandler.BuyPackage();
                            if (response == null)
                            {
                                sendRES(sock, 400, "Bad Request", "The server did not understand the request.");
                            }
                            else if (response == "403")
                            {
                                sendRES(sock, 403, "Forbidden", "Not enough money for buying a card package");
                            }
                            else if (response == "404")
                            {
                                sendRES(sock, 404, "Not found", "No card package available for buying");

                            }
                            else
                            {
                                sendRES(sock, 200, "OK", response);
                            }
                        }
                        else
                        {
                            sendRES(sock, 401, "Unauthorized", "Access token is missing or invalid.");
                        }
                    }

                }

                // GET METHODS

                if (request.Method == Method.GET)
                {
                    Console.WriteLine("GET REQ");

                    if (request.Path == "/users/")
                    {
                        if (request.QueryParams.Count > 0)
                        {
                            if (dbHandler.AuthorizedUser())
                            {
                                if (dbHandler.currentUser == request.QueryParams["username"] || dbHandler.currentUser == "admin")
                                {
                                    AccountData? accountData = dbHandler.getUserFromDB(request.QueryParams["username"]);
                                    if (accountData != null)
                                    {
                                        sendRES(sock, 200, "OK", System.Text.Json.JsonSerializer.Serialize(accountData));
                                    }
                                    else
                                    {
                                        sendRES(sock, 404, "Not found", "User not found");
                                    }
                                }

                            }
                            else
                            {
                                sendRES(sock, 401, "Invalid", "Access token is missing or invalid");
                                Console.WriteLine("auth failed");
                            }

                        }
                        else
                        {
                            Console.WriteLine("not enough query params");
                        }

                    }
                    else if (request.Path == "/cards")
                    {
                        if (dbHandler.AuthorizedUser())
                        {
                            string? response = dbHandler.FetchCardsFromDataBase();
                            if (response != null)
                            {
                                sendRES(sock, 200, "OK", response);
                            }
                            else
                            {
                                sendRES(sock, 204, "No content", "The request was fine, but the user doesn't have any cards.");
                            }

                        }
                        else
                        {
                            sendRES(sock, 401, "Invalid", "Access token is missing or invalid");

                        }
                    }
                    else if (request.Path == "/stats")
                    {
                        if (dbHandler.AuthorizedUser())
                        {
                            if (dbHandler.currentUser != null)
                            {
                                AccountStats? accountStats = dbHandler.getAccountStats();
                                if (accountStats != null)
                                {
                                    sendRES(sock, 200, "OK", System.Text.Json.JsonSerializer.Serialize(accountStats));

                                }
                                else
                                {
                                    // Send bad request
                                    sendRES(sock, 400, "Bad Request", "The server did not understand the request.");
                                }

                            }
                        }
                        else
                        {
                            sendRES(sock, 401, "Invalid", "Access token is missing or invalid");
                        }

                    }
                    else if (request.Path == "/scoreboard")
                    {
                        if (dbHandler.AuthorizedUser())
                        {
                            if (dbHandler.currentUser != null)
                            {
                                string? response = dbHandler.getScoreboard();
                                if (response != null)
                                {
                                    sendRES(sock, 200, "OK", response);
                                }
                                else
                                {
                                    // Send bad response
                                    sendRES(sock, 400, "Bad Request", "The server did not understand the request.");
                                }
                            }
                        }
                        else
                        {
                            // unauthorized
                            sendRES(sock, 401, "Invalid", "Access token is missing or invalid");
                        }
                    }
                }

                // END GET//

                // PUT REQ //
                if (request.Method == Method.PUT)
                {
                    Console.WriteLine("PUT REQ");

                    if (request.Path == "/users/")
                    {
                        if (request.QueryParams.Count > 0)
                        {

                            Console.WriteLine(request.BodyMessage["Name"]);
                            if (dbHandler.AuthorizedUser())
                            {
                                Console.WriteLine(request.BodyMessage["Name"]);

                                AccountData accountData = new AccountData();
                                accountData.Username = request.QueryParams["username"];
                                accountData.Name = request.BodyMessage["Name"];
                                accountData.Bio = request.BodyMessage["Bio"];
                                accountData.Image = request.BodyMessage["Image"];
                                int returnCode = dbHandler.UpdateAccount(accountData);

                                if (returnCode == 1)
                                {
                                    sendRES(sock, 200, "OK", "User successfully updated!");
                                }
                                else if (returnCode == 0)
                                {
                                    sendRES(sock, 404, "Not found", "User not found.");
                                }
                                else
                                {
                                    // no.
                                }
                            }
                            else
                            {
                                sendRES(sock, 401, "Invalid", "Access token is missing or invalid!");
                                Console.WriteLine("Token invalid!");
                            }

                        }
                        else
                        {
                            // not enough params
                            Console.WriteLine("Not enough params.");

                        }
                    }
                    else
                    {
                        // unknow path
                        Console.WriteLine("Unknown endpoint");
                    }
                }

                // END PUT REQ



            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        public void sendRES(TcpClient sock, int code, string text, string body, string contentType = "application/json")
        {
            StreamWriter writer = new StreamWriter(sock.GetStream()) { AutoFlush = true };
            Response response = new Response(writer);
            response.ResponseCode = code;
            response.ResponseText = text;
            response.Content = body;
            response.Headers.Add("Content-Type", contentType);
            response.Process();
            writer.Close();
        }





    }
}
