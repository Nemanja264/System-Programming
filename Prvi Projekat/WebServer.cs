using System;
using System.Net;
using System.Threading;

namespace Prvi_Projekat
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

        public void Start()
        {
            _listener.Start();
            Program.Log($"Web server je pokrenut na {_listener.Prefixes.First()}");
            Program.Log("Cekam na zahteve...");

            while (true)
            {
                HttpListenerContext context = _listener.GetContext();

                ThreadPool.QueueUserWorkItem(o => _requestHandler.ProcessRequest(context));
            }
        }
    }
}
