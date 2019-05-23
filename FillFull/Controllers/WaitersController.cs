using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using FillFull.Models;
using FillFull.Services;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;


namespace FillFull.Controllers
{

    [AuthorizeIPAddress]
    public class WaitersController : Controller
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
        // GET: Waiters
        [Authorize(Roles = "Manager")]
        public ActionResult Index()
        {
            return View(db.Waiters.ToList());
        }

        // GET: Waiters/Details/5
        [Authorize(Roles = "Manager")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Waiter waiter = db.Waiters.Find(id);
            if (waiter == null)
            {
                return HttpNotFound();
            }
            return View(waiter);
        }

        // GET: Waiters/Create
        [Authorize(Roles = "Manager")]
        public ActionResult Create()
        {
            return View();
        }


        [Authorize(Roles = "waiter")]
        public ActionResult StartWork(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var wa = db.Waiters.SingleOrDefault(p => p.WaiterID == id);
            if (wa == null)
            {
                return HttpNotFound();
            }
            var ifstillworking = db.WaiterWorks.Include(b => b.WaiterBreaks).SingleOrDefault(p => p.WaiterID == id && p.IsClosed == false);
            StartWorkViewModel startmodel;
            if (ifstillworking == null)
            {
                startmodel = new StartWorkViewModel
                {
                    WaiterID = id.Value,

                };
                var waworkmonthly = db.WaiterWorks.Where(p => p.WaiterID == wa.WaiterID && p.StartAt.Year == DateTime.Now.Year && p.StartAt.Month == DateTime.Now.Month).ToList();
                var totalminmonthly = waworkmonthly.Sum(p => p.TotalMin);
                double totalbrake = 0;
                foreach (var wawo in waworkmonthly)
                {
                    if (wawo.WaiterBreaks != null)
                        totalbrake += wawo.WaiterBreaks.Where(c => c.EndAt != null && c.StartAt.Month != DateTime.Now.Month && c.StartAt.Year != DateTime.Now.Year).Sum(p => (p.EndAt.Value - p.StartAt).TotalMinutes);
                }
                if (wa.MaxWorkingHours != 0)
                {
                    if ((totalminmonthly - totalbrake) > (wa.MaxWorkingHours * 60))
                    {
                        startmodel.TotalMin = wa.MaxWorkingHours * 60;
                        startmodel.TotalExtaMin = totalminmonthly - totalbrake - (wa.MaxWorkingHours * 60);
                        startmodel.Total_Wage = Convert.ToDecimal(startmodel.TotalMin / 60) * wa.Wage;
                        startmodel.ExtraTimeWage = Convert.ToDecimal(startmodel.TotalExtaMin / 60) * wa.WageafterMaxHours;
                        startmodel.IsExceeded = true;
                        return View(startmodel);
                    }
                    else
                    {
                        startmodel.TotalMin = totalminmonthly - totalbrake;
                        startmodel.Total_Wage = Convert.ToDecimal(startmodel.TotalMin / 60) * wa.Wage;
                        startmodel.IsExceeded = false;
                    }
                }
            }
            else
            {
                startmodel = new StartWorkViewModel
                {
                    WaiterID = id.Value,
                    WorkStartID = ifstillworking.WaiterWorkID,
                    Start = ifstillworking.StartAt
                };
                double totalbreakda = 0;
                if (ifstillworking.WaiterBreaks != null)
                {
                    startmodel.waiterBreaks = ifstillworking.WaiterBreaks.ToList();
                    foreach (var waw in startmodel.waiterBreaks)
                    {
                        if (waw.EndAt != null)
                            totalbreakda += (waw.EndAt.Value - waw.StartAt).TotalMinutes;
                        else
                        {
                            totalbreakda += (DateTime.Now - waw.StartAt).TotalMinutes;
                        }
                    }
                }

                var totalmin = (DateTime.Now - startmodel.Start.Value).TotalMinutes;
                var waworkmonthly = db.WaiterWorks.Where(p => p.WaiterID == wa.WaiterID && p.StartAt.Year == DateTime.Now.Year && p.StartAt.Month == DateTime.Now.Month).ToList();
                var totalminmonthly = waworkmonthly.Sum(p => p.TotalMin);
                double totalbrake = 0;
                foreach (var wawo in waworkmonthly)
                {
                    if (wawo.WaiterBreaks != null)
                        totalbrake += wawo.WaiterBreaks.Where(c => c.EndAt != null && c.StartAt.Month != DateTime.Now.Month && c.StartAt.Year != DateTime.Now.Year).Sum(p => (p.EndAt.Value - p.StartAt).TotalMinutes);
                }
                if (wa.MaxWorkingHours != 0)
                {
                    if ((totalmin + totalminmonthly - totalbrake - totalbreakda) > (wa.MaxWorkingHours * 60))
                    {
                        startmodel.TotalMin = wa.MaxWorkingHours * 60;
                        startmodel.TotalExtaMin = totalminmonthly + totalmin - totalbrake - totalbreakda - (wa.MaxWorkingHours * 60);
                        startmodel.Total_Wage = Convert.ToDecimal(startmodel.TotalMin / 60) * wa.Wage;
                        startmodel.ExtraTimeWage = Convert.ToDecimal(startmodel.TotalExtaMin / 60) * wa.WageafterMaxHours;
                        startmodel.TotalMinDaily = totalmin - totalbreakda;
                        startmodel.TotalBreakDaily = totalbreakda;
                        startmodel.IsExceeded = true;
                        return View(startmodel);
                    }
                    else
                    {
                        startmodel.TotalMin = totalminmonthly - totalbrake;
                        startmodel.Total_Wage = Convert.ToDecimal(startmodel.TotalMin / 60) * wa.Wage;
                        startmodel.TotalMinDaily = totalmin - totalbreakda;
                        startmodel.TotalBreakDaily = totalbreakda;
                        startmodel.IsExceeded = false;
                    }

                }
            }
            return View(startmodel);
        }

        [Authorize(Roles = "waiter")]
        [HttpPost]
        public ActionResult StartWork(StartWorkViewModel startWorkViewModel)
        {
            if (startWorkViewModel.WaiterID != 0)
            {
                WaiterWork waiterWork = new WaiterWork
                {
                    WaiterID = startWorkViewModel.WaiterID,
                    StartAt = DateTime.Now,
                    IsClosed = false,

                };
                db.WaiterWorks.Add(waiterWork);
                var waiter = db.Waiters.SingleOrDefault(p => p.WaiterID == startWorkViewModel.WaiterID);
                Activities activities = new Activities
                {
                    ActivityText = waiter.FirstName + " " + waiter.LastName + " has started new shift",
                    ActivityDate = DateTime.Now,
                    WaiterID = waiter.WaiterID
                };
                db.Activities.Add(activities);
                db.SaveChanges();
                return RedirectToAction("StartWork", new { id = waiterWork.WaiterID });
            }
            return View(startWorkViewModel);
        }

        [Authorize(Roles = "waiter")]
        [HttpPost]
        public ActionResult EndShift(int? shiftid)
        {
            if (shiftid == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var runingshift = db.WaiterWorks.Include(c => c.WaiterBreaks).SingleOrDefault(p => p.WaiterWorkID == shiftid);
            if (runingshift == null)
            {
                return HttpNotFound();
            }
            double totalbreakda = 0;
            if (runingshift.WaiterBreaks != null)
            {
                var waiterBreaks = runingshift.WaiterBreaks.ToList();
                foreach (var waw in waiterBreaks)
                {
                    if (waw.EndAt != null)
                        totalbreakda += (waw.EndAt.Value - waw.StartAt).TotalMinutes;
                    else
                    {
                        totalbreakda += (DateTime.Now - waw.StartAt).TotalMinutes;
                    }
                }
            }
            var totalmin = (DateTime.Now - runingshift.StartAt).TotalMinutes;
            runingshift.IsClosed = true;
            runingshift.TotalMin = totalmin - totalbreakda;
            runingshift.EndAt = DateTime.Now;
            var waiter = db.Waiters.SingleOrDefault(p => p.WaiterID == runingshift.WaiterID);
            Activities activities = new Activities
            {
                ActivityText = waiter.FirstName + " " + waiter.LastName + " has ended his shift",
                ActivityDate = DateTime.Now,
                WaiterID = waiter.WaiterID
            };
            db.Activities.Add(activities);
            db.SaveChanges();
            return Json(new { id1 = runingshift.WaiterID }, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "waiter")]
        [HttpPost]
        public ActionResult BreakWork(int? shiftid)
        {
            if (shiftid == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var waiterwork = db.WaiterWorks.SingleOrDefault(p => p.WaiterWorkID == shiftid);
            if (waiterwork == null)
            {
                return HttpNotFound();
            }
            WaiterBreak breake1 = new WaiterBreak
            {
                WaiterWorkID = waiterwork.WaiterWorkID,
                StartAt = DateTime.Now,
            };
            db.WaiterBreaks.Add(breake1);
            var waiter = db.Waiters.SingleOrDefault(p => p.WaiterID == waiterwork.WaiterID);
            Activities activities = new Activities
            {
                ActivityText = waiter.FirstName + " " + waiter.LastName + " has started a break",
                ActivityDate = DateTime.Now,
                WaiterID = waiter.WaiterID
            };
            db.Activities.Add(activities);
            db.SaveChanges();
            return Json(new { id1 = waiterwork.WaiterID }, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "waiter")]
        [HttpPost]
        public ActionResult Continue(int? shiftid)
        {
            if (shiftid == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var waiterwork = db.WaiterBreaks.SingleOrDefault(p => p.WaiterWorkID == shiftid && p.EndAt == null);
            if (waiterwork == null)
            {
                return HttpNotFound();
            }
            waiterwork.EndAt = DateTime.Now;
            var waiter = db.Waiters.SingleOrDefault(p => p.WaiterID == waiterwork.WaiterWork.WaiterID);
            Activities activities = new Activities
            {
                ActivityText = waiter.FirstName + " " + waiter.LastName + " has ended his break",
                ActivityDate = DateTime.Now,
                WaiterID = waiter.WaiterID
            };
            db.Activities.Add(activities);
            db.SaveChanges();
            return Json(new { id1 = waiterwork.WaiterWork.WaiterID }, JsonRequestBehavior.AllowGet);
        }

        // POST: Waiters/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager")]
        public ActionResult Create(Waiter waiter, HttpPostedFileBase uploadFile)
        {
            if (ModelState.IsValid)
            {
                string password = waiter.LastName.Substring(0, 2).Trim() + "_Wa_12345";
                if (ModelState.IsValid)
                {
                    var user = new ApplicationUser { UserName = waiter.Email.Trim(), Email = waiter.Email.Trim() };
                    var result = UserManager.Create(user, password);
                    if (result.Succeeded)
                    {
                        var result1 = CreateRolesandUsers("waiter", user);
                        if (result1.Succeeded)
                        {
                            db.Waiters.Add(waiter);
                            db.SaveChanges();
                            try
                            {
                                if (uploadFile != null && uploadFile.ContentLength > 0)
                                {
                                    var fileName = waiter.WaiterID.ToString() + "_Image" + Path.GetExtension(uploadFile.FileName);
                                    var path = Path.Combine(Server.MapPath("~/PeopleImages/"), fileName);
                                    uploadFile.SaveAs(path);
                                    waiter.ImagePath = "PeopleImages/" + fileName;
                                }
                                else
                                {
                                    waiter.ImagePath = "PeopleImages/default.png";
                                }
                            }
                            catch
                            {
                                waiter.ImagePath = "PeopleImages/default.png";
                            }
                            db.SaveChanges();
                            return RedirectToAction("Index");
                        }
                    }
                }
            }

            return View(waiter);
        }

        [Authorize(Roles = "Manager")]
        private IdentityResult CreateRolesandUsers(string Role, ApplicationUser user)
        {
            ApplicationDbContext context = new ApplicationDbContext();

            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            // In Startup iam creating first Admin Role and creating a default Admin User    
            if (!roleManager.RoleExists(Role))
            {
                // first we create Admin rool   
                var role = new IdentityRole();
                role.Name = Role;
                roleManager.Create(role);
                //Here we create a Admin super user who will maintain the website                  
                var result1 = UserManager.AddToRole(user.Id, Role);
                return result1;
            }
            else
            {
                var result1 = UserManager.AddToRole(user.Id, Role);
                return result1;
            }
        }
        // GET: Waiters/Edit/5
        [Authorize(Roles = "Manager")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Waiter waiter = db.Waiters.Find(id);
            if (waiter == null)
            {
                return HttpNotFound();
            }
            return View(waiter);
        }

        // POST: Waiters/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager")]
        public ActionResult Edit(Waiter waiter, HttpPostedFileBase uploadFile)
        {

            if (ModelState.IsValid)
            {
                if (uploadFile != null && uploadFile.ContentLength > 0)
                {
                    if (!string.IsNullOrEmpty(waiter.ImagePath) && waiter.ImagePath != "PeopleImages/default.png")
                    {
                        if (System.IO.File.Exists(Server.MapPath(waiter.ImagePath)))
                        {
                            System.IO.File.Delete(Server.MapPath(waiter.ImagePath));
                        }
                    }
                    var fileName = waiter.WaiterID.ToString() + "_Image" + Path.GetExtension(uploadFile.FileName);
                    var path = Path.Combine(Server.MapPath("~/PeopleImages/"), fileName);
                    uploadFile.SaveAs(path);
                    waiter.ImagePath = "PeopleImages/" + fileName;
                }
                db.Entry(waiter).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(waiter);
        }

        // GET: Waiters/Delete/5
        [Authorize(Roles = "Manager")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Waiter waiter = db.Waiters.Find(id);
            if (waiter == null)
            {
                return HttpNotFound();
            }
            return View(waiter);
        }

        // POST: Waiters/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager")]
        public ActionResult DeleteConfirmed(int id)
        {
            Waiter waiter = db.Waiters.Find(id);
            db.Waiters.Remove(waiter);
            db.SaveChanges();
            return RedirectToAction("Index");
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
