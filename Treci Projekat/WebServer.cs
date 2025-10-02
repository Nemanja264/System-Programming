using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Treci_Projekat.Infrastructure;

namespace Treci_Projekat
{
    public class WebServer
    {
        private readonly HttpListener _listener = new();
        private readonly CancellationTokenSource _cts = new();
        private readonly IObservable<HttpListenerContext> _requestStream;
        private IDisposable? _subscription;
        private readonly YouTubeSettings _ytSettings;
        private readonly YouTubeCommentsService _yt;
        private readonly SentimentService _sentiment;

    }
}
