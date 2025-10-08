using System;
using System.Threading;

namespace Prvi_Projekat
{
    class Program
    {
        // Objekat za zakljucavanje pristupa konzoli.
        private static readonly object ConsoleLock = new object();

        static void Main(string[] args)
        {
            try
            {
                // Kreiranje instance servera i njegovo pokretanje.
                var server = new WebServer("http://localhost:5050/");
                server.Start();
            }
            catch (Exception ex)
            {
                Log($"Doslo je do kriticne greske prilikom pokretanja servera: {ex.Message}");
            }
        }

        /// <summary>
        /// Pomocna metoda za logovanje poruka na konzolu na thread-safe nacin.
        /// </summary>
        public static void Log(string message)
        {
            lock (ConsoleLock)
            {
                // Ispisuje poruku zajedno sa ID-jem niti koja je izvrsava.
                Console.WriteLine($"[Nit: {Thread.CurrentThread.ManagedThreadId}] {message}");
            }
        }
    }
}
