using System;
using System.Collections.Generic;

namespace jafleet.EF
{
    public partial class Aircraft
    {
        public string RegistrationNumber { get; set; }
        public string TypeCode { get; set; }
        public string RegisterDate { get; set; }
        public string OperationCode { get; set; }
        public string WifiCode { get; set; }
        public string Remarks { get; set; }
        public string SerialNumber { get; set; }
        public string UpdateTime { get; set; }
        public string CreationTime { get; set; }
        public string Airline { get; set; }
    }
}
