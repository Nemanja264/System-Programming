using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Reactive.Linq;
using Treci_Projekat.Core.Entities;

namespace Treci_Projekat.Services
{
    public sealed class YouTubeCommentsService : IDisposable
    {
        private readonly YouTubeSettings _settings;
        private readonly HttpClient _httpClient = new();

        public YouTubeCommentsService(YouTubeSettings settings) => _settings = settings;
        public IObservable<YouTubeComment> GetTopLevelComments(string videoId)
        {

        }
        public void Dispose() => _httpClient.Dispose();
    }
}
