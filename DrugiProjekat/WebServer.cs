using System;
using System.Net;
using System.Threading.Tasks;
using System.Linq;

namespace DrugiProjekat
{
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

        public async Task StartAsync()
        {
            _listener.Start();
            Program.Log($"Web server je pokrenut na {_listener.Prefixes.First()}");
            Program.Log("Cekam na zahteve...");

            while (true)
            {
                try
                {
                    HttpListenerContext context = await _listener.GetContextAsync();

                    _ = _requestHandler.ProcessRequestAsync(context);
                }
                catch (HttpListenerException ex)
                {
                    Program.Log($"HttpListener je zaustavljen: {ex.Message}");
                    break;
                }
                catch (Exception ex)
                {
                    Program.Log($"Neocekivana greska u petlji servera: {ex.Message}");
                }
            }
        }
    }
}
