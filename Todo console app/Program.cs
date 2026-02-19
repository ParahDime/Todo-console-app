// See https://aka.ms/new-console-template for more information

using System;
using Todo_console_app.Data;
using Microsoft.Data.Sqlite;
//for hashing passswords

namespace TestConsoleApp
{
    internal class Program
    {


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
        static bool Login()
        {
            //ask for user information
            string username = ReadRequiredInput("Please enter your username ");
            string password = ReadRequiredInput("Please enter your password ");
            //salt and hash password

            //check if in systems
            if (Data.validateUser(username, password))
            {
                Console.WriteLine("Login successful");
                Console.WriteLine($"Welcome, {username}");
                Console.Clear();
                return true;
            }
            else //user not in system
            {
                Console.WriteLine("Invalid Credentials");
                Console.WriteLine("Press any key to continue");
                Console.ReadKey();
                Console.Clear();
                return false;
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


        //Terminates the program without redundant code in Main
        static void TerminateProgram()
        {
            Console.Clear();
            Console.WriteLine("Program Terminated"); //Message to be not redundant
        }

        static void Spacer(int lines)
        {
            for (int i = 0; i < lines; i++)
                Console.WriteLine();
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

            bool AccessGRANT = false;
            bool Running = true;
            ConsoleKeyInfo SubMenu;

            ConsoleKeyInfo Menu; //selects the menu
            Console.Clear();
            //auth loop to check if user in system
            while (!AccessGRANT)
            {
                Console.Clear();
                Spacer(1);
                Console.WriteLine("Please select an option");
                Console.WriteLine("[1] : Login \n[2] : Create Account \n[0] : Exit");
                Menu = Console.ReadKey();
                Spacer(2);

                switch (Menu.Key)
                {
                    case ConsoleKey.D1: //Login
                    case ConsoleKey.NumPad1:
                        if (Login()) //if login successful
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
                        TerminateProgram();
                        return;

                    default:
                        Console.WriteLine("Invalid selection.\nPress any key to continue");
                        Console.ReadKey();
                        break;
                }
            }

            //items in program
            while (Running)
            {
                Console.Clear();
                Console.WriteLine("Select Menu Option");
                Console.WriteLine("[1] : To Do List");
                Console.WriteLine("[2] : Expenses");
                Console.WriteLine("[0] : Exit");
                Menu = Console.ReadKey();
                Spacer(2);

                switch (Menu.Key)
                {
                    case ConsoleKey.D1: //Go to ToDo List
                    case ConsoleKey.NumPad1:

                        break;

                    case ConsoleKey.D2: //Go to Expenses
                    case ConsoleKey.NumPad2:

                        break;

                    case ConsoleKey.D0: //Exit the progrram
                    case ConsoleKey.NumPad0:
                        TerminateProgram();
                        return;

                    default:
                        Console.WriteLine("Invalid selection.\nPress any key to continue");
                        Console.ReadKey();
                        break;
                }

                Console.Clear();
                Console.WriteLine("Select SubMenu Option:");
                Console.WriteLine("[1] : Add item");
                Console.WriteLine("[2] : Remove Item");
                Console.WriteLine("[3] : Get item description");
                Console.WriteLine("[4] : Edit item in list");
                Console.WriteLine("[5] : Mark as completed");
                Console.WriteLine("[0] : Exit");

                SubMenu = Console.ReadKey();
                Spacer(2);

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
                        TerminateProgram();
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
