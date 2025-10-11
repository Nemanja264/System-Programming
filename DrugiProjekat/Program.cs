using System;
using System.Threading.Tasks;

namespace DrugiProjekat
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string prefix = "http://localhost:5050/";
            WebServer server = new WebServer(prefix);

            try
            {
                await server.StartAsync();
            }
            catch (Exception ex)
            {
                Log($"Greska u radu servera: {ex.Message}");
            }
        }

        public static void Log(string message)
        {
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}");
        }
    }
}
