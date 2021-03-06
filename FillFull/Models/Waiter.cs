﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FillFull.Models
{
    public class Waiter
    {
        public int WaiterID { get; set; }

        [Display(Name = "First Name")]
        [Required]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        [Required]
        public string LastName { get; set; }

        [DataType(DataType.EmailAddress)]
        [Required]
        public string Email { get; set; }

        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        public string Address { get; set; }

        public string ImagePath { get; set; }

        [Required]
        public decimal Wage { get; set; }

        public double MaxWorkingHours { get; set; }

        public decimal WageafterMaxHours { get; set; }

        public ICollection<WaiterWork> WaiterWorks { get; set; }

        public ICollection<Activities> Activities { get; set; }

    }
}