using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FillFull.Models;

namespace FillFull.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private FillFullDataContext db = new FillFullDataContext();
        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        public ActionResult Index()
        {

            if (User.IsInRole("waiter"))
            {
                var user = User.Identity.Name;
                var user2 = UserManager.FindByName(user);
                var waiter1 = db.Waiters.SingleOrDefault(p => p.Email == user2.Email);
                return RedirectToAction("StartWork", "Waiters", new { id = waiter1.WaiterID });
            }
            DashboardViewModel dashboardViewModel = new DashboardViewModel();
            dashboardViewModel.TotalHoursMo = 0;
            dashboardViewModel.TotalWageMo = 0;
            dashboardViewModel.TotalExtraHoursMo = 0;
            var waiters = db.Waiters.ToList();
            var waworkmonthly = db.WaiterWorks.Where(p => p.StartAt.Year == DateTime.Now.Year && p.StartAt.Month == DateTime.Now.Month).ToList();
            foreach (var wa in waiters)
            {
                double watotalbreke = 0;
                var inwaworkmonthly = waworkmonthly.Where(p => p.WaiterID == wa.WaiterID).ToList();

                foreach (var wawo in inwaworkmonthly)
                {
                    if (wawo.WaiterBreaks != null)
                        watotalbreke += wawo.WaiterBreaks.Where(c => c.EndAt != null && c.StartAt.Month != DateTime.Now.Month && c.StartAt.Year != DateTime.Now.Year).Sum(p => (p.EndAt.Value - p.StartAt).TotalMinutes);
                }
                var totalminmonthly = inwaworkmonthly.Sum(p => p.TotalMin);
                if (wa.MaxWorkingHours != 0)
                {
                    if ((totalminmonthly - watotalbreke) > (wa.MaxWorkingHours * 60))
                    {
                        var TotalMin = wa.MaxWorkingHours * 60;
                        var TotalExtaMin = totalminmonthly - watotalbreke - (wa.MaxWorkingHours * 60);
                        var Total_Wage = Convert.ToDecimal(TotalMin / 60) * wa.Wage;
                        var ExtraTimeWage = Convert.ToDecimal(TotalExtaMin / 60) * wa.WageafterMaxHours;
                        dashboardViewModel.TotalExtraHoursMo += TotalExtaMin;
                        dashboardViewModel.TotalHoursMo += TotalMin;
                        dashboardViewModel.TotalWageMo += Total_Wage + ExtraTimeWage;
                    }
                    else
                    {
                        var TotalMin = totalminmonthly - watotalbreke;
                        var Total_Wage = Convert.ToDecimal(TotalMin / 60) * wa.Wage;
                        dashboardViewModel.TotalHoursMo += TotalMin;
                        dashboardViewModel.TotalWageMo += Total_Wage;
                    }
                }
            }
            return View(dashboardViewModel);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}