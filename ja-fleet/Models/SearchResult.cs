﻿using jafleet.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace jafleet.Models
{
    public class SearchResult
    {
        public string SearchConditionKey { get; set; }
        public AircraftView[] ResultList { get; set; }
    }
}
