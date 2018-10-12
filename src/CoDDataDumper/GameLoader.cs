using System;
using System.Collections.Generic;
using System.Diagnostics;
using PhilLibX;
using PhilLibX.IO;

namespace CoDDataDumper
{
    class GameLoader
    {
        /// <summary>
        /// Active Process Reader
        /// </summary>
        public static ProcessReader Reader { get; set; }

        /// <summary>
        /// Supported Games
        /// </summary>
        public static Dictionary<string, Tuple<Func<bool, bool>, bool>> Games = new Dictionary<string, Tuple<Func<bool, bool>, bool>>()
        {
            { "BlackOps4",      new Tuple<Func<bool, bool>, bool>(BlackOps4.Process,    true) },
            { "s2_mp64_ship",   new Tuple<Func<bool, bool>, bool>(WorldWarII.Process,   true) },
            { "s2_sp64_ship",   new Tuple<Func<bool, bool>, bool>(WorldWarII.Process,   false) },
        };

        /// <summary>
        /// Loads first matching game process and dumps supported assets
        /// </summary>
        public static void Load()
        {
            // Loop all processes and check against dict
            Process[] Processes = Process.GetProcesses();

            foreach(var process in Processes)
            {
                // Check process name against game list
                if(Games.ContainsKey(process.ProcessName))
                {
                    // Result
                    var game = Games[process.ProcessName];
                    // Info
                    Printer.WriteLine("INFO", String.Format("Found matching game {0}", process.ProcessName));
                    // Set Reader
                    Reader = new ProcessReader(process);
                    // Load Game
                    if (!game.Item1(game.Item2))
                        Printer.WriteLine("ERROR", "This game is supported, but this update is not.", ConsoleColor.Red);
                    // Done
                    return;
                }
            }

            // Failed
            Printer.WriteLine("ERROR", "Failed to find a supported game", ConsoleColor.Red);
        }
    }
}
