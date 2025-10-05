using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Treci_Projekat.Infrastructure;
using Treci_Projekat.Services;
using Treci_Projekat.Core.Entities;


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
                var query = req.QueryString;

                if (!string.IsNullOrWhiteSpace(query["health"]))
                {
                    var payload = new { ok = true, ts = DateTime.UtcNow };
                    await WriteSuccessResponse(res, JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true }));
                    return;
                }

                var single = query["video"];
                var many = query["ids"];

                string[] ids = Array.Empty<string>();
                if (!string.IsNullOrWhiteSpace(single))
                    ids = new[] { single.Trim() };
                else if (!string.IsNullOrWhiteSpace(many))
                    ids = many.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                if (ids.Length == 0)
                {
                    var usage = new
                    {
                        error = "Provide ?video=<id> or ?ids=<id1,id2,...> (optional ?max=<n>, default 200)",
                        examples = new[] { "/?health=1", "/?video=dQw4w9WgXcQ&max=150", "/?ids=id1,id2,id3&max=200" }
                    };
                    await WriteAnyTypeOfError(res, 400, JsonSerializer.Serialize(usage, new JsonSerializerOptions { WriteIndented = true }));
                    return;
                }

                int max = 200;
                if (int.TryParse(query["max"], out var m))
                    max = Math.Clamp(m, 1, 2000);

                var summaries = new List<VideoSentimentSummary>();
                foreach (var videoId in ids.Distinct())
                {
                    var summary = await _yt.GetTopLevelComments(videoId)
                        .Take(max)
                        .Select(c =>
                        {
                            var (isPos, p) = _prediction.Predict(c.Text);
                            c.IsPositive = isPos;
                            c.Probability = p;
                            return c;
                        })
                        .Aggregate(new VideoSentimentSummary(videoId), (acc, c) => { acc.Add(c); return acc; })
                        .ToTask();

                    summaries.Add(summary);
                }

                var responseObj = new
                {
                    generatedAt = DateTime.UtcNow,
                    videos = summaries.Select(s => new
                    {
                        videoId = s.VideoId,
                        total = s.TotalComments,
                        positive = s.PositiveCount,
                        positiveRatio = Math.Round(s.PositiveRatio, 4),
                        topPositive = s.TopPositive is null ? null : new { text = s.TopPositive.Value.Text, score = s.TopPositive.Value.P },
                        topNegative = s.TopNegative is null ? null : new { text = s.TopNegative.Value.Text, score = s.TopNegative.Value.P }
                    })
                };

                await WriteSuccessResponse(res, JsonSerializer.Serialize(responseObj, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch (Exception)
            {
                await WriteAnyTypeOfError(res, 500, JsonSerializer.Serialize(new { error = "Internal server error" }, new JsonSerializerOptions { WriteIndented = true }));
            }
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
