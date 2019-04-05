using System.Net.Http;

namespace jafleet.Manager
{
    public static class HttpClientManager
    {
        private static HttpClient _client = new HttpClient();

        public static HttpClient GetInstance()
        {
            return _client;
        }
    }
}
