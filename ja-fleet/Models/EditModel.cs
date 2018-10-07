using System;
using System.ComponentModel.DataAnnotations;
using jafleet.EF;
using System.Dynamic;
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
        public jafleet.EF.Type[] TypeList { get; set; }

        [Display(Name = "運用状況")]
        public Code[] OperationList { get; set; }

        [Display(Name = "WiFi")]
        public Code[] WiFiList { get; set; }

        public Exception ex { get; set; };
    }
}
