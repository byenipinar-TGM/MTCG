using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using byenipinar_MTCG.GameClasses;
using System.Diagnostics;
using Newtonsoft.Json;

namespace byenipinar_MTCG
{
    public class Request
    {
        public string request { get; private set; }
        public string response { get; private set; }

        private const string adminToken = "admin-mtcgToken";

        public Data db { get; private set; }
        public DataBattleTrade dataBattleTrade { get; private set; }

        public Request()
        {
        }

        public Request(string Request)
        {
            this.db = new Data();
            this.dataBattleTrade = new DataBattleTrade();
            this.response = "";
            this.request = Request;
            HandleRequest();
        }

        private void HandleRequest()
        {
            Response responseMsg = new Response();
            try
            {
                string authenticationToken = db.ExtractAuthorizationToken(request);
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
                            var userObject = System.Text.Json.JsonSerializer.Deserialize<User>(jsonPayload);
                            Console.WriteLine(userObject);

                            this.db = new Data(userObject);
                            if (request.Contains("POST /users"))
                            {
                                if (!db.DoesUserExist(userObject.Username))
                                {
                                    db.AddUser();
                                    response = responseMsg.MessageFromResponse("users", 201);
                                }
                                else response = responseMsg.MessageFromResponse("users", 409);

                            }
                            else if (request.Contains("POST /sessions"))
                            {

                                if (db.VerifyUserCredentials(userObject))
                                {
                                    response = responseMsg.MessageFromResponse("sessions", 200) + "Token: " + userObject.Username + "-mtcgToken\r\n";
                                }
                                else
                                {
                                    response = responseMsg.MessageFromResponse("sessions", 401);
                                }
                            }
                        }
                        else if (request.Contains("POST /packages"))
                        {

                            if (authenticationToken.Length > 0)
                            {
                                if (authenticationToken == adminToken)
                                {
                                    var packagesJson = System.Text.Json.JsonSerializer.Deserialize<List<PackageOfCards>>(jsonPayload);
                                    this.db = new Data();

                                    db.SpeicherePackages(jsonPayload);
                                    db.SaveCardsFromRequest(jsonPayload);

                                    response = responseMsg.MessageFromResponse("packages", 201);
                                }
                                else response = responseMsg.MessageFromResponse("packages", 401);
                            }
                        }
                        else if (request.Contains("PUT /deck"))
                        {
                            if (db.TokenExist(authenticationToken))
                            {
                                List<string> cardcount = System.Text.Json.JsonSerializer.Deserialize<List<string>>(jsonPayload);
                                int zaehler = cardcount.Count;
                                bool ok = false;
                                if(zaehler != 4)
                                {
                                    response = responseMsg.MessageFromResponse("put_deck", 400);
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
                                        response = responseMsg.MessageFromResponse("put_deck", 200);
                                    }
                                } else response = responseMsg.MessageFromResponse("put_deck", 403);
                            }
                            else response = responseMsg.MessageFromResponse("put_deck", 401);
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
                                    response = responseMsg.MessageFromResponse("put_users", 200);
                                }
                                else
                                {
                                    response = responseMsg.MessageFromResponse("put_users", 401);
                                }
                            }
                            else
                            {
                                response = responseMsg.MessageFromResponse("put_users", 404);
                            }
                        }
                        else if (request.Contains("POST /tradings"))
                        {
                            string username = db.GetUsername(authenticationToken);

                            if (!db.TokenExist(authenticationToken))
                            {
                                response = responseMsg.MessageFromResponse("post_trade",401);
                                return;
                            }

                            var userObject = System.Text.Json.JsonSerializer.Deserialize<Trading>(jsonPayload);

                            if (!dataBattleTrade.UserHaveCard(username, userObject.CardToTrade))
                            {
                                response = responseMsg.MessageFromResponse("post_trade", 403);
                                return;
                            }

                            if (dataBattleTrade.DoesCardExistInTrading(userObject.CardToTrade))
                            {
                                response = responseMsg.MessageFromResponse("post_trade", 409);
                                return;
                            }

                            dataBattleTrade.InsertTrade(userObject.CardToTrade, userObject.Id, userObject.Type, userObject.MinimumDamage, username);
                            response = responseMsg.MessageFromResponse("post_trade", 201);

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

                            response = responseMsg.MessageFromResponse("transactions/packages", 200);
                        }
                        else
                        {
                            response = responseMsg.MessageFromResponse("transactions/packages", 403);
                        }
                    }
                    else
                    {
                        response = responseMsg.MessageFromResponse("transactions/packages", 404);
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
                            response = responseMsg.MessageFromResponse("cards", 200) + userCards + "\r\n";
                        }
                        else
                        {
                            response = responseMsg.MessageFromResponse("cards", 204);
                        }
                    }
                    else response = responseMsg.MessageFromResponse("cards", 401);
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
                            response = responseMsg.MessageFromResponse("get_deck", 200) + deck + "\r\n";
                        }
                        else
                        {
                            response = responseMsg.MessageFromResponse("get_deck", 204);
                        }
                    }
                    else response = responseMsg.MessageFromResponse("get_deck", 401);
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
                                response = responseMsg.MessageFromResponse("get_users", 200) + dataFromUser + "\r\n";
                        }
                        else
                        {
                            response = responseMsg.MessageFromResponse("get_users", 401);
                        }
                    }
                    else
                    {
                        response = responseMsg.MessageFromResponse("get_users", 404);
                    }
                }
                else if (request.Contains("GET /stats"))
                {
                    if (db.TokenExist(authenticationToken))
                    {
                        response = responseMsg.MessageFromResponse("get_stats",200) + db.GetUserStatisticsJson(authenticationToken) + "\r\n";
                    }
                    else
                    {
                        responseMsg.MessageFromResponse("get_stats", 401);
                    }
                }
                else if (request.Contains("GET /scoreboard"))
                {
                    if (db.TokenExist(authenticationToken))
                    {
                        List<User> scoreboard = db.GetScoreboard();
                        var filteredScoreboard = scoreboard.Select(u => new { u.Name, u.Wins, u.Losses, u.Elo }).ToList();
                        string jsonResult = JsonConvert.SerializeObject(filteredScoreboard, Formatting.Indented);
                        response = responseMsg.MessageFromResponse("get_scoreboard", 200) + jsonResult + "\r\n";
                    }
                    else
                    {
                        response = responseMsg.MessageFromResponse("get_scoreboard", 401);
                    }
                }
                else if (request.Contains("GET /tradings"))
                {
                    string trade = dataBattleTrade.GetTradingDataJson();
                    if (!db.TokenExist(authenticationToken))
                    {
                        response = responseMsg.MessageFromResponse("get_trade",401);
                    }
                    else
                    {
                        if (trade.Count() > 2)
                        {
                            response = responseMsg.MessageFromResponse("get_trade",200) + trade + "\n";
                        }
                        else response = responseMsg.MessageFromResponse("get_trade",205);
                    }
                }
                else if (request.Contains("DELETE /tradings"))
                {
                    string id = "";
                    int start = request.IndexOf("/tradings/");

                    id = ExtractIdFromRequest(request, "/tradings/");
                    Console.WriteLine("Extrahier: " + id);

                    if (!db.TokenExist(authenticationToken))
                    {
                        response = responseMsg.MessageFromResponse("del_trade",401);
                        return;
                    }

                    if (!dataBattleTrade.IdExist(id))
                    {
                        response = responseMsg.MessageFromResponse("del_trade", 404);
                        return;
                    }

                    string username = db.GetUsername(authenticationToken);

                    if (!dataBattleTrade.IdToUser(id, username))
                    {
                        response = responseMsg.MessageFromResponse("del_trade", 403);
                        return;
                    }

                    dataBattleTrade.DeleteTrade(id);
                    response = responseMsg.MessageFromResponse("del_trade", 200);

                }
                else if (request.Contains("POST /tradings/"))
                {
                    string id = "";
                    int start = request.IndexOf("/tradings/");

                    id = ExtractIdFromRequest(request, "/tradings/");
                    Console.WriteLine("Extrahier: " + id);

                    if (!db.TokenExist(authenticationToken))
                    {
                        response = responseMsg.MessageFromResponse("succesful_trade", 401);
                        return;
                    }

                    if (!dataBattleTrade.IdExist(id))
                    {
                        response = responseMsg.MessageFromResponse("succesful_trade", 404);
                        return;
                    }

                    string username = db.GetUsername(authenticationToken);

                    if (dataBattleTrade.IdToUser(id, username))
                    {
                        response = responseMsg.MessageFromResponse("succesful_trade", 410);
                        return;
                    }

                    string extracted = dataBattleTrade.ExtractCardId(request);

                    if (!dataBattleTrade.UserHaveCard(username, extracted))
                    {
                        response = responseMsg.MessageFromResponse("succesful_trade", 403);
                        return;
                    }

                    string cardtotrade = dataBattleTrade.TradebyID(id);

                    if (!dataBattleTrade.CheckDamage(extracted, cardtotrade))
                    {
                        response = responseMsg.MessageFromResponse("succesful_trade", 411);
                        return;
                    }

                    dataBattleTrade.DeleteTradeWithId(id);

                    int card1packageid = dataBattleTrade.GetPackageIdFromCardId(cardtotrade);
                    int card2packageid = dataBattleTrade.GetPackageIdFromCardId(dataBattleTrade.ExtractCardId(request));

                    dataBattleTrade.UpdatePackageIdForCard(cardtotrade, card2packageid);
                    dataBattleTrade.UpdatePackageIdForCard(dataBattleTrade.ExtractCardId(request), card1packageid);

                    response = responseMsg.MessageFromResponse("succesful_trade", 200);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static string ExtractIdFromRequest(string request, string resourcePath)
        {
            int start = request.IndexOf(resourcePath);

            if (start != -1)
            {
                int idStart = start + resourcePath.Length;
                int spaceIndex = request.IndexOf(' ', idStart);

                if (spaceIndex != -1)
                {
                    return request.Substring(idStart, spaceIndex - idStart);
                }
            }

            return null;
        }

    }
}
