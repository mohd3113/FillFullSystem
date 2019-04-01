using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FillFull.Models
{
    public class WaiterBreak
    {
        public int WaiterBreakID { get; set; }

        public int WaiterWorkID { get; set; }

        public DateTime StartAt { get; set; }

        public DateTime? EndAt { get; set; }

        public virtual WaiterWork WaiterWork { get; set; }
    }
}