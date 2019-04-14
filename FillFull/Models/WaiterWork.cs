using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FillFull.Models
{
    public class WaiterWork
    {
        public int WaiterWorkID { get; set; }

        public int WaiterID { get; set; }

        public DateTime StartAt { get; set; }

        public DateTime? EndAt { get; set; }

        public double TotalMin { get; set; }

        public bool IsClosed { get; set; }

        public ICollection<WaiterBreak> WaiterBreaks { get; set; }

        public Waiter Waiter { get; set; }

    }
}