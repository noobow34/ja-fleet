using System.ComponentModel.DataAnnotations;
using System;

namespace jafleet.Models
{
    public class AircraftDetailModel:BaseModel
    {
        public Boolean NeedBack { get; set; }
        public String Reg { get; set; }
    }
}
