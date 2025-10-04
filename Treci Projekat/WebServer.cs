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
using System.Text.Json;


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
        private readonly PredictionService _prediction;

        public WebServer(string prefix, string modelPath, string trainingDataPath)
        {
            var apiKey = ApiKeyProvider.getApiKey();
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("Missing YOUTUBE_API_KEY environment variable.");

            _ytSettings = new YouTubeSettings { ApiKey = apiKey };
            _yt = new YouTubeCommentsService(_ytSettings);
            _prediction = PredictionService.Create(modelPath, trainingDataPath);

            _listener.Prefixes.Add(prefix);

            var stopSignal = Observable.FromEvent(h => _cts.Token.Register(h), h => { });

            _requestStream =
                Observable.Defer(() => Observable.FromAsync(_listener.GetContextAsync))
                          .Repeat()                                 
                          .TakeUntil(stopSignal)
                          .ObserveOn(TaskPoolScheduler.Default)     
                          .SelectMany(ctx =>
                              Observable.FromAsync(async () =>
                              {
                                  Logger.Info($"[START] {ctx.Request.Url} - nit: {Thread.CurrentThread.ManagedThreadId}");
                                  await HandleRequest(ctx);         
                                  Logger.Info($"[END] nit: {Thread.CurrentThread.ManagedThreadId}");
                                  return ctx;
                          })
                          );
        }


        public void Start()
        {
            _listener.Start();
            Logger.Info("Web server started.");

            _subscription = _requestStream.Subscribe(
                _ => { },
                ex => Logger.Error($"[PIPELINE ERROR] {ex.Message}"),
                () => Logger.Info("Web server stopped.")
            );
        }

        public void Stop()
        {
            _cts.Cancel();
            _listener.Stop();
            _subscription?.Dispose();
            Logger.Info("Server stopped");
        }

        private async Task HandleRequest(HttpListenerContext context)
        {
            var req = context.Request;
            var res = context.Response;
            try
            {
                var video = req.QueryString["video"];
                var idsParam = req.QueryString["ids"];
            }
            catch { }
        }

        private static async Task WriteSuccessResponse(HttpListenerResponse response, string jsonResponse)
        {
            response.StatusCode = 200;
            response.ContentType = "application/json; charset=utf-8";
            byte[] buffer = Encoding.UTF8.GetBytes(jsonResponse);
            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            response.OutputStream.Close();
        }
        
        private static async Task WriteAnyTypeOfError(HttpListenerResponse response, int status, string errorMessage)
        {
            response.StatusCode = status;
            response.ContentType = "application/json; charset=utf-8";
            string json = JsonSerializer.Serialize(new { error = errorMessage });
            byte[] buffer = Encoding.UTF8.GetBytes(json);
            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            response.OutputStream.Close();
        }
    }
}
