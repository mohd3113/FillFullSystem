﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FillFull.Models
{
    public class StartWorkViewModel
    {
        public int WaiterID { get; set; }

        public int WorkStartID { get; set; }

        public List<WaiterBreak> waiterBreaks { get; set; }

        public DateTime? Start { get; set; }

        public double Total_Hour { get; set; }

        public double TotalBreakDaily { get; set; }

        public double TotalMin { get; set; }

        public double TotalExtaMin { get; set; }

        public decimal ExtraTimeWage { get; set; }

        public double TotalMinDaily { get; set; }

        public bool IsExceeded { get; set; }

        public decimal Total_Wage { get; set; }

        public StartWorkViewModel()
        {
            waiterBreaks = new List<WaiterBreak>();
        }

    }
}