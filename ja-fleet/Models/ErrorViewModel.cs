using System;

namespace jafleet.Models
{
    public class ErrorViewModel
    {
        public string RequestId { get; set; }

        public Exception Ex { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}