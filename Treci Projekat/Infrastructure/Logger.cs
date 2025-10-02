using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Treci_Projekat.Infrastructure
{
    public static class Logger
    {
        private static readonly object _logLock = new();

        public static void Info(string message) => Log("INFO", message);
        public static void Error(string message) => Log("Error", message);


        public static void Log(string logLevel, string message)
        {
            lock (_logLock)
            {
                string dt = DateTime.Now.ToString("HH:mm:ss");
                Console.WriteLine($"[{dt}] [{logLevel}] {message}");
            }
        }
    }
}
