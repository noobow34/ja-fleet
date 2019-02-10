using jafleet.Manager;
using System.Collections.Generic;
using System.Net.Http;

namespace jafleet.Util
{
    public static class LineUtil
    {
        public static string NotifyEndpoint { get; set; }

        public static void PushMe(string message)
        {
            HttpClient hc = HttpClientManager.GetInstance();
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "message", message }
            });
            hc.PostAsync(NotifyEndpoint, content);
        }
    }
}
