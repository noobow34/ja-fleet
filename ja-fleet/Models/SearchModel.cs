using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using jafleet.EF;

namespace jafleet.Models
{
    public class SearchModel
    {
        [Display(Name = "機体記号")]
        public String RegistrationNumber { get; set; }

        [Display(Name = "航空会社")]
        public Airline[] AirlineList { get; set; }
        public String Airline { get; set; } = String.Empty;

        [Display(Name = "型式")]
        public jafleet.EF.Type[] TypeList { get; set; }
        public String Type { get; set; } = String.Empty;

        [Display(Name = "運用状況")]
        public Code[] OperationList { get; set; }
        public String OperationCode { get; set; }

        [Display(Name = "WiFi")]
        public Code[] WiFiList { get; set; }
        public String WiFiCode { get; set; }

        [Display(Name = "登録年月")]
        public String RegistrationDate { get; set; }
        public String RegistrationSelection { get; set; }
        public List<SelectListItem> RegistrationSelectionList { get; set; }
            = new List<SelectListItem>{new SelectListItem{Value = "0",Text = "と等しい"}
                                       , new SelectListItem { Value = "1", Text = "以前" }
                                        , new SelectListItem { Value = "2", Text = "以降" } };

        [Display(Name = "備考")]
        public Boolean ExistRemarks { get; set; } = false;

        public Boolean IsLoading { get; set; }
    }
}
