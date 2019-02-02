using Newtonsoft.Json;
using System;
using System.ComponentModel;

namespace jafleet.Models
{
    public class SearchConditionInModel
    {
        [DefaultValue(null)]
        public String RegistrationNumber { get; set; }

        [DefaultValue(null)]
        public String Airline { get; set; } = String.Empty;

        [DefaultValue(null)]
        public String OperationCode { get; set; }

        [DefaultValue(null)]
        public String WiFiCode { get; set; }

        [DefaultValue(null)]
        public String RegistrationDate { get; set; }
        [DefaultValue("0")]
        public String RegistrationSelection { get; set; }

        [DefaultValue(null)]
        public String Remarks { get; set; } = String.Empty;

        [DefaultValue(null)]
        public string TypeDetail { get; set; }

        public override string ToString()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this, Formatting.Indented, new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore }); ;
        }
    }
}
