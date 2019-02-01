using System;
using jafleet.Commons.EF;
using jafleet.Commons.Constants;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using Microsoft.EntityFrameworkCore.Extensions.Internal;

namespace jafleet.Manager
{
    public static class MasterManager
    {

        public static void ReadAll() {
            using (var context = new jafleetContext())
            {
                _allAirline = context.Airline.OrderBy(p => p.DisplayOrder).ToArray();
                _ana = context.Airline.Where(p => p.AirlineGroupCode == AirlineGroupCode.ANAGroup).OrderBy(p => p.DisplayOrder).ToArray();
                _jal = context.Airline.Where(p => p.AirlineGroupCode == AirlineGroupCode.JALGroup).OrderBy(p => p.DisplayOrder).ToArray();
                _lcc = context.Airline.Where(p => p.AirlineGroupCode == AirlineGroupCode.LCC).OrderBy(p => p.DisplayOrder).ToArray();
                _other = context.Airline.Where(p => p.AirlineGroupCode == AirlineGroupCode.Other).OrderBy(p => p.DisplayOrder).ToArray();
                _type = context.Type.OrderBy(p => p.DisplayOrder).ToArray();
                _operation = context.Code.Where(p => p.CodeType == CodeType.OPERATION_CODE).OrderBy(p => p.Key).ToArray();
                _wifi = context.Code.Where(p => p.CodeType == CodeType.WIFI).OrderBy(p => p.Key).ToArray();
                _adminUser = context.AdminUser.Select(e => e.UserId).ToList();
                _typeDetailGroup = context.TypeDetailView.OrderBy(p => p.DisplayOrder).OrderBy(p => p.TypeDetailName).ToArray();
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
                        _wifi = context.Code.Where(p => p.CodeType == CodeType.WIFI).OrderBy(p => p.Key).ToArray();
                    }
                }
                return _wifi;
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
                        _operation = context.Code.Where(p => p.CodeType == CodeType.OPERATION_CODE).OrderBy(p => p.Key).ToArray();
                    }
                }
                return _operation;
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
        }

        private static jafleet.Commons.EF.Type[] _type = null;
        public static jafleet.Commons.EF.Type[] Type
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
        }

        public static TypeDetailView[] _typeDetailGroup = null;
        public static TypeDetailView[] TypeDetailGroup
        {
            get
            {
                if(_typeDetailGroup == null)
                {
                    using (var context = new jafleetContext())
                    {
                        _typeDetailGroup = context.TypeDetailView.OrderBy(p => p.DisplayOrder).ToArray();
                    }
                }
                return _typeDetailGroup;
            }
        }

        private static List<string> _adminUser = null;
        public static List<string> AdminUser
        {
            get
            {
                if (_adminUser == null)
                {
                    using (var context = new jafleetContext())
                    {
                        _adminUser = context.AdminUser.Select(e => e.UserId).ToList();
                    }
                }
                return _adminUser;
            }
        }
    }
}
