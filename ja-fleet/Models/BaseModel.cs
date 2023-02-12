namespace jafleet.Models
{
    public class BaseModel
    {
        public bool NoHead { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsDetail { get; set; }
        public string Title { get; set; }
        public string TableId { get; set; }
        public string api { get; set; }
    }
}
