using byenipinar_MTCG.GameClasses;
using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace byenipinar_MTCG
{
    public class DataBattleTrade
    {
        private static readonly object clientLock = new object();
        private static int connectedClientCount = 0;
        private static SemaphoreSlim semaphore = new SemaphoreSlim(0, 2);

        private static string username1;
        private static string username2;

        private static string getConnectionString()
        {
            return "Host=" + "localhost" + ";Username=" + "postgres" + ";Password=" + "Pass2020!" + ";Database=" + "MTCG";
        }

        private static List<(string, double)> user1Cards;
        private static List<(string, double)> user2Cards;

        private string connectionString = "Host=localhost;Database=MTCG;Username=postgres;Password=Pass2020!";

        public DataBattleTrade() { }

        public string GetTradingDataJson()
        {
            List<Trading> serializedTrades = new List<Trading>();

            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                using (NpgsqlCommand command = new NpgsqlCommand("SELECT id, cardtotrade, card_type, minimumdamage, username FROM tradings;", connection))
                {

                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Trading serializedTrade = new Trading
                            {
                                Id = reader.GetString(0),
                                CardToTrade = reader.GetString(1),
                                Type = reader.GetString(2),
                                MinimumDamage = reader.GetDouble(3)
                            };

                            serializedTrades.Add(serializedTrade);
                        }
                    }
                }

                connection.Close();
            }

            string jsonResult = System.Text.Json.JsonSerializer.Serialize(serializedTrades, new JsonSerializerOptions { WriteIndented = true });

            Console.WriteLine(jsonResult);

            return jsonResult;
        }

        public bool IdExist(string id)
        {
            bool exists = false;

            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    using (NpgsqlCommand command = new NpgsqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandType = CommandType.Text;
                        command.CommandText = "SELECT id FROM tradings WHERE id = @Id";

                        command.Parameters.AddWithValue("@Id", id);

                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            exists = reader.Read();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Fehlerbehandlung bei einer Ausnahme
                Console.WriteLine("Fehler: " + ex.Message);
            }

            return exists;
        }


        public bool IdToUser(string id, string username)
        {
            bool exists = false;

            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    using (NpgsqlCommand command = new NpgsqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandType = CommandType.Text;
                        command.CommandText = "SELECT id FROM tradings WHERE id = @id AND username = @username";

                        command.Parameters.AddWithValue("@id", id);
                        command.Parameters.AddWithValue("@username", username);

                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            exists = reader.Read();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Fehlerbehandlung bei einer Ausnahme
                Console.WriteLine("Fehler: " + ex.Message);
            }

            return exists;
        }

        public bool UserHaveCard(string username, string cardId)
        {
            bool hasResult = false;

            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    using (NpgsqlCommand command = new NpgsqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandType = CommandType.Text;
                        command.CommandText = "SELECT cards.id FROM user_packages INNER JOIN cards ON user_packages.package_id = cards.package_id WHERE user_packages.username = @username AND cards.id = @cardId";

                        command.Parameters.AddWithValue("@username", username);
                        command.Parameters.AddWithValue("@cardId", cardId);

                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            hasResult = reader.Read();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Fehlerbehandlung bei einer Ausnahme
                Console.WriteLine("Fehler: " + ex.Message);
            }

            return hasResult;
        }

        public string ExtractCardId(string httpRequest)
        {
            int startIndex = httpRequest.IndexOf("\"") + 1;

            if (startIndex >= 0)
            {
                int endIndex = httpRequest.IndexOf("\"", startIndex);

                if (endIndex > startIndex)
                {
                    return httpRequest.Substring(startIndex, endIndex - startIndex);
                }
            }

            return null;
        }


        public string TradebyID(string tradingId)
        {
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    using (NpgsqlCommand command = new NpgsqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandType = CommandType.Text;
                        command.CommandText = "SELECT cardtotrade FROM tradings WHERE id = @tradingId";
                        command.Parameters.AddWithValue("@tradingId", tradingId);

                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return reader.GetString(0);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Fehlerbehandlung bei einer Ausnahme
                Console.WriteLine("Fehler: " + ex.Message);
            }

            // Return null if no data found or an exception occurred
            return null;
        }

        public double GetCardDamageById(string cardId)
        {
            double cardDamage = 0.0;

            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    using (NpgsqlCommand command = new NpgsqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandType = CommandType.Text;
                        command.CommandText = "SELECT damage FROM cards WHERE id = @cardId";
                        command.Parameters.AddWithValue("@cardId", cardId);

                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                cardDamage = reader.GetDouble(0);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Fehlerbehandlung bei einer Ausnahme
                Console.WriteLine("Fehler: " + ex.Message);
            }

            // Rückgabe des Karten-Schadens
            return cardDamage;
        }

        public double GetMinimumDamage(string cardId)
        {
            double cardDamage = 0.0;

            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    using (NpgsqlCommand command = new NpgsqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandType = CommandType.Text;
                        command.CommandText = "SELECT minimumdamage FROM tradings WHERE id = @tradingId;";
                        command.Parameters.AddWithValue("@cardId", cardId);

                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                cardDamage = reader.GetDouble(0);
                            }
                        }
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                // Fehlerbehandlung bei einer Ausnahme
                Console.WriteLine("Fehler: " + ex.Message);
            }

            // Rückgabe des Karten-Schadens
            return cardDamage;
        }

        public bool CheckDamage(string cardId, string tradingId)
        {
            double cardDamage = GetCardDamageById(cardId);
            double minimumDamageInOffer = GetMinimumDamage(tradingId);

            return cardDamage >= minimumDamageInOffer;
        }

        public void DeleteTradeWithId(string tradeId)
        {
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    using (NpgsqlCommand command = new NpgsqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandType = CommandType.Text;

                        // DELETE-Anweisung, um den Handel mit der angegebenen ID zu löschen
                        command.CommandText = "DELETE FROM tradings WHERE id = @tradeId";

                        command.Parameters.AddWithValue("@tradeId", tradeId);

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                // Fehlerbehandlung bei einer Ausnahme
                Console.WriteLine("Fehler: " + ex.Message);
            }
        }

        public int GetPackageIdFromCardId(string cardId)
        {
            int packageId = 0; // Initialisierung auf den Standardwert 0

            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    using (NpgsqlCommand command = new NpgsqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandType = CommandType.Text;

                        // SQL-Abfrage, um die package_id für die angegebene Karten-ID zu erhalten
                        command.CommandText = "SELECT package_id FROM cards WHERE id = @cardId";

                        command.Parameters.AddWithValue("@cardId", cardId);

                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Lesen und Zuweisen der package_id
                                packageId = reader.GetInt32(0);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Fehlerbehandlung bei einer Ausnahme
                Console.WriteLine("Fehler: " + ex.Message);
            }

            // Rückgabe der Paket-ID
            return packageId;
        }

        public bool UpdatePackageIdForCard(string cardIdToUpdate, int newPackageId)
        {
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    using (NpgsqlCommand command = new NpgsqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandType = CommandType.Text;

                        // SQL-Abfrage, um die package_id für die angegebene Karten-ID zu aktualisieren
                        command.CommandText = "UPDATE cards SET package_id = @newPackageId WHERE id = @cardIdToUpdate";

                        command.Parameters.AddWithValue("@newPackageId", newPackageId);
                        command.Parameters.AddWithValue("@cardIdToUpdate", cardIdToUpdate);

                        // Ausführen der Aktualisierungsabfrage
                        int rowsAffected = command.ExecuteNonQuery();

                        // Überprüfen, ob die Aktualisierung erfolgreich war (mindestens eine Zeile betroffen)
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                // Fehlerbehandlung bei einer Ausnahme
                Console.WriteLine("Fehler: " + ex.Message);
                return false;
            }
        }

        public bool DoesCardExistInTrading(string cardId)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                using (NpgsqlCommand command = new NpgsqlCommand("SELECT id FROM tradings WHERE cardtotrade = @cardId;", connection))
                {
                    command.Parameters.AddWithValue("@cardId", cardId);

                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        bool exists = reader.Read();

                        connection.Close();

                        return exists;
                    }
                }
            }
        }

        public void InsertTrade(string cardToTrade, string id, string cardType, double minimumDamage, string username)
        {
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    using (NpgsqlCommand command = new NpgsqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandType = CommandType.Text;

                        // SQL-Abfrage zum Einfügen eines Handels in die Tabelle "tradings"
                        command.CommandText = "INSERT INTO tradings (id, cardtotrade, card_type, minimumdamage, username) " +
                                              "VALUES (@Id, @cardToTrade, @cardType, @minimumDamage, @username)";

                        // Hinzufügen der Parameter zur Abfrage
                        command.Parameters.AddWithValue("@Id", id);
                        command.Parameters.AddWithValue("@cardToTrade", cardToTrade);
                        command.Parameters.AddWithValue("@cardType", cardType);
                        command.Parameters.AddWithValue("@minimumDamage", minimumDamage);
                        command.Parameters.AddWithValue("@username", username);

                        // Ausführen der Datenbankeinfügeabfrage
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                // Fehlerbehandlung bei einer Ausnahme
                Console.WriteLine("Fehler: " + ex.Message);
            }
        }

        public void DeleteTrade(string tradeId)
        {
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    using (NpgsqlCommand command = new NpgsqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandType = CommandType.Text;

                        // SQL-Abfrage zum Löschen eines Handels aus der Tabelle "tradings"
                        command.CommandText = "DELETE FROM tradings WHERE id = @tradeId";

                        // Hinzufügen des Parameters zur Abfrage
                        command.Parameters.AddWithValue("@tradeId", tradeId);

                        // Ausführen der Datenbanklöschabfrage
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                // Fehlerbehandlung bei einer Ausnahme
                Console.WriteLine("Fehler: " + ex.Message);
            }
        }
    }
}
