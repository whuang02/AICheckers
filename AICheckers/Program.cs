using System;

namespace AICheckers
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (CheckersMain game = new CheckersMain())
            {
                game.Run();
            }
        }
    }
#endif
}

