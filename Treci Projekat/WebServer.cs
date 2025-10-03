using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Treci_Projekat.Infrastructure;
using Treci_Projekat.Services;
using Treci_Projekat.Core;

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

        public WebServer(string prefix)
        {
            var apiKey = ApiKeyProvider.getApiKey();

            _ytSettings = new YouTubeSettings { ApiKey = apiKey };
        }


        public void Start()
        {
            try
            {
                _listener.Start();
                Logger.Info($"Web server started at");

                _subscription = _requestStream
                    .ObserveOn(TaskPoolScheduler.Default)
                    .SelectMany(ctx => Observable.FromAsync(() => HandleRequest(ctx)))
                    .Subscribe(_ => { },
                        ex => Logger.Error(ex, "[PIPELINE ERROR]"),
                        () => Logger.Info("Server completed")
                    );
            }
            catch (HttpListenerException ex) { Logger.Error($"Failed to start HttpListener: {ex.Message}"); throw; }
        }

        public void Stop()
        {
            _cts.Cancel();
            _subscription?.Dispose();
            _listener.Stop();
            Logger.Info("Server stopped");
        }
    }
}
