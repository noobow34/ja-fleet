using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using jafleet.Commons.EF;
using jafleet.Manager;

namespace jafleet.Models
{
    public class SearchModel:BaseModel
    {
        [Display(Name = "機体記号")]
        public string RegistrationNumber { get; set; }

        [Display(Name = "航空会社")]
        public Airline[] AirlineList { get; set; }
        public string Airline { get; set; } = String.Empty;

        [Display(Name = "型式")]
        public TypeDetailView[] TypeDetailList { get; set; }
        public string TypeDetail { get; set; }

        [Display(Name = "運用状況")]
        public Code[] OperationList { get; set; }
        public string OperationCode { get; set; }

        [Display(Name = "WiFi")]
        public Code[] WiFiList { get; set; }
        public string WiFiCode { get; set; }

        [Display(Name = "登録年月")]
        public string RegistrationDate { get; set; }
        public string RegistrationSelection { get; set; }
        public List<SelectListItem> RegistrationSelectionList { get; set; } = MasterManager.PERIOD_SELECTION;

        [Display(Name = "備考")]
        public string Remarks { get; set; } = String.Empty;
        public string RemarksKeyword { get; set; } = String.Empty;
        public List<SelectListItem> RemarksList { get; set; } = MasterManager.EXIST_SELECTION;

        [Display(Name = "特別塗装")]
        public string SpecialLivery { get; set; } = String.Empty;
        public string SpecialLiveryKeyword { get; set; } = String.Empty;
        public List<SelectListItem> SpecialLiveryList { get; set; } = MasterManager.EXIST_SELECTION_HISTORY;

        public bool IsLoading { get; set; } = true;

        public bool IsDirect { get; set; } = false;
    }
}
