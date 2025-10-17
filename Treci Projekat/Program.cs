using System;
using System.Threading.Tasks;
using Google.Apis.YouTube.v3;
using Google.Apis.Auth.OAuth2;
using Google.Apis.YouTube.v3.Data;
using Google.Apis.Services;
using Treci_Projekat.Services;


namespace Treci_Projekat
{
    internal static class Program
    {
        public static async Task Main(string[] args)
        {
            string root = Directory.GetCurrentDirectory();
            string modelPath = Path.Combine(root, "data", "sentimentModel.zip");
            string trainingData = Path.Combine(root, "data", "sentiment.tsv");
            string prefix = "http://localhost:5055/";

            var server = new WebServer(prefix, modelPath, trainingData);
            server.Start();

            Console.WriteLine($"Server running at {prefix}");
            Console.WriteLine("PRESS ENTER TO STOP");
            Console.ReadLine();
            await Task.CompletedTask;
        }
    }
}
