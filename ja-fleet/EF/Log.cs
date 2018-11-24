using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace jafleet.EF
{
    public class Log
    {
        public int? LogId { get; set; }
        public string LogDate { get; set; }
        public string LogType { get; set; }
        public string LogDetail { get; set; }
        public string UserId { get; set; }
    }
}