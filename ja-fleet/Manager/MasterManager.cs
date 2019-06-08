using System;
using jafleet.Commons.EF;
using jafleet.Commons.Constants;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using jafleet.Models;
using Newtonsoft.Json;
using System.Diagnostics;
using Type = jafleet.Commons.EF.Type;

namespace jafleet.Manager
{
    public static class MasterManager
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

            _ana = context.Airline.AsNoTracking().Where(p => p.AirlineGroupCode == AirlineGroupCode.ANAGroup).OrderBy(p => p.DisplayOrder).ToArray();
            _jal = context.Airline.AsNoTracking().Where(p => p.AirlineGroupCode == AirlineGroupCode.JALGroup).OrderBy(p => p.DisplayOrder).ToArray();
            _lcc = context.Airline.AsNoTracking().Where(p => p.AirlineGroupCode == AirlineGroupCode.LCC).OrderBy(p => p.DisplayOrder).ToArray();
            _other = context.Airline.AsNoTracking().Where(p => p.AirlineGroupCode == AirlineGroupCode.Other).OrderBy(p => p.DisplayOrder).ToArray();
            _type = context.Type.AsNoTracking().OrderBy(p => p.DisplayOrder).ToArray();
            _wifi = context.Code.AsNoTracking().Where(p => p.CodeType == CodeType.WIFI).OrderBy(p => p.Key).ToArray();
            _adminUser = context.AdminUser.AsNoTracking().Select(e => e.UserId).ToList();
            _typeDetailGroup = context.TypeDetailView.AsNoTracking().OrderBy(p => p.DisplayOrder).ThenBy(p => p.TypeDetailName).ToArray();

            var tempop = context.Code.AsNoTracking().Where(p => p.CodeType == CodeType.OPERATION_CODE).OrderBy(p => p.Key).ToList();
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
                }else if (OperationCode.OTHERS.Contains(o.Key))
                {
                    o.OptGroup = "その他";
                }
            });
            _operation = tempop.ToArray();

            var airlineType = context.AircraftView.AsNoTracking().Select(av => new { av.Airline, av.TypeCode}).Distinct().OrderBy(av => av.Airline).ToList();
            string currentAirline = airlineType[0].Airline;
            var typelist = new List<Type>();
            foreach(var at in airlineType)
            {
                if(currentAirline != at.Airline)
                {
                    _airlineType.Add(currentAirline, typelist.OrderBy(t => t.DisplayOrder).ToList());
                    currentAirline = at.Airline;
                    typelist  = new List<Type>();
                }
                typelist.Add(_type.Where(t => t.TypeCode == at.TypeCode).SingleOrDefault());
            }
        }

        public static string GetSearchConditionDisp(string scKey, jafleetContext context)
        {
            {
                if (_searchCondition.ContainsKey(scKey))
                {
                    return _searchCondition[scKey];
                }
                else
                {
                    string scJson;
                    string searchConditionDisp = string.Empty;
                    scJson = context.SearchCondition.FirstOrDefault(sc => sc.SearchConditionKey == scKey)?.SearchConditionJson ?? string.Empty;
                    if (!string.IsNullOrEmpty(scJson) && scJson.Contains("TypeDetail"))
                    {
                        var scm = JsonConvert.DeserializeObject<SearchConditionInModel>(scJson, new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Populate });
                        var typeDetails = _typeDetailGroup.Where(td => scm.TypeDetail.Split("|").ToList().Contains(td.TypeDetailId.ToString()));
                        scm.TypeDetail = string.Join("|", typeDetails.Select(td => td.TypeDetailName));
                        searchConditionDisp = scm.ToString();
                    }
                    else
                    {
                        searchConditionDisp = scJson;
                    }
                    _searchCondition.Add(scKey, searchConditionDisp);
                    return searchConditionDisp;
                }
            }
        }

        public static int GetScCacheCount() => _searchCondition.Count;

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

        private static Dictionary<string, List<Type>> _airlineType = new Dictionary<string, List<Type>>();
        public static Dictionary<string, List<Type>> AirlineType { get { return _airlineType; } }

        private static Dictionary<string, string> _searchCondition = new Dictionary<string, string>();

    }
}
