using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FillFull.Models
{
    public class Activities
    {
        public int ID { get; set; }

        public string ActivityText { get; set; }

        public DateTime ActivityDate { get; set; }

        public int WaiterID { get; set; }

        public virtual Waiter Waiter { get; set; }
    }
}