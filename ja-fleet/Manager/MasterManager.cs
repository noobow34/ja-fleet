using System;
using jafleet.Commons.EF;
using jafleet.Commons.Constants;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using Microsoft.EntityFrameworkCore.Extensions.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace jafleet.Manager
{
    public class MasterManager
    {
        public static void ReadAll(jafleetContext context) {
            var tempaa = context.Airline.OrderBy(p => p.DisplayOrder).ToList();
            tempaa.ForEach(aa =>
            {
                switch (aa.AirlineGroupCode)
                {
                    case AirlineGroupCode.ANAGroup:
                        aa.AirlineGroup = "ANAグループ";
                        break;
                    case AirlineGroupCode.JALGroup:
                        aa.AirlineGroup = "JALグループ";
                        break;
                    case AirlineGroupCode.LCC:
                        aa.AirlineGroup = "LCC";
                        break;
                    case AirlineGroupCode.Other:
                        aa.AirlineGroup = "その他";
                        break;

                }
            });
            _allAirline = tempaa.ToArray();

            _ana = context.Airline.Where(p => p.AirlineGroupCode == AirlineGroupCode.ANAGroup).OrderBy(p => p.DisplayOrder).ToArray();
            _jal = context.Airline.Where(p => p.AirlineGroupCode == AirlineGroupCode.JALGroup).OrderBy(p => p.DisplayOrder).ToArray();
            _lcc = context.Airline.Where(p => p.AirlineGroupCode == AirlineGroupCode.LCC).OrderBy(p => p.DisplayOrder).ToArray();
            _other = context.Airline.Where(p => p.AirlineGroupCode == AirlineGroupCode.Other).OrderBy(p => p.DisplayOrder).ToArray();
            _type = context.Type.OrderBy(p => p.DisplayOrder).ToArray();
            _wifi = context.Code.Where(p => p.CodeType == CodeType.WIFI).OrderBy(p => p.Key).ToArray();
            _adminUser = context.AdminUser.Select(e => e.UserId).ToList();
            _typeDetailGroup = context.TypeDetailView.OrderBy(p => p.DisplayOrder).ThenBy(p => p.TypeDetailName).ToArray();

            var tempop = context.Code.Where(p => p.CodeType == CodeType.OPERATION_CODE).OrderBy(p => p.Key).ToList();
            tempop.ForEach(o => {
                if(OperationCode.PRE_OPERATION.Contains(o.Key))
                {
                    o.OptGroup = "運用前";
                }else if(OperationCode.IN_OPERATION.Contains(o.Key))
                {
                    o.OptGroup = "運用中";
                }else if(OperationCode.RETIRE.Contains(o.Key))
                {
                    o.OptGroup = "退役";
                }
            });
            _operation = tempop.ToArray();
        }

        private static Code[] _wifi = null;
        public static Code[] Wifi { get { return _wifi; } }

        private static Code[] _operation = null;
        public static Code[] Operation { get { return _operation; } }

        private static Airline[] _allAirline = null;
        public static Airline[] AllAirline { get { return _allAirline; } }


        private static Airline[] _ana = null;
        public static Airline[] ANA { get { return _ana; } }

        private static Airline[] _jal = null;
        public static Airline[] JAL { get { return _jal; } }

        private static Airline[] _lcc = null;
        public static Airline[] LCC { get { return _lcc; } }

        private static Airline[] _other = null;
        public static Airline[] Other { get { return _other; } }

        private static jafleet.Commons.EF.Type[] _type = null;
        public static jafleet.Commons.EF.Type[] Type { get { return _type; } }

        public static TypeDetailView[] _typeDetailGroup = null;
        public static TypeDetailView[] TypeDetailGroup { get { return _typeDetailGroup; } }

        private static List<string> _adminUser = null;
        public static List<string> AdminUser { get { return _adminUser; } }

    }
}
