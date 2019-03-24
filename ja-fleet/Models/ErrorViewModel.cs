﻿using System;

namespace jafleet.Models
{
    public class ErrorViewModel:BaseModel
    {
        public string RequestId { get; set; }

        public Exception Ex { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}