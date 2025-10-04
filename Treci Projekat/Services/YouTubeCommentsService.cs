using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Treci_Projekat.Core.Entities;
using Treci_Projekat.Infrastructure;

namespace Treci_Projekat.Services
{
    public sealed class YouTubeCommentsService : IDisposable
    {
        private readonly YouTubeSettings _settings;
        private readonly HttpClient _httpClient = new();

        public YouTubeCommentsService(YouTubeSettings settings) => _settings = settings;

        private async Task FetchCommentsLoopAsync(string videoId, IObserver<YouTubeComment> observer, CancellationToken ct)
        {
            string? pageToken = null;

            do
            {
                ct.ThrowIfCancellationRequested();

                var url = _settings.BuildCommentsEndpoint(videoId, pageToken);
                using var resp = await _httpClient.GetAsync(url, ct).ConfigureAwait(false);
                resp.EnsureSuccessStatusCode();

                var json = await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
                var root = JObject.Parse(json);

                if (root["items"] is JArray items)
                {
                    foreach (var it in items)
                    {
                        ct.ThrowIfCancellationRequested();

                        var sn = it["snippet"]?["topLevelComment"]?["snippet"];
                        if (sn == null) continue;

                        observer.OnNext(new YouTubeComment
                        {
                            VideoId = videoId,
                            Text = sn.Value<string>("textOriginal") ?? sn.Value<string>("textDisplay") ?? "",
                            Author = sn.Value<string>("authorDisplayName") ?? "",
                            CommentId = it["snippet"]?["topLevelComment"]?["id"]?.ToString() ?? it["id"]?.ToString()
                        });
                    }
                }

                pageToken = root.Value<string>("nextPageToken");
            }
            while (!string.IsNullOrWhiteSpace(pageToken));
        }

        public IObservable<YouTubeComment> GetTopLevelComments(string videoId)
        {
            return Observable.Create<YouTubeComment>(observer =>
            {
                var cts = new CancellationTokenSource();
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await FetchCommentsLoopAsync(videoId, observer, cts.Token).ConfigureAwait(false);
                        if(!cts.IsCancellationRequested) observer.OnCompleted();
                    }
                    catch (Exception ex)
                    {
                        observer.OnError(ex);
                    }
                }, cts.Token);

                return () => cts.Cancel();
            });
        }
        public void Dispose() => _httpClient.Dispose();
    }
}
