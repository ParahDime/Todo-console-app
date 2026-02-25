
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Todo_console_app.Frequency;
using Todo_console_app.Updates;
using Todo_console_app.Users;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static Todo_console_app.Frequency.Frequency;
using static Todo_console_app.Updates.Update;

namespace Todo_console_app.Data
{
    internal class Data
    {

        private const string ConnectionString = "Data Source=app.db; Foreign Keys = True";

        //initialise SQL Data
        public static bool Initialise()
        {
            try
            {
                //read in tables
                using var connection = new SqliteConnection(ConnectionString);
                connection.Open();

                string sql = File.ReadAllText("Data/schema.sql");
                Console.WriteLine(connection.GetType().FullName);
                var command = connection.CreateCommand();

                command.CommandText = sql;
                command.ExecuteNonQuery();


                connection.Close();
                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Initialisation failed: {ex.Message}");
                Console.WriteLine("Press any key to continue");
                Console.ReadKey();
                return false;
            }
        }

        public static bool Seed() //Populate the data if purged
        {
            try
            {
                //read in seed data
                using var connection = new SqliteConnection(ConnectionString);
                connection.Open();
                string sql = File.ReadAllText("Data/seed.sql");

                var command = connection.CreateCommand();
                command.CommandText = sql;
                command.ExecuteNonQuery();

                connection.Close();
                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Seeding failed: {ex.Message}");
                Console.WriteLine("Press any key to continue");
                Console.ReadKey();
                return false;
            }
        }

        public static bool IsDBEmpty() //check if the DB is empty
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM Users;";
            long count = (long)command.ExecuteScalar();

            return count == 0;
        }

        public static List<Dictionary<string, object>> GetExpensesList(User person) //Gets the expenses list in order
        {
            var results = new List<Dictionary<string, object>>();

            try
            {
                using var connection = new SqliteConnection(ConnectionString);
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = @"
                SELECT *, 
                CASE 
                    WHEN Frequency = 'Daily' THEN Amount * 30
                    WHEN Frequency = 'Weekly' THEN Amount * 4
                    ELSE Amount 
                END AS MonthlyCost
                FROM Expenses 
                WHERE UserId = @userId
                ORDER BY MonthlyCost DESC"; 
                //gets the amount, when sorted
                command.Parameters.AddWithValue("@userId", person.Id);

                using var reader = command.ExecuteReader();
                while (reader.Read())//while there is data
                {
                    var row = new Dictionary<string, object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row.Add(reader.GetName(i), reader.GetValue(i));
                    }
                    results.Add(row);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database Error: {ex.Message}");
            }

            return results;
        }
        public static double CalculateSpend(User person) //Calculates the monthly spend of all expenses
        {
            double totalMonthly = 0;

            try
            {
                using var connection = new SqliteConnection(ConnectionString);
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = "SELECT Amount, Frequency FROM Expenses WHERE UserId = @userId";
                command.Parameters.AddWithValue("@userId", person.Id);

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    double amount = reader.GetDouble(0);

                    if (Enum.TryParse(reader.GetString(1), true, out Frequent freq))
                    {
                        switch (freq)
                        {
                            case Frequent.Daily:
                                totalMonthly += (amount * 30);
                                break;
                            case Frequent.Weekly:
                                totalMonthly += (amount * 4);
                                break;
                            case Frequent.Monthly:
                                totalMonthly += amount;
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database Error: {ex.Message}");
            }

            return totalMonthly;
        }
        public static double GetCompletedAmount(User person) //get % of all items done
        {
            try
            {
                using var connection = new SqliteConnection(ConnectionString);
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = $@"
                SELECT 
                    (CAST(SUM(CASE WHEN Is_Complete = 1 THEN 1 ELSE 0 END) AS DOUBLE) / 
                     CAST(COUNT(*) AS DOUBLE)) * 100
                FROM ActionsToDo
                WHERE UserId = @userId";

                command.Parameters.AddWithValue("@userId", person.Id);

                var result = command.ExecuteScalar();

                if (result == DBNull.Value || result == null) return 0.0;

                return Convert.ToDouble(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database Error: {ex.Message}");
                return 0.0;
            }
        }
        public static List<Dictionary<string, object>> ShowRecentItems(User person) //all items added within 24h
        {
            var results = new List<Dictionary<string, object>>();

            try
            {
                using var connection = new SqliteConnection(ConnectionString);
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $@"
                    SELECT * FROM ActionsToDo
                    WHERE UserId = @userId 
                    AND Time_Create >= strftime('%s', 'now', '-24 hours's)";

                command.Parameters.AddWithValue("@userId", person.Id);

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var row = new Dictionary<string, object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row.Add(reader.GetName(i), reader.GetValue(i));
                    }
                    results.Add(row);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database Error: {ex.Message}");
            }

            return results;
        }

        public static UpdateResult MarkCompleted(int Id, User person)
        {
            try
            {
                using var connection = new SqliteConnection(ConnectionString);
                connection.Open();

                // 1. Check if the item exists and what its current status is
                var checkCmd = connection.CreateCommand();
                checkCmd.CommandText = "SELECT Is_Complete FROM ActionsToDo WHERE Id = @Id AND UserId = @UserId";
                checkCmd.Parameters.AddWithValue("@Id", Id);
                checkCmd.Parameters.AddWithValue("@UserId", person.Id);

                var result = checkCmd.ExecuteScalar();

                if (result == null)
                    return UpdateResult.NotFound;

                if (Convert.ToInt32(result) == 1)
                    return UpdateResult.AlreadyCompleted;

                var updateCmd = connection.CreateCommand();
                updateCmd.CommandText = "UPDATE ActionsToDo SET Is_Complete = 1 WHERE Id = @Id";
                updateCmd.Parameters.AddWithValue("@Id", Id);

                updateCmd.ExecuteNonQuery();
                return UpdateResult.Success;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DB Error: {ex.Message}");
                return UpdateResult.Error;
            }
        }

        public static bool RemoveCompletedItems(User person) //removes all comp items relating to user
        {
            if (person == null) return false;

            try
            {
                using var connection = new SqliteConnection(ConnectionString);
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                    DELETE FROM ActionsToDo 
                    WHERE UserId = @UserId AND Is_Complete = 1";

                command.Parameters.AddWithValue("@UserId", person.Id);

                int rowsAffected = command.ExecuteNonQuery();

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database Error: {ex.Message}");
                return false;
            }
        }
        public static Dictionary<string, object> GetItemData(int Id, string Table) //Get all data on an item, output
        {
            var result = new Dictionary<string, object>();

            try
            {
                using var connection = new SqliteConnection(ConnectionString);
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = $"SELECT * FROM {Table} WHERE Id = @Id LIMIT 1";
                command.Parameters.AddWithValue("@id", Id);

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        result.Add(reader.GetName(i), reader.GetValue(i));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database Error: {ex.Message}");
            }

            return result; // Returns an empty dictionary if no item is found
        }

        public static Dictionary<string, object> GetItemByAnyIdentifier(string table, string input, User person) //get an item by its ID
        {
            var result = new Dictionary<string, object>();

            try
            {
                using var connection = new SqliteConnection(ConnectionString);
                connection.Open();

                var command = connection.CreateCommand();

                // We check if the input matches the ID OR the Title
                // Note: Title uses 'LIKE' for a bit of flexibility with casing
                command.CommandText = $@"
            SELECT * FROM {table} 
            WHERE (Id = @input OR Title LIKE @input) 
            AND UserId = @userId 
            LIMIT 1";

                command.Parameters.AddWithValue("@input", input);
                command.Parameters.AddWithValue("@userId", person.Id);

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        result.Add(reader.GetName(i), reader.GetValue(i));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database Error: {ex.Message}");
            }

            return result;
        }

        public static bool RemoveItem(string table, int itemId) //Remove an item from the DB
        {
            //prevents users being removed
            string[] allowedTables = {"ActionsToDo", "Expenses"};
            if (!allowedTables.Contains(table))
            {
                return false;
            }
            try
            {
                using var connection = new SqliteConnection(ConnectionString);
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"DELETE FROM {table} WHERE Id = @id";
                command.Parameters.AddWithValue("@id", itemId);

                int rowsAffected = command.ExecuteNonQuery();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database Error: {ex.Message}");
                return false;
            }
        }
        public static bool AddToDoItem(string Title, string Description, User person) //Add an item to the DB
        {
            try
            {
                using var connection = new SqliteConnection(ConnectionString);
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                INSERT INTO ActionsToDo (UserId, Title, Description, Is_Complete, Time_Create) 
                VALUES (@UserId, @Title, @Description, 0, CURRENT_TIMESTAMP)";
            

                command.Parameters.AddWithValue("@UserId", person.Id);
                command.Parameters.AddWithValue("@Title", Title);
                command.Parameters.AddWithValue("@Description", Description);

                int rows = command.ExecuteNonQuery();
                return rows > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DB Error: {ex.Message}");
                return false;
            }
           
        }

        public static bool AddExpenseItem(string Title, int Frequency, int Amount, User person)
        {
            try
            {
                using var connection = new SqliteConnection(ConnectionString);
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                INSERT INTO Expenses (UserId, Amount, Frequency, Title) 
                VALUES (@UserId, @Amount, @Frequency, @Tile)"";
                ";

                command.Parameters.AddWithValue("@UserId", person.Id);
                command.Parameters.AddWithValue("@Amount", Amount);
                command.Parameters.AddWithValue("@Frequency", Frequency);
                command.Parameters.AddWithValue("@Title", Title);

                var result = command.ExecuteNonQuery();

                int rows = command.ExecuteNonQuery();
                return rows > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DB Error: {ex.Message}");
                return false;
            }
        }

        //create a salt for passwords
        public static string GenerateSalt()
        {
            byte[] salt = RandomNumberGenerator.GetBytes(16);
            return Convert.ToBase64String(salt);
        }

        public static string HashPassword(string password, string storedSalt)
        {
            byte[] salt = Convert.FromBase64String(storedSalt);

            using var pbkdf2 = new Rfc2898DeriveBytes(
                password,
                salt,
                100_000,                 // iteration count
                HashAlgorithmName.SHA256
            );

            byte[] hash = pbkdf2.GetBytes(32); // 256-bit key
            return Convert.ToBase64String(hash);
        }

        
        //check the username exists
        public static bool validateUser(string username, string password)
        {
            try
            {
                using var connection = new SqliteConnection(ConnectionString);
                connection.Open();

                //check if the username exists within the username table
                var command = connection.CreateCommand();
                command.CommandText = @"
                SELECT Passwrd_Hash, Salt
                FROM Users
                WHERE Username = @Username;;
            ";

                command.Parameters.AddWithValue("@Username", username);

                var result = command.ExecuteReader();

                //if person is not found
                if (!result.Read())
                {
                    Console.WriteLine("No person found");
                    return false;
                }

                string storedHash = result.GetString(0);
                string storedSalt = result.GetString(1);

                string computedHash = HashPassword(password, storedSalt);

                return CryptographicOperations.FixedTimeEquals( //compare the new result to the table, check if there
                        Convert.FromBase64String(computedHash),
                        Convert.FromBase64String(storedHash)
                    );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DB Error: {ex.Message}");
                return false;
            }
        }

        public static User GetUser(string username)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT UserId, Username FROM Users WHERE Username = @Username";
            command.Parameters.AddWithValue("@Username", username);

            using var reader = command.ExecuteReader();

            if (reader.Read())
            {
                return new User
                {
                    Id = reader.GetInt32(0),
                    Username = reader.GetString(1)
                };
            }

            // 5. If no user was found, return null
            return null;
        }

        public static void createUser(string username, string password)
        {
            try
            {
                string salt = GenerateSalt();
                password = HashPassword(password, salt);

                //Console.WriteLine(password);
                //Console.WriteLine(salt);
                using var connection = new SqliteConnection(ConnectionString);
                connection.Open();

                //create command for generating a new user
                var command = connection.CreateCommand();
                command.CommandText = @"INSERT INTO Users (Username, Salt, Passwrd_Hash)
            VALUES (@Username, @Salt, @PasswordHash)
            ";

                command.Parameters.AddWithValue("@Username", username);
                command.Parameters.AddWithValue("@Salt", salt);
                command.Parameters.AddWithValue("@PasswordHash", password);

                var result = command.ExecuteScalar();
            } catch (Exception ex)
            {
                Console.WriteLine($"DB Error: {ex.Message}");
            }
        }
        //count all items within a table
        /*public static void GetCount(string table)
        {
            using var connection = new SqliteConnection(ConnectionString);


        }
        */


        //list all items in a table
        public static List<Dictionary<string, object>> GetAllItems(string table, int UserId)
        {
            using var connection = new SqliteConnection(ConnectionString);
            var results = new List<Dictionary<string, object>>();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = $"SELECT * FROM {table} WHERE UserId = @UserId";

            command.Parameters.AddWithValue("@UserId", UserId);
            using var reader = command.ExecuteReader();

            //read data from sql query
            while (reader.Read())
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    // reader.GetName(i) gets the column name from the SQL result
                    row.Add(reader.GetName(i), reader.GetValue(i));
                }
                results.Add(row);
            }
            return results;
        }
    }
}
