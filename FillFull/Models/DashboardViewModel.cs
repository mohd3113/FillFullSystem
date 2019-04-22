using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FillFull.Models
{

    public class CorrentlyWaiters {

        public string Name { get; set; }

        public string StartAt { get; set; }

        public string status { get; set; }
    }


    public class DashboardViewModel
    {
        public decimal TotalWageMo { get; set; }

        public double TotalExtraHoursMo { get; set; }

        public double TotalHoursMo { get; set; }

        public List<CorrentlyWaiters> CorrentlyWaiters { get; set; }

        public List<Activities> Activities { get; set; }
    }
}