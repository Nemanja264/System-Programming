using System;
using System.Threading.Tasks;

namespace DrugiProjekat
{
    /// <summary>
    /// Glavna klasa aplikacije.
    /// </summary>
    class Program
    {
        /// <summary>
        /// Ulazna tacka programa. Kreira i pokrece WebServer asinhrono.
        /// </summary>
        static async Task Main(string[] args)
        {
            // Definisemo prefiks na kojem ce server slusati.
            string prefix = "http://localhost:5050/";
            WebServer server = new WebServer(prefix);

            try
            {
                // Pokrecemo server i cekamo da se zavrsi (sto se u ovom slucaju nece desiti
                // jer server radi u beskonacnoj petlji).
                await server.StartAsync();
            }
            catch (Exception ex)
            {
                Log($"Greska u radu servera: {ex.Message}");
            }
        }

        /// <summary>
        /// Pomocna metoda za logovanje poruka na konzolu.
        /// Thread-safe je jer koristi Console.WriteLine koji je interno sinhronizovan.
        /// </summary>
        public static void Log(string message)
        {
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}");
        }
    }
}
