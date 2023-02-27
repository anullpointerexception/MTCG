﻿using MTCG.DAL;
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
                                sendRES(sock, 201, "OK", "User successfully registered!");

                            }
                            else
                            {
                                Console.WriteLine("fail");
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

                            }
                            else
                            {
                                sendRES(sock, 401, "Unauthorized", "Invalid Username/Password");
                            }
                        }
                    }
                    {

                    }
                }

                if (request.Method == Method.GET)
                {
                    Console.WriteLine("GET REQ");
                    if (request.Path == "/users/")
                    {
                        if (request.QueryParams.Count > 0)
                        {
                            if (dbHandler.AuthorizedUser(request.QueryParams["username"]))
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
                                else
                                {
                                    sendRES(sock, 401, "Invalid", "Access token is missing or invalid");
                                    Console.WriteLine("Mismatch!");
                                }
                            }
                            else
                            {
                                Console.WriteLine("auth failed");
                            }

                        }
                        else
                        {
                            Console.WriteLine("not enough query params");
                        }
                        {

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
                            if (dbHandler.AuthorizedUser(request.QueryParams["username"]))
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
