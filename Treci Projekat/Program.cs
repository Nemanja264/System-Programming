using System;
using System.Threading.Tasks;
using Google.Apis.YouTube.v3;
using Google.Apis.Auth.OAuth2;
using Google.Apis.YouTube.v3.Data;


namespace CommentSentimentRx // optional—use your project’s namespace
{
    internal static class Program
    {
        // sync Main
        // public static void Main(string[] args)
        // {
        //     Console.WriteLine("Hello, World!");
        // }

        // or async Main (handy for HTTP calls, etc.)
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            Console.ReadLine();
            await Task.CompletedTask;
        }
    }
}
