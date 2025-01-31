namespace jafleet.Manager
{
    public static class HttpClientManager
    {
        private static HttpClient _client = new();

        public static HttpClient GetInstance()
        {
            return _client;
        }
    }
}
