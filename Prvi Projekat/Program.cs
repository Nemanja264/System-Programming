using System;
using System.Threading;

namespace Prvi_Projekat
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var server = new WebServer("http://localhost:5050/");
                server.Start();
            }
            catch (Exception ex)
            {
                Log($"Doslo je do kriticne greske prilikom pokretanja servera: {ex.Message}");
            }
        }

        public static void Log(string message)
        {
            Console.WriteLine($"[Nit: {Thread.CurrentThread.ManagedThreadId}] {message}");
        }
    }
}
