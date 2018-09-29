using System;
using jafleet.EF;
using jafleet.Constants;
using System.Linq;

namespace jafleet.Manager
{
    public static class MasterManager
    {
     
        public static void ReadAll(){
            using (var context = new jafleetContext())
            {
                _allAirline = context.Airline.OrderBy(p => p.DisplayOrder).ToArray();
                _ana = context.Airline.Where(p => p.AirlineGroupCode == AirlineGroupCode.ANAGroup).OrderBy(p => p.DisplayOrder).ToArray();
                _jal = context.Airline.Where(p => p.AirlineGroupCode == AirlineGroupCode.JALGroup).OrderBy(p => p.DisplayOrder).ToArray();
                _lcc = context.Airline.Where(p => p.AirlineGroupCode == AirlineGroupCode.LCC).OrderBy(p => p.DisplayOrder).ToArray();
                _other = context.Airline.Where(p => p.AirlineGroupCode == AirlineGroupCode.Other).OrderBy(p => p.DisplayOrder).ToArray();
                _type = context.Type.OrderBy(p => p.DisplayOrder).ToArray();
                _operation = context.Code.Where(p => p.CodeType == "OPE").OrderBy(p => p.Key).ToArray();
                _wifi = context.Code.Where(p => p.CodeType == "WIFI").OrderBy(p => p.Key).ToArray();
            }
        }

        private static Code[] _wifi = null;
        public static Code[] Wifi
        {
            get
            {
                if (_wifi == null)
                {
                    using (var context = new jafleetContext())
                    {
                        _wifi = context.Code.Where(p => p.CodeType == "WIFI").OrderBy(p => p.Key).ToArray();
                    }
                }
                return _wifi;
            }
            set
            {
                _wifi = value;
            }
        }

        private static Code[] _operation = null;
        public static Code[] Operation
        {
            get
            {
                if (_operation == null)
                {
                    using (var context = new jafleetContext())
                    {
                        _operation = context.Code.Where(p => p.CodeType == "OPE").OrderBy(p => p.Key).ToArray();
                    }
                }
                return _operation;
            }
            set
            {
                _operation = value;
            }
        }

        private static Airline[] _allAirline = null;
        public static Airline[] AllAirline
        {
            get
            {
                if (_allAirline == null)
                {
                    using (var context = new jafleetContext())
                    {
                        _allAirline = context.Airline.OrderBy(p => p.DisplayOrder).ToArray();
                    }
                }
                return _allAirline;
            }
            set
            {
                _allAirline = value;
            }
        }


        private static Airline[] _ana = null;
        public static Airline[] ANA
        {
            get
            {
                if (_ana == null)
                {
                    using (var context = new jafleetContext())
                    {
                        _ana = context.Airline.Where(p => p.AirlineGroupCode == AirlineGroupCode.ANAGroup).OrderBy(p => p.DisplayOrder).ToArray();
                    }
                }
                return _ana;
            }
            set
            {
                _ana = value;
            }
        }

        private static Airline[] _jal = null;
        public static Airline[] JAL
        {
            get
            {
                if (_jal == null)
                {
                    using (var context = new jafleetContext())
                    {
                        _jal = context.Airline.Where(p => p.AirlineGroupCode == AirlineGroupCode.JALGroup).OrderBy(p => p.DisplayOrder).ToArray();
                    }
                }
                return _jal;
            }
            set
            {
                _jal = value;
            }
        }

        private static Airline[] _lcc = null;
        public static Airline[] LCC
        {
            get
            {
                if (_lcc == null)
                {
                    using (var context = new jafleetContext())
                    {
                        _lcc = context.Airline.Where(p => p.AirlineGroupCode == AirlineGroupCode.LCC).OrderBy(p => p.DisplayOrder).ToArray();
                    }
                }
                return _lcc;
            }
            set
            {
                _lcc = value;
            }
        }

        private static Airline[] _other = null;
        public static Airline[] Other
        {
            get
            {
                if (_other == null)
                {
                    using (var context = new jafleetContext())
                    {
                        _other = context.Airline.Where(p => p.AirlineGroupCode == AirlineGroupCode.Other).OrderBy(p => p.DisplayOrder).ToArray();
                    }
                }
                return _other;
            }
            set
            {
                _other = value;
            }
        }

        private static jafleet.EF.Type[] _type = null;
        public static jafleet.EF.Type[] Type
        {
            get
            {
                if (_type == null)
                {
                    using (var context = new jafleetContext())
                    {
                        _type = context.Type.OrderBy(p => p.DisplayOrder).ToArray();
                    }
                }
                return _type;
            }
            set
            {
                _type = value;
            }
        }
    }
}
