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

        private readonly Dictionary<int, string> messages;

        public Response(string path)
        {
            if (path == "users")
            {
                messages = new Dictionary<int, string>
                {
                    { 201, "HTTP/1.1 201 Created\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Benutzer wurde erfolgreich erstellt\"}\r\n" },
                    { 409, "HTTP/1.1 409 Conflict\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Benutzer mit dem selben Username existiert bereits\"}\r\n" }
                };
            }
            else if (path == "sessions")
            {
                messages = new Dictionary<int, string>
                {
                    { 200, "HTTP/1.1 200 OK\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Benutzer login erfolgreich\"}\r\n" },
                    { 401, "HTTP/1.1 401 Unauthorized\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Ungültiger Username/Password angegeben.\"}\r\n" },
                };
            }
            else if (path == "packages")
            {
                messages = new Dictionary<int, string>
                {
                    { 201, "HTTP/1.1 201 Created\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Package and cards successfully created\"}\r\n" },
                    { 401, "HTTP/1.1 401 Unauthorized\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Access token is missing or invalid\"}\r\n" },
                    { 403, "HTTP/1.1 403 Forbidden\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Provided user is not \"admin\"\"}\r\n" },
                    { 409, "HTTP/1.1 409 Conflict\r\nContent-Type: application/json\r\n\r\n{\"message\": \"At least one card in the packages already exists\"}\r\n" },
                };
            }
            else if (path == "transactions/packages")
            {
                messages = new Dictionary<int, string>
                {
                    { 200, "HTTP/1.1 200 OK\r\nContent-Type: application/json\r\n\r\n{\"message\": \"A package has been successfully bought\"}\r\n" },
                    { 401, "HTTP/1.1 401 Unauthorized\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Access token is missing or invalid\"}\r\n" },
                    { 403, "HTTP/1.1 403 No Money\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Not enough money for buying a card package\"}\r\n" },
                    { 404, "HTTP/1.1 404 No Package\r\nContent-Type: application/json\r\n\r\n{\"message\": \"No card package available for buying\"}\r\n" },
                };
            }
            else if (path == "cards")
            {
                messages = new Dictionary<int, string>
                {
                    { 200, "HTTP/1.1 200 OK\r\nContent-Type: application/json\r\n\r\n{\"message\": \"The user has cards, the response contains these\"}\r\n" },
                    { 204, "HTTP/1.1 204 No Card\r\nContent-Type: application/json\r\n\r\n{\"message\": \"The request was fine, but the user doesn't have any cards\"}\r\n" },
                    { 401, "HTTP/1.1 401 Token Error\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Access token is missing or invalid\"}\r\n" }
                };
            }
        }

        public byte[] GenerateResponseData()
        {
            string response = $"{StatusCode}\r\n{ContentType}\r\n\r\n{Content}";
            return Encoding.UTF8.GetBytes(response);
        }

        public string GetResponseMessage(int statusCode)
        {
            if (messages.TryGetValue(statusCode, out string message))
            {
                return message;
            }

            return $"HTTP/1.1 {statusCode} \r\nContent-Type: text/plain\r\n\r\nUnknown Status Code";
        }
    }
}
