using System.ComponentModel.DataAnnotations;

namespace jafleet.EF
{
    public partial class Aircraft
    {
        [Display(Name = "航空会社")]
        public string Airline { get; set; }
        [Display(Name = "機体記号")]
        public string RegistrationNumber { get; set; }
        [Display(Name = "型式")]
        public string TypeCode { get; set; }
        [Display(Name = "登録年月日")]
        public string RegisterDate { get; set; }
        [Display(Name = "運用状況")]
        public string OperationCode { get; set; }
        [Display(Name = "WiFi")]
        public string WifiCode { get; set; }
        [Display(Name = "備考")]
        public string Remarks { get; set; }
        [Display(Name = "製造番号")]
        public string SerialNumber { get; set; }
        [Display(Name = "作成日時")]
        public string UpdateTime { get; set; }
        [Display(Name = "更新日時")]
        public string CreationTime { get; set; }
    }
}
