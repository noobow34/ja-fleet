using System;
using System.ComponentModel.DataAnnotations;
using jafleet.Commons.EF;
using System.Dynamic;
using jafleet.Commons.Constants;

namespace jafleet.Models
{
    public class EditModel
    {
        public Aircraft Aircraft { get; set; }
        public Boolean IsNew { get; set; } = false;

        //選択用リスト
        [Display(Name = "航空会社")]
        public Airline[] AirlineList { get; set; }

        [Display(Name = "型式")]
        public jafleet.Commons.EF.Type[] TypeList { get; set; }

        [Display(Name = "詳細型式")]
        public jafleet.Commons.EF.TypeDetail[] TypeDetailList { get; set; }

        [Display(Name = "運用状況")]
        public Code[] OperationList { get; set; }

        [Display(Name = "WiFi")]
        public Code[] WiFiList { get; set; }

        [Display(Name = "更新日付を更新しない")]
        public Boolean NotUpdateDate { get; set; } = false;

        public Exception ex { get; set; }

        public string LinkPage {
            get {
                if (Aircraft != null)
                {
                    string additional = string.Empty;
                    if(Aircraft.OperationCode == OperationCode.RETIRE_UNREGISTERED)
                    {
                        additional = "?includeRetire=true";
                    }
                    return $"https://ja-fleet.noobow.me/Aircraft/Airline/{Aircraft.Airline}/{Aircraft.TypeCode}{additional}";
                }
                else
                {
                    return string.Empty;
                }
            }
        }
    }
}
