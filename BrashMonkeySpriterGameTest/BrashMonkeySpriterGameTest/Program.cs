/*==========================================================================
 * Project: BrashMonkeySpriterGameTest
 * File: Program.cs
 *
 *==========================================================================
 * Author:
 * Geoff "NowSayPillow" Lodder
 *==========================================================================*/

using System;

namespace BrashMonkeySpriterGameTest {
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (Game1 game = new Game1())
            {
                game.Run();
            }
        }
    }
#endif
}

