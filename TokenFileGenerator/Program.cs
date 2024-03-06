using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace TokenFileGenerator
{
    internal class Program
    {
        private static String version;
        private static String path;

        private static void Main(String[] args)
        {
            InitWindow();

            String rawToken = GetToken();

            WriteResultToDisk(ref rawToken);

            Console.Write("Done\n\npress return to exit: ");
            Console.ReadLine();
        }

        private static void InitWindow()
        {
            path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            Console.Title = "Master token generator v" + version;
        }

        private static String GetToken()
        {
            Console.Write("Trailing whitespaces will be removed.\n\nEnter bot token: ");

            return Console.ReadLine();
        }

        private static void WriteResultToDisk(ref String rawToken)
        {
            rawToken = rawToken.Trim();

            Console.WriteLine($"Using: '{rawToken}'\n");

            Byte[] tokenBytes = Encoding.UTF8.GetBytes(rawToken);
            String encodedToken = Convert.ToBase64String(tokenBytes);
            Byte[] rawEncodedToken = Encoding.UTF8.GetBytes(encodedToken);

            using FileStream fileStream = new(path + "\\token", FileMode.Create, FileAccess.Write, FileShare.None);

            fileStream.Write(rawEncodedToken, 0, rawEncodedToken.Length);
        }
    }
}