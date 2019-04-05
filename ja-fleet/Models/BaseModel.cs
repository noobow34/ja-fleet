using System;

namespace jafleet.Models
{
    public class BaseModel
    {
        public Boolean NoHead { get; set; }
        public Boolean IsAdmin { get; set; }
        public Boolean IsDetail { get; set; }
        public string Title { get; set; }
        public string TableId { get; set; }
        public string api { get; set; }
    }
}
