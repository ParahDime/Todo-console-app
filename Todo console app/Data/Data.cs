using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Todo_console_app.Data
{
    internal class Data
    {

        private const string ConnectionString = "Data Source=app.db";

        //initialise SQL Data
        public static void Initialise()
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            // initialise schema.sql

            connection.Close();
        }

        //function to recall information (to test and expand ability of function
        public static void GetUserCount()
        {
            /*using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "";

            var result = command.ExecuteScalar();
            return result;*/
        }
    }
}
