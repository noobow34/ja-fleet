﻿using Newtonsoft.Json;
using System.ComponentModel;

namespace jafleet.Models
{
    public class SearchConditionInModel : BaseModel
    {
        [DefaultValue(null)]
        public string RegistrationNumber { get; set; }

        [DefaultValue(null)]
        public string Airline { get; set; } = String.Empty;

        [DefaultValue(null)]
        public string OperationCode { get; set; }

        [DefaultValue(null)]
        public string WiFiCode { get; set; }

        [DefaultValue(null)]
        public string RegistrationDate { get; set; }
        [DefaultValue("0")]
        public string RegistrationSelection { get; set; }

        [DefaultValue(null)]
        public string Remarks { get; set; } = String.Empty;
        [DefaultValue(null)]
        public string RemarksKeyword { get; set; } = String.Empty;

        [DefaultValue(null)]
        public string SpecialLivery { get; set; } = String.Empty;
        [DefaultValue(null)]
        public string SpecialLiveryKeyword { get; set; } = String.Empty;

        [DefaultValue(null)]
        public string TypeDetail { get; set; }

        public override string ToString()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this, Formatting.Indented, new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore });
        }
    }
}
