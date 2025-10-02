using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Treci_Projekat.Services
{
    public sealed class YouTubeSettings
    {
        public string ApiKey { get; set; } = "";
        public string BaseUrl { get; set; } = "https://www.googleapis.com/youtube/v3";

        public string BuildCommentsEndpoint(string videoId, string? pageToken = null)
        {
            string token = string.IsNullOrWhiteSpace(pageToken) ? "" : $"&pageToken={pageToken}";

            return $"{BaseUrl}/commentThreads"
                + $"?part=snippet&textFormat=plainText&maxResults=100&order=time"
                + $"&videoId={videoId}&key={ApiKey}{token}";
        }
    }
}
