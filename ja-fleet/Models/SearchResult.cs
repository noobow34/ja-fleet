using jafleet.Commons.EF;

namespace jafleet.Models
{
    public class SearchResult:BaseModel
    {
        public string SearchConditionKey { get; set; }
        public AircraftView[] ResultList { get; set; }

        public string ErrorMessage { get; set; }
    }
}
