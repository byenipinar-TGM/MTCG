using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using byenipinar_MTCG.GameClasses;
using Newtonsoft.Json;
using System.Text.Json;

namespace byenipinar_MTCG
{
    public class Data
    {
        private string connectionString = "Host=localhost;Database=MTCG;Username=postgres;Password=Pass2020!";

        private NpgsqlConnection connection;
        private User User { get; set; }
        private PackageOfCards PackageOfCards { get; set; }

        public Data(User u1)
        {
            User = u1;
        }

        public Data()
        {
            //DataAutomation();
        }

        public void DataAutomation()
        {
            DropTable(connectionString, "deck");
            DropTable(connectionString, "user_packages");
            DropTable(connectionString, "cards");
            DropTable(connectionString, "packages");
            DropTable(connectionString, "users");
            

            CreateTable(connectionString, "users", "CREATE TABLE IF NOT EXISTS users (token varchar(255) ,username VARCHAR(255) NOT NULL PRIMARY KEY ,password VARCHAR(255) NOT NULL,coins int NOT NULL ,name VARCHAR(255) ,bio VARCHAR(255) ,image VARCHAR(255) ,elo int NOT NULL ,wins int NOT NULL ,losses int NOT NULL);");
            CreateTable(connectionString, "packages", "CREATE TABLE IF NOT EXISTS packages (package_id SERIAL PRIMARY KEY, bought BOOLEAN NOT NULL);");
            CreateTable(connectionString, "cards", "CREATE TABLE IF NOT EXISTS cards (id VARCHAR(255) PRIMARY KEY, name VARCHAR(255) NOT NULL, damage DOUBLE PRECISION NOT NULL, package_id INTEGER REFERENCES packages(package_id));");
            CreateTable(connectionString, "user_packages", "CREATE TABLE IF NOT EXISTS user_packages (username VARCHAR(255) REFERENCES users(username), package_id INT REFERENCES packages(package_id), PRIMARY KEY (username, package_id));");
            CreateTable(connectionString, "deck", "CREATE TABLE IF NOT EXISTS deck (username VARCHAR(255) REFERENCES users(username),card_id VARCHAR(255) REFERENCES cards(id),PRIMARY KEY (username, card_id));");
        }

        private bool TablesExist(string connectionString)
        {
            return TableExists(connectionString, "user_packages") &&
                   TableExists(connectionString, "cards") &&
                   TableExists(connectionString, "packages") &&
                   TableExists(connectionString, "users");
        }

        private bool TableExists(string connectionString, string tableName)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                using (var command = new NpgsqlCommand($"SELECT EXISTS (SELECT FROM information_schema.tables WHERE table_name = '{tableName}')", connection))
                {
                    return (bool)command.ExecuteScalar();
                }
            }
        }


        public void CreateTable(string connectionString, string nameTable, string sql)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                {
                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"{e.Message}");
                    }
                }
                connection.Close();
            }

        }

        private void DropTable(string connectionString, string nameTable)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                using (NpgsqlCommand command = new NpgsqlCommand($"DROP TABLE IF EXISTS {nameTable};", connection))
                {
                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"{e.Message}");
                    }
                }
                connection.Close();
            }
        }


        public void AddUser()
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                // Füge den Benutzer zur Datenbank hinzu
                using (NpgsqlCommand command = new NpgsqlCommand("INSERT INTO users (token, username, password, coins, elo, wins, losses) VALUES (@token, @username, @password, @coins, 0, 0, 0)", connection))
                {
                    string token = User.Username + "-mtcgToken";

                    command.Parameters.AddWithValue("@token", token);
                    command.Parameters.AddWithValue("@username", User.Username);
                    command.Parameters.AddWithValue("@password", User.Password);
                    command.Parameters.AddWithValue("@coins", 20);

                    command.ExecuteNonQuery();
                }

                connection.Close();
            }
        }



        public bool DoesUserExist()
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                using (NpgsqlCommand command = new NpgsqlCommand("SELECT COUNT(*) FROM users WHERE username = @username;", connection))
                {
                    command.Parameters.AddWithValue("@username", User.Username);

                    int count = Convert.ToInt32(command.ExecuteScalar());

                    connection.Close();
                    return count > 0;
                }
            }
        }

        public bool DoesUserExist(string user)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                // Verwende den übergebenen Benutzernamen (user) anstelle von User.Username
                using (NpgsqlCommand command = new NpgsqlCommand("SELECT COUNT(*) FROM users WHERE username = @username;", connection))
                {
                    command.Parameters.AddWithValue("@username", user);

                    int count = Convert.ToInt32(command.ExecuteScalar());

                    connection.Close();
                    return count > 0;
                }
            }
        }




        public bool VerifyUserCredentials(User loginUser)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                string selectQuery = "SELECT username, password FROM users WHERE username = @username;";

                using (NpgsqlCommand command = new NpgsqlCommand(selectQuery, connection))
                {
                    command.Parameters.AddWithValue("@username", loginUser.Username);

                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string storedUsername = reader.GetString(0);
                            string storedPasswordHash = reader.GetString(1);

                            if (loginUser.Password == storedPasswordHash && loginUser.Username == storedUsername)
                            {
                                connection.Close();
                                return true;
                            }
                        }
                    }

                    connection.Close();
                    return false;
                }
            }
        }


        public void SpeicherePackages(string jsonString)
        {
            try
            {
                List<Card> cards = JsonConvert.DeserializeObject<List<Card>>(jsonString);

                // Erstellen Sie ein neues PackageOfCards-Objekt
                PackageOfCards packageOfCards = new PackageOfCards();

                // Holen Sie die letzte verwendete package_id ab
                int lastPackageId = GetLastPackageId();

                // Setzen Sie die nächste package_id für das neue Paket
                packageOfCards.PackageId = lastPackageId + 1;

                // Fügen Sie die Cards zum PackageOfCards hinzu
                packageOfCards.Cards.AddRange(cards);

                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    // Überprüfen Sie, ob ein Datensatz mit dem gleichen package_id bereits existiert
                    string checkQuery = "SELECT COUNT(*) FROM packages WHERE package_id = @packageId";

                    using (NpgsqlCommand checkCommand = new NpgsqlCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@packageId", packageOfCards.PackageId);

                        int existingCount = Convert.ToInt32(checkCommand.ExecuteScalar());

                        // Wenn der Datensatz bereits existiert, überspringe die Einfügung
                        if (existingCount > 0)
                        {
                            Console.WriteLine($"Paket mit package_id {packageOfCards.PackageId} existiert bereits. Überspringe Einfügung.");
                            return;
                        }
                    }

                    // Führen Sie die Einfügung durch, wenn der Datensatz nicht existiert
                    string insertQuery = "INSERT INTO packages (package_id, bought) VALUES (@packageId, @bought)";

                    using (NpgsqlCommand command = new NpgsqlCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@packageId", packageOfCards.PackageId);
                        command.Parameters.AddWithValue("@bought", false);

                        command.ExecuteNonQuery();
                    }
                }

                Console.WriteLine("Package erfolgreich gespeichert!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Speichern des Packages: {ex.ToString()}");
            }
        }


        public void SaveCardsFromRequest(string jsonPayload)
        {
            try
            {
                List<Card> cards = JsonConvert.DeserializeObject<List<Card>>(jsonPayload);

                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    foreach (var card in cards)
                    {
                        // Ersetzen Sie 'your_damage_property' durch die entsprechende Eigenschaft Ihrer Card-Klasse
                        string insertQuery = "INSERT INTO cards (id, name, damage, package_id) VALUES (@id, @name, @damage, @packageId)";

                        using (NpgsqlCommand cmd = new NpgsqlCommand(insertQuery, connection))
                        {
                            cmd.Parameters.AddWithValue("@id", card.Id);
                            cmd.Parameters.AddWithValue("@name", card.Name);

                            // Stellen Sie sicher, dass 'your_damage_property' ein double-Wert ist
                            cmd.Parameters.AddWithValue("@damage", card.Damage);

                            // Annahme: package_id ist der Wert, den Sie zurückgeben möchten
                            cmd.Parameters.AddWithValue("@packageId", GetLastPackageId());

                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                Console.WriteLine("Karten erfolgreich gespeichert!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Speichern der Karten: {ex.ToString()}");
            }
        }


        public int GetLastPackageId()
        {
            int lastPackageId = 0;

            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    // Abfrage, um die letzte verwendete package_id zu erhalten
                    string query = "SELECT MAX(package_id) FROM packages";

                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        object result = command.ExecuteScalar();

                        if (result != null && result != DBNull.Value)
                        {
                            // Konvertieren Sie das Ergebnis in einen Integer
                            lastPackageId = Convert.ToInt32(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Abrufen der letzten PackageID: {ex.ToString()}");
            }

            return lastPackageId;
        }


        public void BuyPackage(string username, int packageId)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                using (NpgsqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Schritt 1: UPDATE-Anweisung, um das Paket als gekauft zu markieren
                        string updateQuery = "UPDATE packages SET bought = true WHERE package_id = @PackageId;";
                        using (NpgsqlCommand updateCommand = new NpgsqlCommand(updateQuery, connection, transaction))
                        {
                            updateCommand.Parameters.AddWithValue("@PackageId", packageId);
                            updateCommand.ExecuteNonQuery();
                        }

                        // Schritt 2: INSERT-Anweisung, um die Verbindung zwischen Benutzer und gekauftem Paket zu speichern
                        string insertQuery = "INSERT INTO user_packages (username, package_id) VALUES (@Username, @PackageId);";
                        using (NpgsqlCommand insertCommand = new NpgsqlCommand(insertQuery, connection, transaction))
                        {
                            insertCommand.Parameters.AddWithValue("@Username", username);
                            insertCommand.Parameters.AddWithValue("@PackageId", packageId);
                            insertCommand.ExecuteNonQuery();
                        }

                        // Transaktion erfolgreich durchgeführt, Änderungen übernehmen
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        // Bei einem Fehler die Transaktion rückgängig machen
                        Console.WriteLine($"Fehler beim Kauf des Pakets: {ex.Message}");
                        transaction.Rollback();
                    }
                }
            }
        }

        public void ChangeUserCoins(string username, int newCoinsAmount)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                try
                {
                    // UPDATE-Anweisung, um die Anzahl der Coins für den Benutzer zu ändern
                    string updateQuery = "UPDATE users SET coins = @NewCoinsAmount WHERE username = @Username;";
                    using (NpgsqlCommand updateCommand = new NpgsqlCommand(updateQuery, connection))
                    {
                        updateCommand.Parameters.AddWithValue("@NewCoinsAmount", newCoinsAmount);
                        updateCommand.Parameters.AddWithValue("@Username", username);
                        updateCommand.ExecuteNonQuery();
                    }

                    Console.WriteLine($"Die Anzahl der Coins für den Benutzer {username} wurde erfolgreich aktualisiert.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Fehler beim Ändern der Coins: {ex.Message}");
                }
            }
        }


        public int GetUserCoins(string username)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                using (NpgsqlCommand command = new NpgsqlCommand("SELECT coins FROM users WHERE username = @username;", connection))
                {
                    command.Parameters.AddWithValue("@username", username);

                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int storedCoins = reader.GetInt32(0);
                            connection.Close();
                            return storedCoins;
                        }
                    }
                    connection.Close();
                    return -1;
                }
            }
        }

        public string GetUsername(string token)
        {
            string username = "";
            int indexOfHyphen = token.IndexOf('-');

            if (indexOfHyphen != -1) username = token.Substring(0, indexOfHyphen);

            return username;
        }


        public List<int> GetAvailablePackages()
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                using (NpgsqlCommand command = new NpgsqlCommand("SELECT package_id FROM packages WHERE bought = false;", connection))
                {
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        List<int> packageIds = new List<int>();

                        while (reader.Read())
                        {
                            int packageId = reader.GetInt32(0);
                            packageIds.Add(packageId);
                        }

                        return packageIds;
                    }
                }
            }
        }


        public bool TokenExist(string token)
        
        {
            // Verbindung zur Datenbank herstellen
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                try
                {
                    // Verbindung öffnen
                    connection.Open();

                    // SQL-Befehl erstellen
                    string sql = "SELECT COUNT(*) FROM users WHERE token = @token;";
                    using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                    {
                        // Parameter hinzufügen, um SQL-Injection zu verhindern
                        command.Parameters.AddWithValue("@token", token);

                        // Befehl ausführen und Anzahl der übereinstimmenden Zeilen erhalten
                        int rowCount = Int32.Parse(command.ExecuteScalar().ToString());

                        // Rückgabewert: true, wenn Anzahl größer als null; sonst false
                        return rowCount > 0;
                    }
                }
                catch (Exception ex)
                {
                    // Fehlerbehandlung hier nach Bedarf
                    Console.WriteLine("Fehler beim Zugriff auf die Datenbank: " + ex.Message);
                    return false;
                }
            }
        }



        public string GetCardDataJson(string token)
        {
            // Liste für Kartendaten erstellen
            List<Card> userCards = new List<Card>();

            // Verbindung zur Datenbank herstellen
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                try
                {
                    // Verbindung öffnen
                    connection.Open();

                    // SQL-Befehl erstellen
                    string sql = "SELECT cards.id, cards.name, cards.damage " +
                                 "FROM users " +
                                 "JOIN user_packages ON users.username = user_packages.username " +
                                 "JOIN cards ON user_packages.package_id = cards.package_id " +
                                 "WHERE users.token = @token;";

                    using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                    {
                        // Parameter hinzufügen, um SQL-Injection zu verhindern
                        command.Parameters.AddWithValue("@token", token);

                        // Daten aus der Datenbank lesen
                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            // Zeilen durchlaufen
                            while (reader.Read())
                            {
                                // Card-Objekt erstellen und mit Daten füllen
                                Card card = new Card
                                {
                                    Id = reader.GetString(0),
                                    Name = reader.GetString(1),
                                    Damage = reader.GetDouble(2)
                                };

                                // Card zur Liste hinzufügen
                                userCards.Add(card);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Fehlerbehandlung hier nach Bedarf
                    Console.WriteLine("Fehler beim Zugriff auf die Datenbank: " + ex.Message);
                }
            }

            // Verbindung wurde bereits automatisch geschlossen
            // Kartendaten in JSON konvertieren
            string jsonResult = System.Text.Json.JsonSerializer.Serialize(userCards, new JsonSerializerOptions { WriteIndented = true });

            // JSON-Zeichenfolge zurückgeben
            return jsonResult;
        }


        public string GetCardsFromDeck(string token, bool format)
        {
            List<Card> userCards = new List<Card>();

            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string sql = "SELECT cards.id, cards.name, cards.damage " +
                                 "FROM users " +
                                 "JOIN user_packages ON users.username = user_packages.username " +
                                 "JOIN cards ON user_packages.package_id = cards.package_id " +
                                 "JOIN deck ON users.username = deck.username AND cards.id = deck.card_id " +
                                 "WHERE users.token = @token;";

                    using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@token", token);

                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Card card = new Card
                                {
                                    Id = reader.GetString(0),
                                    Name = reader.GetString(1),
                                    Damage = reader.GetDouble(2)
                                };

                                userCards.Add(card);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Fehler beim Zugriff auf die Datenbank: " + ex.Message);
                }
            }

            if (format)
            {
                // Formatierung als Text
                StringBuilder formattedText = new StringBuilder();

                foreach (Card card in userCards)
                {
                    formattedText.AppendLine($"ID: {card.Id}, Name: {card.Name}, Damage: {card.Damage}");
                }

                return formattedText.ToString();
            }
            else
            {
                // JSON-Format
                string jsonResult = System.Text.Json.JsonSerializer.Serialize(userCards, new JsonSerializerOptions { WriteIndented = true });
                return jsonResult;
            }
        }


        public bool CardacquireUser(string token, string cardId)
        {
            // SQL-Abfrage
            string sqlQuery = "SELECT COUNT(*) " +
                              "FROM users " +
                              "JOIN user_packages ON users.username = user_packages.username " +
                              "JOIN cards ON user_packages.package_id = cards.package_id " +
                              "WHERE users.token = @token AND cards.id = @cardId;";

            // Ergebnisinitialisierung
            int count = 0;

            // Verbindung zur Datenbank
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                // SQL-Befehl und Parameter
                using (NpgsqlCommand command = new NpgsqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@token", token);
                    command.Parameters.AddWithValue("@cardId", cardId);

                    // Ausführung der Abfrage
                    count = Convert.ToInt32(command.ExecuteScalar());
                }
            }

            // Überprüfung des Ergebnisses
            return count > 0;
        }

        public bool DeleteDeckFromUser(string username)
        {
            // SQL-DELETE-Abfrage
            string sqlQuery = "DELETE FROM deck WHERE username = @username;";

            // Anzahl der gelöschten Zeilen
            int rowsAffected = 0;

            // Verbindung zur Datenbank
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                // SQL-Befehl und Parameter
                using (NpgsqlCommand command = new NpgsqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@username", username);

                    // Ausführung der DELETE-Abfrage
                    rowsAffected = command.ExecuteNonQuery();
                }
            }

            // Überprüfung der Anzahl der gelöschten Zeilen
            return rowsAffected > 0;
        }

        public bool AddDeck(string username, string cardId)
        {
            // SQL-INSERT-Abfrage
            string sqlQuery = "INSERT INTO deck (username, card_id) VALUES (@username, @cardId);";

            // Anzahl der eingefügten Zeilen
            int rowsAffected = 0;

            // Verbindung zur Datenbank
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                // SQL-Befehl und Parameter
                using (NpgsqlCommand command = new NpgsqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@cardId", cardId);

                    // Ausführung der INSERT-Abfrage
                    rowsAffected = command.ExecuteNonQuery();
                }
            }

            // Überprüfung der Anzahl der eingefügten Zeilen
            return rowsAffected > 0;
        }


        public string ExtractUsernameFromRequest(string request)
        {
            string username = string.Empty;

            // Find the index of "/users/"
            int start = request.IndexOf("/users/");

            if (start != -1)
            {
                // Calculate the starting index of the username
                int usernameStart = start + "/users/".Length;

                // Find the index of the next space after the username
                int spaceIndex = request.IndexOf(' ', usernameStart);

                // Extract the username if space is found
                if (spaceIndex != -1)
                {
                    username = request.Substring(usernameStart, spaceIndex - usernameStart);
                }
            }

            return username;
        }

        public bool IsTokenValid(string token, string username)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                using (NpgsqlCommand command = new NpgsqlCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = "SELECT COUNT(*) FROM users WHERE username = @username AND token = @token;";

                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@token", token);

                    int count = Convert.ToInt32(command.ExecuteScalar());

                    return count > 0;
                }
            }
        }


        public string GetUserDataJson(string username)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                using (NpgsqlCommand command = new NpgsqlCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = "SELECT image, bio, name, username FROM users WHERE username = @username;";
                    command.Parameters.AddWithValue("@username", username);

                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            User user = new User
                            {
                                Image = reader.IsDBNull(0) ? null : reader.GetString(0),
                                Bio = reader.IsDBNull(1) ? null : reader.GetString(1),
                                Name = reader.IsDBNull(2) ? null : reader.GetString(2),
                                Username = reader.IsDBNull(3) ? null : reader.GetString(3)
                            };

                            var result = new
                            {
                                Bio = user.Bio,
                                Image = user.Image,
                                Name = user.Name,
                                Username = user.Username
                            };

                            // Serialize user object to JSON
                            string jsonResult = JsonConvert.SerializeObject(user, Formatting.Indented);

                            return jsonResult;
                        }
                    }
                }
            }

            // Return an empty JSON object if no data found
            return "{}";
        }


        public bool UpdateUserData(string username, string jsonUserData)
        {
            try
            {
                // Deserialisiere JSON-Daten in ein User-Objekt
                User updatedUser = JsonConvert.DeserializeObject<User>(jsonUserData);

                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    using (NpgsqlCommand command = new NpgsqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandType = CommandType.Text;
                        command.CommandText = "UPDATE users SET name = @name, bio = @bio, image = @image WHERE username = @username;";

                        // Füge Parameter zur SQL-Abfrage hinzu
                        command.Parameters.AddWithValue("@username", username);
                        command.Parameters.AddWithValue("@name", updatedUser.Name);
                        command.Parameters.AddWithValue("@bio", updatedUser.Bio);
                        command.Parameters.AddWithValue("@image", updatedUser.Image);

                        // Führe die SQL-Abfrage aus
                        int rowsAffected = command.ExecuteNonQuery();

                        // Überprüfe das Ergebnis und gib true zurück, wenn mindestens eine Zeile aktualisiert wurde
                        return rowsAffected > 0;
                    }
                }
            }
            catch (System.Text.Json.JsonException ex)
            {
                // Fehler beim Deserialisieren der JSON-Daten
                Console.WriteLine("Fehler beim Deserialisieren der JSON-Daten: " + ex.Message);
                return false;
            }
        }

        public string GetUserStatisticsJson(string token)
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
                        command.CommandText = "SELECT name, elo, wins, losses FROM users WHERE token = @token;";
                        command.Parameters.AddWithValue("@token", token);

                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var userStats = new
                                {
                                    Name = reader.GetString(0),
                                    Elo = reader.GetInt32(1),
                                    Wins = reader.GetInt32(2),
                                    Losses = reader.GetInt32(3)
                                };

                                // Serialize userStats object to JSON
                                string jsonResult = JsonConvert.SerializeObject(userStats, Formatting.Indented);

                                return jsonResult;
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

            // Return an empty JSON object if no data found or an exception occurred
            return "{}";
        }


        public List<User> GetScoreboard()
        {
            var scoreboardList = new List<User>();

            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    using (NpgsqlCommand command = new NpgsqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandType = CommandType.Text;
                        command.CommandText = "SELECT name, elo, wins, losses FROM users WHERE Name IS NOT NULL AND Name <> '' ORDER BY Elo DESC";

                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var name = reader.GetString(0);
                                var elo = reader.GetInt32(1);
                                var wins = reader.GetInt32(2);
                                var losses = reader.GetInt32(3);

                                scoreboardList.Add(new User
                                {
                                    Name = name,
                                    Elo = elo,
                                    Wins = wins,
                                    Losses = losses
                                }); 

                            }
                        }
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                // Fehlerbehandlung bei einer Ausnahme
                Console.WriteLine("Fehler: " + ex.ToString());
            }

            return scoreboardList;
        }



    }
}
