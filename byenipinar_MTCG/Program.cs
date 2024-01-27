using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace byenipinar_MTCG
{
    class Program
    {
        static async Task Main()
        {
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            int port = 10001;
            Data data = new Data();
            data.DataAutomation();
            TcpListener tcpListener = new TcpListener(ipAddress, port);
            tcpListener.Start();
            Console.WriteLine($"Server started. Listening on {ipAddress}:{port}");

            while (true)
            {
                TcpClient client = await tcpListener.AcceptTcpClientAsync();

                // Starte die Verarbeitung des Clients in einem separaten Thread
                Task.Run(() => HandleClient(client));
            }
        }

        static async Task HandleClient(TcpClient client)
        {
            using (NetworkStream networkStream = client.GetStream())
            using (StreamReader reader = new StreamReader(networkStream))
            using (StreamWriter writer = new StreamWriter(networkStream))
            {
                StringBuilder requestBuilder = new StringBuilder();
                string line;

                // Lese die Anfragezeilen bis zum Leerzeichen
                while (!string.IsNullOrWhiteSpace(line = await reader.ReadLineAsync()))
                {
                    requestBuilder.AppendLine(line);
                }

                string fullRequest = requestBuilder.ToString();
                Console.WriteLine($"Received request: {fullRequest}");

                // Extrahiere den Anfrage-Body
                int contentLength = ExtractContentLength(fullRequest);
                char[] requestBody = new char[contentLength];
                if (contentLength > 0)
                {
                    await reader.ReadAsync(requestBody, 0, contentLength);
                }
                string jsonBody = new string(requestBody);

                // Jetzt hast du den vollständigen HTTP-Request und den JSON-Body
                Console.WriteLine($"JSON Body: {jsonBody}");
                string temp = fullRequest + jsonBody;
                // Erstelle das Request-Objekt mit dem JSON-Body
                Request HandleRequest = new Request(temp);

                byte[] responseData = Encoding.UTF8.GetBytes(HandleRequest.response);
                await networkStream.WriteAsync(responseData, 0, responseData.Length);
            }

            Console.WriteLine("Client disconnected");
        }

        // Hilfsmethode zur Extraktion des Content-Length-Headers
        private static int ExtractContentLength(string request)
        {
            const string contentLengthHeader = "Content-Length:";
            int index = request.IndexOf(contentLengthHeader, StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                int start = index + contentLengthHeader.Length;
                int end = request.IndexOf("\r\n", start, StringComparison.OrdinalIgnoreCase);
                if (end > start && int.TryParse(request.Substring(start, end - start).Trim(), out int length))
                {
                    return length;
                }
            }
            return 0;
        }
    }
}