﻿using jafleet.EF;
using System.Collections.Generic;

namespace jafleet.Models
{
    public class HomeModel
    {
        public List<Airline> ana { get; set; }
        public List<Airline> jal { get; set; }
        public List<Airline> lcc { get; set; }
        public List<Airline> other { get; set; }
    }
}
