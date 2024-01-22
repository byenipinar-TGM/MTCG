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
                { "users_201", "HTTP/1.1 201 Created\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Benutzer wurde erfolgreich erstellt\"}\r\n" },
                { "users_409", "HTTP/1.1 409 Conflict\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Benutzer mit demselben Benutzernamen existiert bereits\"}\r\n" },

                { "sessions_200", "HTTP/1.1 200 OK\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Benutzerlogin erfolgreich\"}\r\n" },
                { "sessions_401", "HTTP/1.1 401 Unauthorized\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Ungültiger Benutzername/Passwort angegeben.\"}\r\n" },

                { "packages_201", "HTTP/1.1 201 Created\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Paket und Karten erfolgreich erstellt\"}\r\n" },
                { "packages_401", "HTTP/1.1 401 Unauthorized\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Access-Token fehlt oder ist ungültig\"}\r\n" },
                { "packages_403", "HTTP/1.1 403 Forbidden\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Der bereitgestellte Benutzer ist nicht 'admin'\"}\r\n" },
                { "packages_409", "HTTP/1.1 409 Conflict\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Mindestens eine Karte im Paket existiert bereits\"}\r\n" },

                { "transactions/packages_200", "HTTP/1.1 200 OK\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Ein Paket wurde erfolgreich gekauft\"}\r\n" },
                { "transactions/packages_401", "HTTP/1.1 401 Unauthorized\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Access-Token fehlt oder ist ungültig\"}\r\n" },
                { "transactions/packages_403", "HTTP/1.1 403 No Money\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Nicht genug Geld, um ein Kartenpaket zu kaufen\"}\r\n" },
                { "transactions/packages_404", "HTTP/1.1 404 No Package\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Kein Kartenpaket verfügbar zum Kauf\"}\r\n" },

                { "cards_200", "HTTP/1.1 200 OK\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Der Benutzer hat Karten, die Antwort enthält diese\"}\r\n" },
                { "cards_204", "HTTP/1.1 204 No Card\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Die Anfrage war in Ordnung, aber der Benutzer hat keine Karten\"}\r\n" },
                { "cards_401", "HTTP/1.1 401 Token Error\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Access-Token fehlt oder ist ungültig\"}\r\n" },

                { "get_deck_200", "HTTP/1.1 200 OK\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Das Deck enthält Karten, die Antwort enthält diese\"}\r\n" },
                { "get_deck_204", "HTTP/1.1 204 No Card\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Die Anfrage war in Ordnung, aber das Deck enthält keine Karten\"}\r\n" },
                { "get_deck_401", "HTTP/1.1 401 Token Error\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Access-Token fehlt oder ist ungültig\"}\r\n" },
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
