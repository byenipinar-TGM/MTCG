using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace byenipinar_MTCG
{
    public class Response
    {
        public string StatusCode { get; set; } = "HTTP/1.1 200 OK";
        public string ContentType { get; set; } = "Content-Type: text/plain";
        public string Content { get; set; } = "Hello, world!";

        private readonly Dictionary<string, string> messages;

        public Response()
        {
            messages = new Dictionary<string, string>
            {
                { "users_201", "HTTP/1.1 201 Erstellt\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Benutzer wurde erfolgreich erstellt\"}\r\n" },
                { "users_409", "HTTP/1.1 409 Konflikt\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Benutzer mit demselben Benutzernamen existiert bereits\"}\r\n" },

                { "sessions_200", "HTTP/1.1 200 Erfolgreich\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Benutzerlogin erfolgreich\"}\r\n" },
                { "sessions_401", "HTTP/1.1 401 Nicht authorisiert\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Ungültiger Benutzername/Passwort angegeben.\"}\r\n" },

                { "packages_201", "HTTP/1.1 201 Erstellt\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Paket und Karten erfolgreich erstellt\"}\r\n" },
                { "packages_401", "HTTP/1.1 401 Authorisiert\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Access-Token fehlt oder ist ungültig\"}\r\n" },
                { "packages_403", "HTTP/1.1 403 Verboten\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Der bereitgestellte Benutzer ist nicht 'admin'\"}\r\n" },
                { "packages_409", "HTTP/1.1 409 Konflikt\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Mindestens eine Karte im Paket existiert bereits\"}\r\n" },

                { "transactions/packages_200", "HTTP/1.1 200 Erfolgreich\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Ein Paket wurde erfolgreich gekauft\"}\r\n" },
                { "transactions/packages_401", "HTTP/1.1 401 Authorisiert\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Access-Token fehlt oder ist ungültig\"}\r\n" },
                { "transactions/packages_403", "HTTP/1.1 403 Kein Geld\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Nicht genug Geld, um ein Kartenpaket zu kaufen\"}\r\n" },
                { "transactions/packages_404", "HTTP/1.1 404 Kein Paket\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Kein Kartenpaket verfügbar zum Kauf\"}\r\n" },

                { "cards_200", "HTTP/1.1 200 Erfolgreich\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Der Benutzer hat Karten, die Antwort enthält diese\"}\r\n" },
                { "cards_204", "HTTP/1.1 204 Keine Karte\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Die Anfrage war in Ordnung, aber der Benutzer hat keine Karten\"}\r\n" },
                { "cards_401", "HTTP/1.1 401 Token Error\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Access-Token fehlt oder ist ungültig\"}\r\n" },

                { "get_deck_200", "HTTP/1.1 200 Erfolgreich\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Das Deck enthält Karten, die Antwort enthält diese\"}\r\n" },
                { "get_deck_204", "HTTP/1.1 204 Keine Karte\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Die Anfrage war in Ordnung, aber das Deck enthält keine Karten\"}\r\n" },
                { "get_deck_401", "HTTP/1.1 401 Token Error\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Access-Token fehlt oder ist ungültig\"}\r\n" },

                { "put_deck_200", "HTTP/1.1 200 Erfolgreich\r\nContent-Type: application/json\r\n\r\n{\"message\": \"The deck has cards, the response contains these\"}\r\n" },
                { "put_deck_204", "HTTP/1.1 204 Keine Karte\r\nContent-Type: application/json\r\n\r\n{\"message\": \"The request was fine, but the deck doesn't have any cards\"}\r\n" },
                { "put_deck_401", "HTTP/1.1 401 Token Error\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Access token is missing or invalid\"}\r\n" },

                { "get_users_200", "HTTP/1.1 200 Erfolgreich\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Data successfully retrieved\"}\r\n" },
                { "get_users_401", "HTTP/1.1 401 Token Error\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Access token is missing or invalid\"}\r\n" },
                { "get_users_404", "HTTP/1.1 404 Nicht Gefunden\r\nContent-Type: application/json\r\n\r\n{\"message\": \"User not found.\"}\r\n" },

                { "put_users_200", "HTTP/1.1 200 Erfolgreich\r\nContent-Type: application/json\r\n\r\n{\"message\": \"User sucessfully updated.\"}\r\n" },
                { "put_users_401", "HTTP/1.1 401 Token Error\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Access token is missing or invalid\"}\r\n" },
                { "put_users_404", "HTTP/1.1 404 Nicht Gefunden\r\nContent-Type: application/json\r\n\r\n{\"message\": \"User not found.\"}\r\n" },

                { "get_stats_200", "HTTP/1.1 200 Erfolgreich\r\nContent-Type: application/json\r\n\r\n{\"message\": \"The stats could be retrieved successfully.\"}\r\n" },
                { "get_stats_401", "HTTP/1.1 401 Token Error\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Access token is missing or invalid\"}\r\n" },

                { "get_scoreboard_200", "HTTP/1.1 200 Erfolgreich\r\nContent-Type: application/json\r\n\r\n{\"message\": \"The scoreboard could be retrieved successfully.\"}\r\n" },
                { "get_scoreboard_401", "HTTP/1.1 401 Token Error\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Access token is missing or invalid\"}\r\n" },

                { "get_trade_200", "HTTP/1.1 200 Erfolgreich\r\nContent-Type: application/json\r\n\r\n{\"message\": \"There are trading deals available, the response contains these.\"}\r\n" },
                { "get_trade_205", "HTTP/1.1 205 Erfolgreich\r\nContent-Type: application/json\r\n\r\n{\"message\": \"The request was fine, but there are no trading deals available.\"}\r\n" },
                { "get_trade_401", "HTTP/1.1 401 Token Error\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Access token is missing or invalid.\"}\r\n" },

                { "post_trade_201", "HTTP/1.1 201 Trade Success\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Trading deal successfully created.\"}\r\n" },
                { "post_trade_401", "HTTP/1.1 401 Token Error\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Access token is missing or invalid.\"}\r\n" },
                { "post_trade_403", "HTTP/1.1 403 Falscher Trade\r\nContent-Type: application/json\r\n\r\n{\"message\": \"The deal contains a card that is not owned by the user or locked in the deck.\"}\r\n" },
                { "post_trade_409", "HTTP/1.1 409 Existiert bereits\r\nContent-Type: application/json\r\n\r\n{\"message\": \"A deal with this deal ID already exists..\"}\r\n" },

                { "del_trade_200", "HTTP/1.1 200 Erfolgreich\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Trading deal successfully deleted.\"}\r\n" },
                { "del_trade_401", "HTTP/1.1 401 Token Error\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Access token is missing or invalid.\"}\r\n" },
                { "del_trade_403", "HTTP/1.1 403 Falscher Trade\r\nContent-Type: application/json\r\n\r\n{\"message\": \"The deal contains a card that is not owned by the user.\"}\r\n" },
                { "del_trade_404", "HTTP/1.1 404 ID nicht gefunden\r\nContent-Type: application/json\r\n\r\n{\"message\": \"The provided deal ID was not found.\"}\r\n" },
                { "del_trade_409", "HTTP/1.1 409 Existiert bereits\r\nContent-Type: application/json\r\n\r\n{\"message\": \"A deal with this deal ID already exists.\"}\r\n" },

                { "succesful_trade_200", "HTTP/1.1 200 Erfolgreich\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Trading deal successfully executed.\"}\r\n" },
                { "succesful_trade_401", "HTTP/1.1 401 Token Error\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Access token is missing or invalid.\"}\r\n" },
                { "succesful_trade_403", "HTTP/1.1 403 Falscher Trade\r\nContent-Type: application/json\r\n\r\n{\"message\": \"The offered card is not owned by the user, or the requirements are not met (Type, MinimumDamage), or the offered card is locked in the deck.\"}\r\n" },
                { "succesful_trade_404", "HTTP/1.1 404 ID nicht Gefunden\r\nContent-Type: application/json\r\n\r\n{\"message\": \"The provided deal ID was not found.\"}\r\n" },
                { "succesful_trade_410", "HTTP/1.1 410 User Trade Fehlgeschlagen\r\nContent-Type: application/json\r\n\r\n{\"message\": \"The User wanted to Trade with himself.\"}\r\n" },
                { "succesful_trade_411", "HTTP/1.1 411 Zu Wenig Damage\r\nContent-Type: application/json\r\n\r\n{\"message\": \"The Damage of the Provided Card is too low.\"}\r\n" }




            };

            // Standardfall
            if (!messages.TryGetValue($"", out _))
            {
                messages["default"] = "HTTP/1.1 500 Internal Server Error\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Unerwarteter Fehler\"}\r\n";
            }
        }

        public byte[] GenerateResponseData()
        {
            string response = $"{StatusCode}\r\n{ContentType}\r\n\r\n{Content}";
            return Encoding.UTF8.GetBytes(response);
        }

        public string GetResponseMessage(string path,int statusCode)
        {
            if (messages.TryGetValue($"{path}_{statusCode}", out string message))
            {
                return message;
            }

            return $"HTTP/1.1 {statusCode} \r\nContent-Type: text/plain\r\n\r\nUnknown Status Code";
        }
    }
}
