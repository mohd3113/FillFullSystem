using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FillFull.Models
{

    public class Employeestatus {

        public int ID { get; set; }

        public string Name { get; set; }

        public string status { get; set; }
    }


    public class DashboardViewModel
    {
        public decimal TotalWageMo { get; set; }

        public double TotalExtraHoursMo { get; set; }

        public double TotalHoursMo { get; set; }
    }
}