
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Todo_console_app.Users;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

using System.Runtime.InteropServices;

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

        public static bool Seed()
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

        public static bool IsDBEmpty()
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM Users;";
            long count = (long)command.ExecuteScalar();

            return count == 0;
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
            if(!result.Read())
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
