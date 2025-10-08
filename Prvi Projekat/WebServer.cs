using System;
using System.Net;
using System.Threading;

namespace Prvi_Projekat
{
    /// <summary>
    /// Glavna klasa servera. Odgovorna za pokretanje, zaustavljanje
    /// i prihvatanje dolazecih HTTP zahteva.
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
            
            // Dodavanje prefiksa (npr. "http://localhost:5050/") koji ce server slusati.
            _listener.Prefixes.Add(prefix);
        }

        /// <summary>
        /// Pokrece glavnu petlju servera koja ceka na zahteve.
        /// </summary>
        public void Start()
        {
            _listener.Start();
            Program.Log($"Web server je pokrenut na {_listener.Prefixes.First()}");
            Program.Log("Cekam na zahteve...");

            while (true)
            {
                // Cekanje na dolazeci zahtev (blokirajuca operacija).
                HttpListenerContext context = _listener.GetContext();

                // Kada zahtev stigne, njegova obrada se delegira ThreadPool-u.
                ThreadPool.QueueUserWorkItem(o => _requestHandler.ProcessRequest(context));
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
