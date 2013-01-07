using System;

namespace FieldPong
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (FieldPong game = new FieldPong())
            {
                game.Run();
            }
        }
    }
}

