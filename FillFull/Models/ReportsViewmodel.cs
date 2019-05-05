using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FillFull.Models
{


    public class IndivisualList
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public double TotalHours { get; set; }

        public decimal TotalWage { get; set; }

        public double TotalExtra { get; set; }

        public double TotalBreak { get; set; }

        public DateTime StartAt { get; set; }

        public DateTime EndAt { get; set; }

        public decimal TotalExtraWage { get; set; }

    }

    public class IndivisualReportViewModel
    {
        public  List<IndivisualList> IndivisualLists { get; set; }

        public decimal TotalWage { get; set; }

        public int ID { get; set; }

        public string Name { get; set; }

        public double TotalExtra { get; set; }

        public double TotalBreak { get; set; }

        public decimal TotalExtraWage { get; set; }

        public double TotalHours { get; set; }

        public IndivisualReportViewModel()
        {
            IndivisualLists = new List<IndivisualList>();
        }
    }


    public class ReportsViewmodel
    {

        public int WaiterID { get; set; }

        public string DateRange { get; set; }

        public string DateRange2 { get; set; }

        public int year { get; set; }

        public int Month { get; set; }

    }
}