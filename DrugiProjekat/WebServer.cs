using System;
using System.Net;
using System.Threading.Tasks;
using System.Linq;

namespace DrugiProjekat
{
    /// <summary>
    /// Asinhroni web server.
    /// Odgovoran za pokretanje, zaustavljanje i prihvatanje dolazecih HTTP zahteva.
    /// </summary>
    public class WebServer
    {
        private readonly HttpListener _listener = new HttpListener();
        private readonly RequestHandler _requestHandler = new RequestHandler();

        public WebServer(string prefix)
        {
            if (!HttpListener.IsSupported)
            {
                throw new NotSupportedException("HttpListener nije podrzan na ovom sistemu.");
            }
            
            _listener.Prefixes.Add(prefix);
        }

        /// <summary>
        /// Pokrece glavnu petlju servera koja asinhrono ceka na zahteve.
        /// </summary>
        public async Task StartAsync()
        {
            _listener.Start();
            Program.Log($"Web server je pokrenut na {_listener.Prefixes.First()}");
            Program.Log("Cekam na zahteve...");

            while (true)
            {
                try
                {
                    // Asinhrono cekanje na dolazeci zahtev. Ne blokira nit dok ceka.
                    HttpListenerContext context = await _listener.GetContextAsync();

                    // Kada zahtev stigne, njegova obrada se delegira kao novi Task.
                    // Koristimo "fire and forget" pristup (_ = ...) da bi server odmah nastavio
                    // sa cekanjem na sledeci zahtev, ne cekajuci da se prethodni obradi.
                    _ = _requestHandler.ProcessRequestAsync(context);
                }
                catch (HttpListenerException ex)
                {
                    // Ova greska se moze desiti ako se listener zaustavi, sto je ocekivano.
                    Program.Log($"HttpListener je zaustavljen: {ex.Message}");
                    break;
                }
                catch (Exception ex)
                {
                    Program.Log($"Neocekivana greska u petlji servera: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Zaustavlja server.
        /// </summary>
        public void Stop()
        {
            _listener.Stop();
            _listener.Close();
        }
    }
}
