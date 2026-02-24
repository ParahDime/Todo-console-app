// See https://aka.ms/new-console-template for more information

using Microsoft.Data.Sqlite;
using System;
using System.IO.Pipes;
using System.Runtime.CompilerServices;
using Todo_console_app.Data;
using Todo_console_app.Users;
using Todo_console_app.Updates;
using static System.Runtime.InteropServices.JavaScript.JSType;
//for hashing passswords

namespace TestConsoleApp
{
   
    internal class Program
    {

        //print all information on the table
        public static void PrintTable(List<Dictionary <string, object>> TableData, string TableName)
        {
            if (TableData.Count == 0) //empty table
            {
                Console.WriteLine("No data found.");
                return;
            }

            string[] columns;
            if (TableName == "ActionsToDo")
            {
                columns = new string[] { "Id", "Title", "Is_Complete" };
            }
            else if (TableName == "Expenses")
            {
                columns = new string[] { "Id", "Title", "Amount" };
            }
            else
            {
                //if table is unknown
                columns = new string[] { "Id", "Title" };
            }

            foreach (var col in columns)
            {
                Console.Write($"{col,-15} | ");
            }
            Console.WriteLine("\n");

            foreach (var row in TableData)
            {
                foreach (var col in columns)
                {
                    if (!row.ContainsKey(col)) continue;

                    object value = row[col];
                    string output = value?.ToString() ?? "N/A";

                    if (col == "Is_Complete")
                    {
                        int completeValue = Convert.ToInt32(value);
                        output = (completeValue == 1) ? "Yes" : "No";
                    }

                    if (col == "Amount")
                    {
                        output = $"£{Convert.ToDouble(value):F2}";
                    }

                    Console.Write($"{output,-15} | ");
                }
                Console.WriteLine();
            }

        }

        public static void DisplayItemDetails(Dictionary<string, object> ItemData)
        {
            if (ItemData == null || ItemData.Count == 0)
            {
                Console.WriteLine("No item found with that ID.");
                return;
            }

            Console.WriteLine("\n--- ITEM DETAILS ---");

            foreach (var entry in ItemData)
            {
                string label = entry.Key.Replace("_", " ");
                label = char.ToUpper(label[0]) + label.Substring(1);

                string displayValue;
                if (entry.Key.ToLower() == "is_complete")
                {
                    displayValue = Convert.ToInt32(entry.Value) == 1 ? "Yes" : "No";
                }
                else
                {
                    displayValue = entry.Value?.ToString() ?? "N/A";
                }

                Console.WriteLine($"{label,-15}: {displayValue}");
            }
            Console.WriteLine("---------------------\n");
        }
        public static void Buffer(string prompt)
        {
            Console.WriteLine($"{prompt}. Press any key to continue.");
            Console.ReadKey();
        }

        //used when empty strings not allowed
        static string ReadRequiredInput(string prompt)
        {
            string? Input;

            do
            {
                Console.WriteLine(prompt);
                Input = Console.ReadLine();
            }
            while (string.IsNullOrWhiteSpace(Input));

            return Input;
        }

        static char ReadRequiredChar(string prompt)
        {
            string Input;
            char ParseInput = ' ';

            do
            {
                Console.WriteLine(prompt);
                Input = Console.ReadLine();
                try
                {
                    ParseInput = Input[0];
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            } while (ParseInput != 'y' && ParseInput != 'n');

            return ParseInput;
        }

        static int ReadRequiredInt(string prompt)
        {
            string Input;
            int ParseInput;
            bool IsValid = false;

            do
            {
                Console.WriteLine(prompt);
                Input = Console.ReadLine();
                IsValid = int.TryParse(Input, out ParseInput);
            } while (!IsValid);

            return ParseInput;
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
                Buffer("Invalid Credentials.");
                return null;
            }
        }

        static void CreateAcc()
        {
            //get user details
            string Username = ReadRequiredInput("Please enter a username:    ");
            string Password = ReadRequiredInput("Please enter a password:    ");

            //input into sql database
            Data.createUser(Username, Password);
            Buffer($"User {Username} has been initialised");
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
                        Buffer("Invalid selection");
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
                            Buffer("\n\nInvalid selection");
                            break;
                    }

                    Console.Clear();
                }

                //print all items
                TableData = Data.GetAllItems(TableName, person.Id);
                PrintTable(TableData, TableName);

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
                char YesNo = ' ';
                int NumberId = 0;
                int Amount;
                Frequency frequency; //expenses handler

                switch (SubMenu.Key)
                {
                    case ConsoleKey.D1: //Add Item
                    case ConsoleKey.NumPad1:
                        Console.WriteLine("[1] : Add item\n");
                        Name = ReadRequiredInput("Enter item name: ");
                        if (TableName == "ActionsToDo")
                        {

                            Description = ReadRequiredInput("Enter the item description: ");
                            Console.WriteLine($"Name: {Name}\n Description: {Description}");
                        }
                        else //table name == expenses
                        {
                            Amount = ReadRequiredInt("Enter the amount (in £): ");
                            //frequency = ReadRequiredInt("Enter how often the expense occurs\n[1] Daily\n[2] Weekly\n[3] Monthly");
                            Console.WriteLine($"Name: {Name}\n Amount: {Amount} \n Frequency: ");

                        }
                            YesNo = ReadRequiredChar("Are these details correct? y/n");
                        
                         if(YesNo == 'y')
                         {
                            if(Data.AddToDoItem(Name, Description, person))
                            { 
                                Buffer($"{Name} was successfully added to the database");
                            }
                            else
                            {
                                Buffer("An error occured");
                            }

                         }
                         else //No or not the correct input
                         {
                            Buffer("Input was not added");
                         }
                         
                        break;
                    case ConsoleKey.D2: //Remove Item
                    case ConsoleKey.NumPad2:
                        Console.WriteLine("[2] : Remove Item\n");
                        int removeNum = ReadRequiredInt("Enter item ID you wish to remove: ");
                        //input number
                        YesNo = ReadRequiredChar($"Remove item {removeNum}? \n y/n");
                        if(YesNo == 'y')
                        {
                            if (Data.RemoveItem(TableName, removeNum)) 
                            {
                                Buffer($"{Name} was removed successfully");
                            }
                            else
                            {
                                Buffer("An error occured");
                            }
                        }
                        else //no / not proper input
                        {
                            Buffer("Input was not removed");
                        }
                        
                        break;
                    case ConsoleKey.D3: //Get Item Description
                    case ConsoleKey.NumPad3:
                        Console.WriteLine("[3] : Get item description\n");
                        NumberId = ReadRequiredInt("Please enter item number: ");

                        Dictionary<string, object>  ItemInfo = Data.GetItemData(NumberId, TableName);

                        DisplayItemDetails(ItemInfo);
                        Buffer("");
                        break;
                    case ConsoleKey.D4: //Edit Item
                    case ConsoleKey.NumPad4:
                        Console.WriteLine("[4] : Edit item in list\n");
                        NumberId = ReadRequiredInt("Select item number you wish to edit: ");

                        //select data to modify
                        Console.WriteLine("Select which part you would like to edit: ");

                        //confirm

                        //yn
                        break;
                    case ConsoleKey.D5 when TableName == "ActionsToDo": //Mark as completed
                    case ConsoleKey.NumPad5 when TableName == "ActionsToDo":
                        Console.WriteLine("[5] : Mark as completed\n");
                        NumberId = ReadRequiredInt("Enter ID number you want to mark as completed");

                        UpdateResult update = Data.MarkCompleted(NumberId, person);
                        switch (update)
                        {
                            case UpdateResult.Success:
                                Buffer("Task marked as completed");
                                break;

                            case UpdateResult.NotFound:
                                Buffer("Task was not found");
                                break;

                            case UpdateResult.AlreadyCompleted:
                                Buffer("Task already marked as completed");
                                break;

                            case UpdateResult.Error:
                                Buffer("A database error occurred.");
                                break;
                        }
                        break;
                    case ConsoleKey.D6 when TableName == "ActionsToDo": //Remove completed items
                    case ConsoleKey.NumPad6 when TableName == "ActionsToDo":
                        Console.WriteLine("[6] : Remove completed items\n");
                        YesNo = ReadRequiredChar("Do you wish to remove completed items? y'n");
                        if(YesNo == 'y')
                        {
                            if (Data.RemoveCompletedItems(person))
                            {
                                Buffer("Completed items were removed successfully");
                            }
                            else
                            {
                                Buffer("No items were deleted");
                            }
                        }
                        else 
                        {
                            Buffer("Item was not removed.");
                        }
                        
                        break;
                    case ConsoleKey.D7 when TableName == "ActionsToDo": //Recent items added
                    case ConsoleKey.NumPad7 when TableName == "ActionsToDo":
                        Console.WriteLine("[7] : Recent items added\n");

                        Data.ShowRecentItems(person); //query, sort by date, then output

                        Buffer("");
                        
                        break;
                    case ConsoleKey.D8 when TableName == "ActionsToDo": //Calc list completion
                    case ConsoleKey.NumPad8 when TableName == "ActionsToDo":
                        Console.WriteLine("[8] : Calculate current list completion\n");

                        double CompletionAmount = Data.GetCompletedAmount(person);
                        if (CompletionAmount > 0)
                        {
                            Buffer($"{CompletionAmount}% of all tasks are marked as completed");
                        }
                        else
                        {
                            Buffer("An error occured");
                        }
                            break;
                    case ConsoleKey.D5 when TableName == "Expenses": //Mark as completed
                    case ConsoleKey.NumPad5 when TableName == "Expenses":
                        Console.WriteLine("[5] : Total Monthly spend\n");
                        Console.WriteLine("Enter ID number you want to mark as completed");
                        break;
                    case ConsoleKey.D6 when TableName == "ActionsToDo": //Remove completed items
                    case ConsoleKey.NumPad6 when TableName == "ActionsToDo":
                        Console.WriteLine("[6] : Sort items by cost per month\n");
                        Console.WriteLine("Do you wish to remove completed items? y'n");
                        break;
                    case ConsoleKey.D0: //Exit the progrram
                    case ConsoleKey.NumPad0:
                        Buffer("Returning to main menu");
                        break;
                    default:
                       Buffer("Invalid selection");
                        break;
                }
                TableName = null;
                Console.Clear();
            }
        }
    }
}
