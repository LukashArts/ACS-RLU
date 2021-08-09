using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts
{
    class Log
    {
        public static string directory = "LogGame/";
        public static string filepath = "";
        public static DateTime now = DateTime.Now;

        public static void CheckIfDirectoryExists()
        {
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
        }

        public static void CreateFile()
        {
            CheckIfDirectoryExists();

            filepath = directory + "server_" + now.Year + now.Month + now.Day + now.Hour + now.Minute + now.Second + ".txt";
            File.AppendAllText(filepath, "File created (date): " + now + "\n");
        }

        public static void Write(string text)
        {
            File.AppendAllText(filepath, text + "\n");
        }
    }
}
