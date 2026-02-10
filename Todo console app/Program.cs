// See https://aka.ms/new-console-template for more information

using System;
using Todo_console_app.Data;
using Microsoft.Data.Sqlite;

namespace TestConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //create variables
            System.Console.WriteLine("Starting...");

            //create instance
            Data.Initialise(); //double check, how to test
            Console.Write("Database initialised.");
            //populate / read data


            //interact with data
        }
    }
}
