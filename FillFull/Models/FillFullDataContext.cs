using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace FillFull.Models
{
    public class FillFullDataContext : DbContext
    {
        public FillFullDataContext() : base("DefaultConnection")
        {
            
        }
        public DbSet<Waiter> Waiters { get; set; }

        public DbSet<Admin> Admins { get; set; }

        public DbSet<WaiterWork> WaiterWorks { get; set; }

        public DbSet<WaiterBreak> WaiterBreaks { get; set; }

        public DbSet<Activities> Activities { get; set; }
    }
}