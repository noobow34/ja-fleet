using System;

namespace jafleet.Models
{
    public class SearchCondition
    {
        public String RegistrationNumber { get; set; }

        public String Airline { get; set; } = String.Empty;

        public String Type { get; set; } = String.Empty;

        public String OperationCode { get; set; }

        public String WiFiCode { get; set; }

        public String RegistrationDate { get; set; }
        public String RegistrationSelection { get; set; }

        public String Remarks { get; set; } = String.Empty;

    }
}
