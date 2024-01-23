using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using byenipinar_MTCG.GameClasses;
using System.Diagnostics;

namespace byenipinar_MTCG
{
    public class Request
    {
        public string request { get; private set; }
        public string response { get; private set; }

        private const string adminToken = "admin-mtcgToken";

        public Data db { get; private set; }

        public Request(string Request)
        {
            this.db = new Data();
            this.response = "";
            this.request = Request;
            HandleRequest();
        }

        private void HandleRequest()
        {
            Response responseMsg = new Response();
            try
            {
                string authenticationToken = ExtractAuthorizationToken(request);
                int bodyStartIndex = this.request.IndexOf("{");

                //string parsedAuthenticationToken = token;

                if (request.Contains("POST /packages") || request.Contains("PUT /deck")) bodyStartIndex = request.IndexOf("[");

                if (bodyStartIndex >= 0)
                {
                    // Find the end of the HTTP headers
                    int headerEndIndex = request.IndexOf("\r\n\r\n") + 4;

                    // Extract the Content-Length header value
                    string contentLengthHeader = request.Substring(request.IndexOf("Content-Length:") + 15);
                    int contentLength = int.Parse(contentLengthHeader.Substring(0, contentLengthHeader.IndexOf("\r\n")));

                    // Extract the JSON payload based on Content-Length
                    var jsonPayload = request.Substring(bodyStartIndex, contentLength);
                    Console.WriteLine("JSON Payload:" + "\n" + jsonPayload);

                    try
                    {
                        if (request.Contains("POST /users") || request.Contains("POST /sessions"))
                        {
                            var userObject = JsonSerializer.Deserialize<User>(jsonPayload);
                            Console.WriteLine(userObject);

                            this.db = new Data(userObject);
                            if (request.Contains("POST /users"))
                            {
                                if (!db.DoesUserExist(userObject.Username))
                                {
                                    db.AddUser();
                                    response = responseMsg.GetResponseMessage("users", 201);
                                }
                                else response = responseMsg.GetResponseMessage("users", 409);

                            }
                            else if (request.Contains("POST /sessions"))
                            {

                                if (db.VerifyUserCredentials(userObject))
                                {
                                    response = responseMsg.GetResponseMessage("sessions", 200) + "Token: " + userObject.Username + "-mtcgToken\r\n";
                                }
                                else
                                {
                                    response = responseMsg.GetResponseMessage("sessions", 401);
                                }
                            }
                        }
                        else if (request.Contains("POST /packages"))
                        {

                            if (authenticationToken.Length > 0)
                            {
                                if (authenticationToken == adminToken)
                                {
                                    var packagesJson = JsonSerializer.Deserialize<List<PackageOfCards>>(jsonPayload);
                                    this.db = new Data();

                                    db.SpeicherePackages(jsonPayload);
                                    db.SaveCardsFromRequest(jsonPayload);

                                    response = responseMsg.GetResponseMessage("packages", 201);
                                }
                                else response = responseMsg.GetResponseMessage("packages", 401);
                            }
                        }
                        else if (request.Contains("PUT /deck"))
                        {
                            if (db.TokenExist(authenticationToken))
                            {
                                List<string> cardcount = JsonSerializer.Deserialize<List<string>>(jsonPayload);
                                int zaehler = cardcount.Count;
                                bool ok = false;
                                if(zaehler != 4)
                                {
                                    response = responseMsg.GetResponseMessage("put_deck", 400);
                                }
                                else if(zaehler == 4)
                                {
                                    for (int i = 0; i < 4; i++)
                                    {
                                        if (db.CardacquireUser(authenticationToken, cardcount[i]) == false) ok = true;
                                    }

                                    if (!ok)
                                    {
                                        db.DeleteDeckFromUser(db.GetUsername(authenticationToken));
                                        for (int i = 0; i < 4; i++)
                                        {
                                            db.AddDeck(db.GetUsername(authenticationToken), cardcount[i]);
                                        }
                                        response = responseMsg.GetResponseMessage("put_deck", 200);
                                    }
                                } else response = responseMsg.GetResponseMessage("put_deck", 403);
                            }
                            else response = responseMsg.GetResponseMessage("put_deck", 401);
                        }
                        else if (request.Contains("PUT /users"))
                        {
                            string user = "";

                            user = db.ExtractUsernameFromRequest(request);
                            Console.WriteLine(user);

                            if (db.DoesUserExist(user))
                            {
                                if (db.IsTokenValid(authenticationToken, user) || authenticationToken == adminToken)
                                {
                                    db.UpdateUserData(user, jsonPayload);
                                    response = responseMsg.GetResponseMessage("put_users", 200);
                                }
                                else
                                {
                                    response = responseMsg.GetResponseMessage("put_users", 401);
                                }
                            }
                            else
                            {
                                response = responseMsg.GetResponseMessage("put_users", 404);
                            }
                        }
                        else if (request.Contains("POST /packages"))
                        {
                            // Handle POST /packages
                        }
                        else if (request.Contains("GET /cards"))
                        {
                            // Handle GET /cards
                        }
                        else if (request.Contains("GET /deck"))
                        {
                            // Handle GET /deck
                        }
                        else if (request.Contains("PUT /users/"))
                        {
                            // Handle PUT /deck
                        }
                        else if (request.Contains("GET /stats"))
                        {
                            // Handle GET /stats
                        }
                        else if (request.Contains("GET /scoreboard"))
                        {
                            // Handle GET /scoreboard
                        }
                        else if (request.Contains("POST /battles"))
                        {
                            // Handle POST /battles
                        }
                        else if (request.Contains("GET /tradings"))
                        {
                            // Handle GET /tradings
                        }
                        else if (request.Contains("POST /tradings"))
                        {
                            // Handle POST /tradings
                        }
                        else if (request.Contains("DELETE /tradings/"))
                        {
                            // Handle DELETE /tradings/{tradingdealid}
                        }
                        else if (request.Contains("POST /tradings/"))
                        {
                            // Handle POST /tradings/{tradingdealid}
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
                else if (request.Contains("POST /transactions/packages"))
                {
                    
                    List<int> packagelist = new List<int>();
                    int coins;
                    string username = db.GetUsername(authenticationToken);


                    coins = db.GetUserCoins(username); //stimmt

                    packagelist = db.GetAvailablePackages(); //stimmt

                    if (packagelist.Count > 0)
                    {
                        if (coins >= 5)
                        {
                            db.ChangeUserCoins(username, coins - 5);

                            db.BuyPackage(username, packagelist[0]);

                            response = responseMsg.GetResponseMessage("transactions/packages", 200);
                        }
                        else
                        {
                            response = responseMsg.GetResponseMessage("transactions/packages", 403);
                        }
                    }
                    else
                    {
                        response = responseMsg.GetResponseMessage("transactions/packages", 404);
                    }
                }
                else if (request.Contains("GET /cards"))
                {
                    
                    if (db.TokenExist(authenticationToken))
                    {
                        Console.WriteLine(db.TokenExist(authenticationToken));
                        string userCards = db.GetCardDataJson(authenticationToken);

                        if (userCards.Length > 2)
                        {
                            response = responseMsg.GetResponseMessage("cards", 200) + userCards + "\r\n";
                        }
                        else
                        {
                            response = responseMsg.GetResponseMessage("cards", 204);
                        }
                    }
                    else response = responseMsg.GetResponseMessage("cards", 401);
                }
                else if (request.Contains("GET /deck"))
                {
                    
                    if (db.TokenExist(authenticationToken))
                    {
                        string deck = "";

                        if (request.Contains("format=plain"))
                        {
                            deck = db.GetCardsFromDeck(authenticationToken, false);
                        }
                        else
                        {
                            deck = db.GetCardsFromDeck(authenticationToken, true);
                        }

                        if (deck.Length > 2)
                        {
                            response = responseMsg.GetResponseMessage("get_deck", 200) + deck + "\r\n";
                        }
                        else
                        {
                            response = responseMsg.GetResponseMessage("get_deck", 204);
                        }
                    }
                    else response = responseMsg.GetResponseMessage("get_deck", 401);
                }
                else if (request.Contains("GET /users"))
                {
                    string user = "";

                    user = db.ExtractUsernameFromRequest(request);
                    Console.WriteLine(user);

                    if (db.DoesUserExist(user))
                    {
                        string usernew = user + "-mtcgToken";
                        if(db.IsTokenValid(authenticationToken, user) || authenticationToken == adminToken)
                        {
                                string dataFromUser = db.GetUserDataJson(user);
                                response = responseMsg.GetResponseMessage("get_users", 200) + dataFromUser + "\r\n";
                        }
                        else
                        {
                            response = responseMsg.GetResponseMessage("get_users", 401);
                        }
                    }
                    else
                    {
                        response = responseMsg.GetResponseMessage("get_users", 404);
                    }
                }
                else if (request.Contains("GET /stats"))
                {
                    if (db.TokenExist(authenticationToken))
                    {
                        response = responseMsg.GetResponseMessage("get_stats",200) + db.GetUserStatisticsJson(authenticationToken) + "\r\n";
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }



        private string ExtractAuthorizationToken(string request)
        {
            const string authorizationHeader = "Authorization: Bearer ";
            int startIndex = request.IndexOf(authorizationHeader);

            if (startIndex >= 0)
            {
                startIndex += authorizationHeader.Length;
                int endIndex = request.IndexOf("\r\n", startIndex);

                if (endIndex >= 0)
                {
                    return request.Substring(startIndex, endIndex - startIndex);
                }
            }

            return "";
        }

    }
}
