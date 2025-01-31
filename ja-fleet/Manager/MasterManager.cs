﻿using jafleet.Commons.EF;
using jafleet.Commons.Constants;
using Microsoft.EntityFrameworkCore;
using Type = jafleet.Commons.EF.Type;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace jafleet.Manager
{
    public static class MasterManager
    {

        public static void ReadAll(jafleetContext context) {
            var tempaa = context.Airline.Where(a => !a.Deleted).OrderBy(p => p.DisplayOrder).ToList();
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

            _ana = context.Airline.AsNoTracking().Where(p => p.AirlineGroupCode == AirlineGroupCode.ANAGroup && !p.Deleted).OrderBy(p => p.DisplayOrder).ToArray();
            _jal = context.Airline.AsNoTracking().Where(p => p.AirlineGroupCode == AirlineGroupCode.JALGroup && !p.Deleted).OrderBy(p => p.DisplayOrder).ToArray();
            _lcc = context.Airline.AsNoTracking().Where(p => p.AirlineGroupCode == AirlineGroupCode.LCC && !p.Deleted).OrderBy(p => p.DisplayOrder).ToArray();
            _other = context.Airline.AsNoTracking().Where(p => p.AirlineGroupCode == AirlineGroupCode.Other && !p.Deleted).OrderBy(p => p.DisplayOrder).ToArray();
            _type = context.Type.AsNoTracking().OrderBy(p => p.DisplayOrder).ToArray();
            _wifi = context.Code.AsNoTracking().Where(p => p.CodeType == CodeType.WIFI).OrderBy(p => p.Key).ToArray();
            _adminUser = context.AdminUser.AsNoTracking().Select(e => e.UserId).ToList();
            _typeDetailGroup = context.TypeDetailView.AsNoTracking().OrderBy(p => p.DisplayOrder).ThenBy(p => p.TypeDetailName).ToArray();
            _seatConfiguration = context.SeatConfigration.AsNoTracking().OrderBy(p => p.Airline).ThenBy(p => p.Type).ToArray();
            AppInfo = context.AppInfo.SingleOrDefault();
            ReloadNamedSearchCondition(context);
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
            _airlineType = new Dictionary<string, List<Type>>();
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
            //最後の1つを処理
            _airlineType.Add(currentAirline, typelist.OrderBy(t => t.DisplayOrder).ToList());
        }

        public static void ReloadNamedSearchCondition(jafleetContext context)
        {
            _namedSearchCondition = context.SearchCondition.AsNoTracking().Where(sc => !string.IsNullOrEmpty(sc.SearchConditionName)).OrderBy(sc => sc.SearchConditionName).ToArray();
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

        private static Dictionary<string, List<Type>> _airlineType;
        public static Dictionary<string, List<Type>> AirlineType { get { return _airlineType; } }

        private static Dictionary<string, string> _searchCondition = new();

        public static SearchCondition[] _namedSearchCondition = null;
        public static SearchCondition[] NamedSearchCondition { get { return _namedSearchCondition; } }

        public static SeatConfiguration[] _seatConfiguration = null;
        public static SeatConfiguration[] SeatConfiguration { get { return _seatConfiguration; } }
        public static AppInfo AppInfo { get; set; }
        public static DateTime LaunchDate { get; private set; } = DateTime.Now;

        public static List<SelectListItem> EXIST_SELECTION = new()
        { new SelectListItem { Value = "1", Text = "なし" }
                                        , new SelectListItem { Value = "2", Text = "あり" }
                                        , new SelectListItem { Value = "3", Text = "キーワード指定" }};

        public static List<SelectListItem> EXIST_SELECTION_HISTORY = new()
        { new SelectListItem { Value = "1", Text = "なし" }
                                        , new SelectListItem { Value = "2", Text = "あり" }
                                        , new SelectListItem { Value = "3", Text = "キーワード指定" }
                                        , new SelectListItem { Value = "4", Text = "あり（履歴含む）" }
                                        , new SelectListItem { Value = "5", Text = "キーワード指定（履歴含む）" }};

        public static List<SelectListItem> PERIOD_SELECTION = new()
        {
            new SelectListItem{Value = "0",Text = "と等しい"}
                                       , new SelectListItem { Value = "1", Text = "以前" }
                                        , new SelectListItem { Value = "2", Text = "以降" }};

    }
}
