using System.ComponentModel.DataAnnotations;
using System;

namespace jafleet.Models
{
    public class AircraftModel:BaseModel
    {
        [Display(Name = "2018/09以降の抹消済も表示")]
        public Boolean IncludeRetire { get; set; }
    }
}
