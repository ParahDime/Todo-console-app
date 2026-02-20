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
            string username = ReadRequiredInput("Please enter your username:    ");
            string password = ReadRequiredInput("Please enter your password:    ");
            //salt and hash password
            Console.Clear();
            //check if in systems
            if (Data.validateUser(username, password))
            {
                User person = Data.GetUser(username);
                Console.WriteLine("Login successful.");
                Console.WriteLine($"Welcome, {username}.\n\n");
                return person;
            }
            else //user not in system
            {
                Console.WriteLine("Invalid Credentials.");
                Console.WriteLine("Press any key to continue.\n\n");
                Console.ReadKey();
                return null;
            }
        }

        static void CreateAcc()
        {
            //get user details
            string username = ReadRequiredInput("Please enter a username:    ");
            string password = ReadRequiredInput("Please enter a password:    ");

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

            //User person;
            bool AccessGRANT = false;
            bool Running = true;
            string TableName = null;
            char YesNo = ' ';
            int NumberId = 0;



            User person = null;
            ConsoleKeyInfo SubMenu;
            ConsoleKeyInfo Menu; //selects the menu

            List<Dictionary<string, object>> TableData;



            Console.Clear();
            //auth loop to check if user in system
            while (!AccessGRANT)
            {
                Console.WriteLine("");
                Console.WriteLine("Please select an option:");
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
                while (TableName == null) {
                    TableName = null;
                    Console.WriteLine("Select Menu Option:\n");
                    Console.WriteLine("[1] : To Do List");
                    Console.WriteLine("[2] : Expenses");
                    Console.WriteLine("[0] : Exit");
                    Menu = Console.ReadKey();

                    switch (Menu.Key)
                    {
                        case ConsoleKey.D1: //Go to ToDo List
                        case ConsoleKey.NumPad1:
                            TableName = "ActionsToDo";
                            break;

                        case ConsoleKey.D2: //Go to Expenses
                        case ConsoleKey.NumPad2:
                            TableName = "Expenses";
                            break;

                        case ConsoleKey.D0: //Exit the progrram
                        case ConsoleKey.NumPad0:
                            Console.WriteLine("\n\nProgram Terminated");
                            return;

                        default:
                            Console.WriteLine("\n\nInvalid selection.\nPress any key to continue..");
                            Console.ReadKey();
                            break;
                    }

                    Console.Clear();
                }

                //print all items
                TableData = Data.GetAllItems(TableName, person.Id);
                PrintTable(TableData);

                Console.WriteLine("Select SubMenu Option:");
                Console.WriteLine("[1] : Add item");
                Console.WriteLine("[2] : Remove Item");
                Console.WriteLine("[3] : Get item information");
                Console.WriteLine("[4] : Edit item in list");
                if (TableName == "ActionsToDo")
                {
                    Console.WriteLine("[5] : Mark as completed");
                    Console.WriteLine("[6] : Remove completed items");
                    Console.WriteLine("[7] : Recent items added");
                    Console.WriteLine("[8] : Calculate current list completion");
                }
                else if (TableName == "Expenses")
                {
                    Console.WriteLine("[5] : Total Monthly spend");
                    Console.WriteLine("[6] : Sort items by cost per month");
                }
                else
                {

                }
                
                Console.WriteLine("[0] : Return to menu");

                SubMenu = Console.ReadKey();
                string Name = null;
                string Description = null;
                switch (SubMenu.Key)
                {
                    case ConsoleKey.D1: //Add Item
                    case ConsoleKey.NumPad1:
                        Console.WriteLine("[1] : Add item\n");
                        Name = ReadRequiredInput("Enter item name: ");
                        Description = ReadRequiredInput("Enter the item description: ");

                        Console.WriteLine("Are these details correct? y/n");
                        Console.WriteLine($"Name: {Name}\n Description: {Description}");

                        //write function to get character input
                        /*
                         if(yesno = 'y')
                        {
                             Data.AddItem(); //make into bool function

                        }
                        else 
                        {
                             Console.WriteLine("Input was not added. Press any key to continue);
                             Console.ReadKey();
                        }
                         */
                        break;
                    case ConsoleKey.D2: //Remove Item
                    case ConsoleKey.NumPad2:
                        Console.WriteLine("[2] : Remove Item\n");
                        Name = ReadRequiredInput("Enter item ID you wish to remove: ");
                        //take input from the user
                        //Console.WriteLine("Are you sure you want to remove {ActualName}?\n y/n);
                        /*if(y)
                        {

                        }
                        else //n
                        {
                        }
                        */
                        //yn delete, keep and loop to submenu
                        break;
                    case ConsoleKey.D3: //Get Item Description
                    case ConsoleKey.NumPad3:
                        Console.WriteLine("[3] : Get item description\n");
                        Console.WriteLine("Please enter item number: ");
                        //check, 
                        //yn show, error msg
                        break;
                    case ConsoleKey.D4: //Edit Item
                    case ConsoleKey.NumPad4:
                        Console.WriteLine("[4] : Edit item in list\n");
                        Console.WriteLine("Select item number you wish to edit: ");
                        //same as 3
                        break;
                    case ConsoleKey.D5 when TableName == "ActionsToDo": //Mark as completed
                    case ConsoleKey.NumPad5 when TableName == "ActionsToDo":
                        Console.WriteLine("[5] : Mark as completed\n");
                        Console.WriteLine("Enter ID number you want to mark as completed");
                        //take user input
                        //check on list
                        //if in list, show
                        //if not, error message
                        break;
                    case ConsoleKey.D6 when TableName == "ActionsToDo": //Remove completed items
                    case ConsoleKey.NumPad6 when TableName == "ActionsToDo":
                        Console.WriteLine("[6] : Remove completed items\n");
                        Console.WriteLine("Do you wish to remove completed items? y'n");
                        /*if(y)
                         {
                        }
                        else 
                        {
                            Console.WriteLine("Item was not removed.");
                            Console.WriteLine("Press any key to continue);
                            Console.ReadKey();
                        }
                        */
                        break;
                    case ConsoleKey.D7 when TableName == "ActionsToDo": //Recent items added
                    case ConsoleKey.NumPad7 when TableName == "ActionsToDo":
                        Console.WriteLine("[7] : Recent items added\n");
                        //query, sort by date, then output
                        break;
                    case ConsoleKey.D8 when TableName == "ActionsToDo": //Calc list completion
                    case ConsoleKey.NumPad8 when TableName == "ActionsToDo":
                        Console.WriteLine("[8] : Calculate current list completion\n");
                        //compare items listed as done to total, output %
                        break;
                    case ConsoleKey.D5 when TableName == "Expenses": //Mark as completed
                    case ConsoleKey.NumPad5 when TableName == "Expenses":
                        Console.WriteLine("[5] : Total Monthly spend\n");
                        Console.WriteLine("Enter ID number you want to mark as completed");
                        //same as 3
                        break;
                    case ConsoleKey.D6 when TableName == "ActionsToDo": //Remove completed items
                    case ConsoleKey.NumPad6 when TableName == "ActionsToDo":
                        Console.WriteLine("[6] : Sort items by cost per month\n");
                        Console.WriteLine("Do you wish to remove completed items? y'n");
                        break;
                    case ConsoleKey.D0: //Exit the progrram
                    case ConsoleKey.NumPad0:
                        Console.WriteLine("Returning to main menu. Press any key to continue");
                        Console.ReadKey();
                        break;
                    default:
                        Console.WriteLine("Invalid selection.\nPress any key to continue");
                        Console.ReadKey();
                        break;
                }
                TableName = null;
                Console.Clear();
            }
        }
    }
}
