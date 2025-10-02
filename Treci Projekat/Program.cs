using System;
using System.Threading.Tasks;
using Google.Apis.YouTube.v3;
using Google.Apis.Auth.OAuth2;
using Google.Apis.YouTube.v3.Data;
using Google.Apis.Services;
using Treci_Projekat.Services;


namespace Treci_Projekat // optional—use your project’s namespace
{
    internal static class Program
    {
        public static async Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: dotnet run -- <YoutubeVideoId> [<YoutubeVideoId>,...]"
                return;
            }
            
            var apiKey = ApiKeyProvider.getApiKey();
            var yt = new YouTubeService(new BaseClientService.Initializer
            {

                ApiKey = apiKey,
                ApplicationName = "TreciProjekat"
            });

            Console.WriteLine(apiKey);
            Console.ReadLine();
            await Task.CompletedTask;
        }
    }
}
