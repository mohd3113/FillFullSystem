using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FillFull.Models
{
    public class Waiter
    {
        public int WaiterID { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public string Address { get; set; }

        public string StartTime { get; set; }

        public string EndTime { get; set; }

        public string ImagePath { get; set; }

        public decimal Wage { get; set; }

        public ICollection<WaiterWork> WaiterWorks { get; set; }

    }
}