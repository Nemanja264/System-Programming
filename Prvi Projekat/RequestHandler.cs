using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Prvi_Projekat
{
    
    public class RequestHandler
    {
        
        private static readonly ConcurrentDictionary<string, string> Cache = new ConcurrentDictionary<string, string>();

        public void ProcessRequest(HttpListenerContext context)
        {
            if (context.Request.Url == null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }
            string fileName = context.Request.Url.AbsolutePath.Trim('/');

            if (fileName.ToLower() == "favicon.ico")
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                context.Response.Close();
                return;
            }

            string filePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);

            Program.Log($"Primljen zahtev za fajl: {fileName}");

            string responseString;
            string? hash;

            if (Cache.TryGetValue(filePath, out hash))
            {
                context.Response.StatusCode = (int)HttpStatusCode.OK; 
                responseString = $"Odgovor iz KESA: SHA256 hash za '{fileName}' je: {hash}";
                Program.Log($"Hash za '{fileName}' pronadjen u kesu.");
            }
            else
            {
                responseString = ProcessFile(filePath, fileName, context);
            }

            
            try
            {
                byte[] buffer = Encoding.UTF8.GetBytes(responseString);
                context.Response.ContentLength64 = buffer.Length;
                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
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

        
        private string ProcessFile(string filePath, string fileName, HttpListenerContext context)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    byte[] fileBytes = File.ReadAllBytes(filePath);

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
