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
            string modelPath = args.Length > 0 ? args[0] : "models/ml_prediction.zip";
            string trainingData = args.Length > 1 ? args[1] : "data/training_data.tsv";
            string prefix = "http://localhost:8080/";

            var server = new WebServer(prefix, modelPath, trainingData);
            server.Start();
            



            Console.ReadLine();
            await Task.CompletedTask;
        }
    }
}
