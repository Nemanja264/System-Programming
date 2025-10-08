using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DrugiProjekat
{
    /// <summary>
    /// Klasa odgovorna za asinhronu obradu pojedinacnog HTTP zahteva.
    /// </summary>
    public class RequestHandler
    {
        // Thread-safe recnik za kesiranje. Isti kao u prvom projektu, jer je ConcurrentDictionary
        // dizajniran za konkurentni pristup i bezbedan je za upotrebu sa async/await.
        private static readonly ConcurrentDictionary<string, string> Cache = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// Asinhrono obradjuje pojedinacni HTTP zahtev.
        /// </summary>
        public async Task ProcessRequestAsync(HttpListenerContext context)
        {
            if (context.Request.Url == null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.Close();
                return;
            }
            string fileName = context.Request.Url.AbsolutePath.Trim('/');
            
            // Ignorisemo zahteve za favicon.ico
            if (fileName.ToLower() == "favicon.ico")
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                context.Response.Close();
                return;
            }

            string filePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);
            Program.Log($"Primljen zahtev za fajl: {fileName}");

            string responseString;

            // Provera da li hash za trazeni fajl vec postoji u kesu.
            if (Cache.TryGetValue(filePath, out string? hash))
            {
                responseString = $"Odgovor iz KESA: SHA256 hash za '{fileName}' je: {hash}";
                Program.Log($"Hash za '{fileName}' pronadjen u kesu.");
            }
            else
            {
                // Ako nije u kesu, obradjujemo fajl asinhrono.
                responseString = await ProcessFileAsync(filePath, fileName, context);
            }

            try
            {
                // Priprema i slanje odgovora klijentu asinhrono.
                byte[] buffer = Encoding.UTF8.GetBytes(responseString);
                context.Response.ContentLength64 = buffer.Length;
                await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            }
            catch (Exception ex)
            {
                Program.Log($"Greska prilikom slanja odgovora: {ex.Message}");
            }
            finally
            {
                context.Response.OutputStream.Close();
            }
        }

        /// <summary>
        /// Asinhrono obradjuje fajl - cita ga, kriptuje i dodaje u kes.
        /// </summary>
        private async Task<string> ProcessFileAsync(string filePath, string fileName, HttpListenerContext context)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    // Asinhrono citanje fajla. Ovo je I/O operacija koja ne blokira nit.
                    byte[] fileBytes = await File.ReadAllBytesAsync(filePath);

                    // Kriptovanje je CPU-bound operacija. Za vrlo velike fajlove, moglo bi se
                    // razmisliti o Task.Run(() => ...), ali za prosecne velicine ovo je dovoljno brzo
                    // i nece znacajno blokirati nit iz ThreadPool-a.
                    using (SHA256 sha256 = SHA256.Create())
                    {
                        byte[] hashBytes = sha256.ComputeHash(fileBytes);
                        string hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                        
                        Cache.TryAdd(filePath, hash);

                        Program.Log($"Fajl '{fileName}' je uspesno kriptovan i dodat u kes.");
                        return $"Odgovor sa SERVERA: SHA256 hash za '{fileName}' je: {hash}";
                    }
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    Program.Log($"Greska: Fajl '{fileName}' nije pronadjen na putanji: {filePath}");
                    return $"GRESKA: Fajl '{fileName}' nije pronadjen.";
                }
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                Program.Log($"Greska prilikom obrade fajla '{fileName}': {ex.Message}");
                return $"GRESKA: Dogodila se greska na serveru: {ex.Message}";
            }
        }
    }
}
