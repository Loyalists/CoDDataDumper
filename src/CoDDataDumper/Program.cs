using System;
using System.Diagnostics;
using PhilLibX;

namespace CoDDataDumper
{
    /// <summary>
    /// Main Program Class
    /// </summary>
    class Program
    {
        /// <summary>
        /// Main Entry Point
        /// </summary>
        /// <param name="args">Arguments</param>
        static void Main(string[] args)
        {
            // Call Updater
            try
            {
                Process.Start("CoDDataDumperUpdater.exe", "Scobalula CoDDataDumper CoDDataDumper coddatadumper.exe false");
            }
            catch { }

            // Set Console Data
            Printer.SetPrefixBackgroundColor(ConsoleColor.DarkBlue);
            Console.Title = "CoDDataDumber - 0.1.0";

            // Initial Print
            Printer.WriteLine("INIT", "-------------------------------------");
            Printer.WriteLine("INIT", "CoDDataDumper - By Scobalula");
            Printer.WriteLine("INIT", "Dumps some stuffs to help Mod Tools Users");
            Printer.WriteLine("INIT", "Version 0.1.0 - Doggo Enhanced");
            Printer.WriteLine("INIT", "-------------------------------------");

            // Launch Game Loader
            GameLoader.Load();

            // DOne
            Printer.WriteLine("INIT", "Execution complete, press Enter to exit.");
            Console.ReadKey();
        }
    }
}
