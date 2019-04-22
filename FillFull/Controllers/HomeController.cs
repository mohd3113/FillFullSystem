using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FillFull.Models;
using System.Data.Entity;

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

            dashboardViewModel.CorrentlyWaiters = new List<CorrentlyWaiters>();


            var inbreakewaiters = db.WaiterBreaks.Where(p => p.EndAt == null).ToList();

            var inworkwaiters = db.WaiterWorks.Where(p => p.IsClosed == false && p.WaiterBreaks.Where(c => c.EndAt == null).Count() == 0).ToList();

            var offlinewaiter = db.Waiters.Where(p => p.WaiterWorks.Where(c => c.IsClosed == false).Count() == 0).ToList();



            foreach (var inworwa in inworkwaiters)
            {
                dashboardViewModel.CorrentlyWaiters.Add(new CorrentlyWaiters { Name = inworwa.Waiter.FirstName + " " + inworwa.Waiter.LastName, StartAt = inworwa.StartAt.ToShortTimeString(), status = "Working" });
            }

            foreach (var inbrewa in inbreakewaiters)
            {
                dashboardViewModel.CorrentlyWaiters.Add(new CorrentlyWaiters { Name = inbrewa.WaiterWork.Waiter.FirstName + " " + inbrewa.WaiterWork.Waiter.LastName, StartAt = inbrewa.StartAt.ToShortTimeString(), status = "Break" });
            }

            foreach (var offewa in offlinewaiter)
            {
                dashboardViewModel.CorrentlyWaiters.Add(new CorrentlyWaiters { Name = offewa.FirstName + " " + offewa.LastName, StartAt = "", status = "Offline" });
            }
            dashboardViewModel.Activities = new List<Activities>();
            dashboardViewModel.Activities = db.Activities.Where(p => p.ActivityDate.Year == DateTime.Now.Year && p.ActivityDate.Month == DateTime.Now.Month && p.ActivityDate.Day == DateTime.Now.Day).ToList();

            return View(dashboardViewModel);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Reports()
        {

            ViewBag.Waiters = db.Waiters.Select(p => new { id = p.WaiterID, Name = p.FirstName + " " + p.LastName }).ToList();
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AllReport(ReportsViewmodel reportmodel)
        {

            List<IndivisualList> indivisualLists = new List<IndivisualList>();
            DateTime start = DateTime.Parse(reportmodel.DateRange.Split('-')[0]);
            DateTime end = DateTime.Parse(reportmodel.DateRange.Split('-')[1]);
            var waiters = db.Waiters.ToList();
            var waworkmonthly = db.WaiterWorks.Where(p => DbFunctions.TruncateTime(p.StartAt) >= DbFunctions.TruncateTime(start)
            && DbFunctions.TruncateTime(p.StartAt) <= DbFunctions.TruncateTime(end)).ToList();
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
                        IndivisualList indivisualList = new IndivisualList
                        {
                            ID = wa.WaiterID,
                            Name = wa.FirstName + " " + wa.LastName,
                            TotalWage = Total_Wage,
                            TotalExtra = TotalExtaMin,
                            TotalHours = TotalMin,
                            TotalExtraWage = ExtraTimeWage
                        };
                        indivisualLists.Add(indivisualList);
                    }
                    else
                    {
                        var TotalMin = totalminmonthly - watotalbreke;
                        var Total_Wage = Convert.ToDecimal(TotalMin / 60) * wa.Wage;
                        IndivisualList indivisualList = new IndivisualList
                        {
                            ID = wa.WaiterID,
                            Name = wa.FirstName + " " + wa.LastName,
                            TotalWage = Total_Wage,
                            TotalHours = TotalMin,
                        };
                        indivisualLists.Add(indivisualList);
                    }
                }
            }


            return PartialView("ReportList", indivisualLists);
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