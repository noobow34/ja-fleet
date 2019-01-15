using jafleet.EF;

namespace jafleet.Models
{
    public class SearchResult
    {
        public string SearchConditionKey { get; set; }
        public AircraftView[] ResultList { get; set; }
    }
}
