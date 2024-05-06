using System;

namespace Configurator
{
    internal static partial class Program
    {
        private static void Act(ref readonly UInt32 userChoice)
        {
            ClearUserSelectionArea(12);

            if (State.MMCServiceIsPresent)
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
        
        private static void ClearUserSelectionArea(Int32 fixLine)
        {
            Console.CursorTop = fixLine;

            for (Int32 i = 0; i < fixLine; ++i)
            {
                Console.WriteLine("                                                                                                                      ");
            }

            Console.CursorTop = fixLine;
        }
    }
}