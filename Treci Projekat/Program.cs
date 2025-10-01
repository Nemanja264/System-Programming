using System;
using System.Threading.Tasks;
using Google.Apis.YouTube.v3;
using Google.Apis.Auth.OAuth2;
using Google.Apis.YouTube.v3.Data;
using Google.Apis.Services;


namespace TreciProjekat // optional—use your project’s namespace
{
    internal static class Program
    {
        public static async Task Main(string[] args)
        {
            DotNetEnv.Env.TraversePath().Load();
            string ApiKey = Environment.GetEnvironmentVariable("YOUTUBE_API_KEY")
                ?? throw new InvalidOperationException("Missing YOUTUBE_API_KEY");

            var yt = new YouTubeService(new BaseClientService.Initializer
            {

                ApiKey = ApiKey,
                ApplicationName = "TreciProjekat"
            });

            Console.WriteLine(ApiKey);
            Console.ReadLine();
            await Task.CompletedTask;
        }
    }
}
