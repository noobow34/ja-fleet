using jafleet.Commons.EF;

namespace jafleet.Models
{
    public class AircraftDetailModel : BaseModel
    {
        public bool NeedBack { get; set; }
        public string Reg { get; set; }
        public AircraftView AV { get; set; }
        public string AirlineGroupNmae { get; set; }
        public string OgImageUrl { get; set; }
    }
}
