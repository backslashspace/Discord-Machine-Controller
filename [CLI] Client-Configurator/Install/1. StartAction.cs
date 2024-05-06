using System;

namespace Configurator
{
    internal static partial class Program
    {
        private static void Act(ref readonly UInt32 userChoice)
        {
            ClearUserSelectionArea();

            if (State.ServiceIsPresent)
            {
                switch (userChoice)
                {
                    case 1:
                        UnInstall();
                        Extract();
                        RegisterService();
                        CreateConfig();
                        return;

                    case 2:
                        UnInstall();
                        return;

                    case 3:
                        CreateConfig();
                        break;

                    case 4:
                        return;
                }
            }
            else
            {
                switch (userChoice)
                {
                    case 1:
                        UnInstall();
                        Extract();
                        RegisterService();
                        CreateConfig();
                        return;

                    case 2:
                        return;
                }
            }
        }

        //
        
        private static void ClearUserSelectionArea()
        {
            Console.CursorTop = 12;

            for (Int32 i = 0; i < 6; ++i)
            {
                Console.WriteLine("                                              ");
            }

            Console.CursorTop = 12;
        }
    }
}