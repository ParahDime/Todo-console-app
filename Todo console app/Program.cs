// See https://aka.ms/new-console-template for more information

using Microsoft.Data.Sqlite;
using System;
using System.IO.Pipes;
using Todo_console_app.Data;
using Todo_console_app.Users;
using static System.Runtime.InteropServices.JavaScript.JSType;
//for hashing passswords

namespace TestConsoleApp
{
    internal class Program
    {

        //print all information on the table
        public static void PrintTable(List<Dictionary <string, object>> TableData)
        {
            if (TableData.Count == 0) //empty table
            {
                Console.WriteLine("No data found.");
                return;
            }
            var columns = TableData[0].Keys;

            foreach (var col in columns)
            {
                Console.Write($"{col,-15} | ");
            }
            Console.WriteLine("\n");
            
            foreach(var row in TableData)
            {
                foreach(var col in columns)
                {
                    Console.Write($"{row[col],-15} | ");
                }
                Console.WriteLine();
            }

        }

        //used when empty strings not allowed
        static string ReadRequiredInput(string prompt)
        {
            string? input;

            do
            {
                Console.Write(prompt);
                input = Console.ReadLine();
            }
            while (string.IsNullOrWhiteSpace(input));

            return input; // Safe: guaranteed non-null
        }

        //login
        static User Login()
        {
            //ask for user information
            string username = ReadRequiredInput("Please enter your username ");
            string password = ReadRequiredInput("Please enter your password ");
            //salt and hash password

            //check if in systems
            if (Data.validateUser(username, password))
            {
                User person = Data.GetUser(username);
                Console.WriteLine("Login successful");
                Console.WriteLine($"Welcome, {username}");
                return person;
            }
            else //user not in system
            {
                Console.WriteLine("Invalid Credentials");
                Console.WriteLine("Press any key to continue");
                Console.ReadKey();
                return null;
            }
        }

        static void CreateAcc()
        {
            //get user details
            string username = ReadRequiredInput("Please enter a username    ");
            string password = ReadRequiredInput("Please enter a password    ");

            //input into sql database
            Data.createUser(username, password);
            Console.WriteLine("User {0} has been initialised. Press any key to continue", username);
        }

        static void Main(string[] args)
        {
            //create variables
            System.Console.WriteLine("Starting...");

            //create instance
            if (!Data.Initialise())
            {
                return;
            }
            Console.WriteLine("Database initialised.");

            //ONLY run if not already populated
            if (Data.IsDBEmpty())
            {
                if (!Data.Seed())
                {
                    return;
                }
                Console.WriteLine("Database populated");
            }

            User person;
            bool AccessGRANT = false;
            bool Running = true;
            string tableName = "";

            ConsoleKeyInfo SubMenu;
            ConsoleKeyInfo Menu; //selects the menu

            List<Dictionary<string, object>> TableData;



            Console.Clear();
            //auth loop to check if user in system
            while (!AccessGRANT)
            {
                Console.WriteLine("");
                Console.WriteLine("Please select an option");
                Console.WriteLine("[1] : Login \n[2] : Create Account \n[0] : Exit");
                Menu = Console.ReadKey();

                Console.WriteLine();

                switch(Menu.Key)
                {
                    case ConsoleKey.D1: //Login
                    case ConsoleKey.NumPad1:
                        person = Login();
                        if(person != null) //if login successful
                        {
                            AccessGRANT = true;
                        }
                        break;

                    case ConsoleKey.D2: //Create Account
                    case ConsoleKey.NumPad2:
                        CreateAcc();
                        break;

                    case ConsoleKey.D0: //Exit the progrram
                    case ConsoleKey.NumPad0:
                        Console.WriteLine("Program Terminated");
                        return;

                    default:
                        Console.WriteLine("Invalid selection.\nPress any key to continue");
                        Console.ReadKey();
                        break;
                }                
            }

            //items in program
            while(Running)
            {
                Console.WriteLine("Select Menu Option");
                Console.WriteLine("[1] : To Do List");
                Console.WriteLine("[2] : Expenses");
                Console.WriteLine("[0] : Exit");
                Menu = Console.ReadKey();

                switch (Menu.Key)
                {
                    case ConsoleKey.D1: //Go to ToDo List
                    case ConsoleKey.NumPad1:
                        tableName = "ActionsToDo";
                        break;

                    case ConsoleKey.D2: //Go to Expenses
                    case ConsoleKey.NumPad2:
                        tableName = "Expenses";
                        break;

                    case ConsoleKey.D0: //Exit the progrram
                    case ConsoleKey.NumPad0: 
                        Console.WriteLine("Program Terminated");
                        return;

                    default:
                        Console.WriteLine("Invalid selection.\nPress any key to continue");
                        Console.ReadKey();
                        break;
                }

                Console.Clear();

                //print all items
                TableData = Data.GetAllItems(tableName, 1);
                PrintTable(TableData);

                Console.WriteLine("Select SubMenu Option:");
                Console.WriteLine("[1] : Add item");
                Console.WriteLine("[2] : Remove Item");
                Console.WriteLine("[3] : Get item description");
                Console.WriteLine("[4] : Edit item in list");
                Console.WriteLine("[5] : Mark as completed");
                Console.WriteLine("[0] : Return to menu");

                SubMenu = Console.ReadKey();

                switch (SubMenu.Key)
                {
                    case ConsoleKey.D1: //Add Item
                    case ConsoleKey.NumPad1:
                        //select item name
                        //item description
                        break;
                    case ConsoleKey.D2: //Remove Item
                    case ConsoleKey.NumPad2:
                        //select item

                        //check if item exists

                        //confirm

                        //yn delete, keep and loop to submenu
                        break;
                    case ConsoleKey.D3: //Get Item Description
                    case ConsoleKey.NumPad3:
                        //input item name
                        //check, 
                        //yn show, error msg
                        break;
                    case ConsoleKey.D4: //Edit Item
                    case ConsoleKey.NumPad4:

                        //same as 3
                        break;
                    case ConsoleKey.D5: //Mark as completed
                    case ConsoleKey.NumPad5:
                        Console.WriteLine("Select an item you want to mark as completed");
                        //same as 3
                        break;
                    case ConsoleKey.D0: //Exit the progrram
                    case ConsoleKey.NumPad0:
                        Console.WriteLine("Program Terminated");
                        return;
                    default:
                        Console.WriteLine("Invalid selection.\nPress any key to continue");
                        Console.ReadKey();
                        break;
                }
            }
        }
    }
}
