using System.ComponentModel.DataAnnotations;
using System;

namespace jafleet.Models
{
    public class AircraftDetailModel
    {
        public Boolean NoHead { get; set; }
        public Boolean IsAdmin { get; set; }
        public String Reg { get; set; }
    }
}
