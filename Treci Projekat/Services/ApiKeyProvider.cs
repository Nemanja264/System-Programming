using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Services;


namespace Treci_Projekat.Services
{
    internal class ApiKeyProvider
    {
        public static bool _loaded;

        public static string? getApiKey()
        {
            if (!_loaded)
            {
                DotNetEnv.Env.TraversePath().Load();
                _loaded = true;
            }

            string ApiKey = Environment.GetEnvironmentVariable("YOUTUBE_API_KEY")
                ?? throw new InvalidOperationException("Missing YOUTUBE_API_KEY");

            return ApiKey;
        }
    }
}
