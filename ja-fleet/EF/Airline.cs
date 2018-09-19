using System;
using System.Collections.Generic;

namespace jafleet.EF
{
    public partial class Airline
    {
        public string AirlineCode { get; set; }
        public string AirlineGroupCode { get; set; }
        public string AirlineNameEn { get; set; }
        public string AirlineNameJp { get; set; }
        public string AirlineNameEnShort { get; set; }
        public string AirlineNameJpShort { get; set; }
        public string DisplayOrder { get; set; }
    }
}
