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
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;

namespace FillFull.Controllers
{
    [Authorize(Roles = "Manager")]
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
        public ActionResult Index()
        {
            return View(db.Waiters.ToList());
        }

        // GET: Waiters/Details/5
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
        public ActionResult Create()
        {
            return View();
        }


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
            var ifstillworking = db.WaiterWorks.FirstOrDefault(p => p.WaiterID == id && p.IsClosed == false);
            StartWorkViewModel startmodel;
            if (ifstillworking == null)
            {
                startmodel = new StartWorkViewModel
                {
                    WaiterID = id.Value,
                };
            }
            else
            {
                startmodel = new StartWorkViewModel
                {
                    WaiterID = id.Value,
                    WorkStartID = ifstillworking.WaiterWorkID,
                    Start = ifstillworking.StartAt
                };
                double totalbreakmin = 0;
                if (ifstillworking.WaiterBreaks != null)
                {
                    startmodel.waiterBreaks = ifstillworking.WaiterBreaks.ToList();
                    foreach(var br in startmodel.waiterBreaks)
                    {
                        if(br.EndAt != null)
                        {
                            totalbreakmin += (br.EndAt.Value - br.StartAt).TotalMinutes;
                        }
                        else
                        {
                            totalbreakmin += (DateTime.Now - br.StartAt).TotalMinutes;
                        }
                    }
                }
           
                var totalmin = (DateTime.Now - startmodel.Start.Value).TotalMinutes;
                if (wa.MaxWorkingHours != 0)
                {
                    if (totalmin > wa.MaxWorkingHours * 60)
                    {
                        startmodel.TotalMin = wa.MaxWorkingHours * 60;
                        startmodel.TotalExtaMin = totalmin - startmodel.TotalMin;
                        startmodel.Total_Wage = Convert.ToDecimal(startmodel.TotalMin / 60) * wa.Wage;
                        startmodel.ExtraTimeWage = Convert.ToDecimal(startmodel.TotalExtaMin / 60) * wa.WageafterMaxHours;
                        return View(startmodel);
                    }

                }
                startmodel.WorkingBreakMin = totalbreakmin;
                startmodel.TotalMin = totalmin;
                startmodel.Total_Wage = Convert.ToDecimal(startmodel.TotalMin / 60) * wa.Wage;
            }
            return View(startmodel);
        }

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
                db.SaveChanges();
                startWorkViewModel.Start = waiterWork.StartAt;
                //startWorkViewModel.waiterBreaks = waiterWork.WaiterBreaks.ToList();
                startWorkViewModel.WorkStartID = waiterWork.WaiterWorkID;
                var employeeentrytime = startWorkViewModel.Start.Value;
                startWorkViewModel.Total_Hour = 0;
                startWorkViewModel.Total_Wage = 0;
                return View(startWorkViewModel);
            }
            return View(startWorkViewModel);
        }

        [HttpPost]
        public ActionResult EndShift(int? shiftid)
        {
            if (shiftid == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var runingshift = db.WaiterWorks.SingleOrDefault(p => p.WaiterWorkID == shiftid);
            if (runingshift == null)
            {
                return HttpNotFound();
            }
            var wa = db.Waiters.SingleOrDefault(p => p.WaiterID == runingshift.WaiterID);
            var totalmin = (DateTime.Now - runingshift.StartAt).TotalMinutes;
            if (wa.MaxWorkingHours != 0)
            {
                if (totalmin > wa.MaxWorkingHours * 60)
                {
                    runingshift.TotalMin = wa.MaxWorkingHours * 60;
                    runingshift.TotalExtraMin = totalmin - runingshift.TotalMin;
                    runingshift.Wage = Convert.ToDecimal(runingshift.TotalMin / 60) * wa.Wage;
                    runingshift.ExtraTimeWage = Convert.ToDecimal(runingshift.TotalExtraMin / 60) * wa.WageafterMaxHours;
                }
            }
            runingshift.IsClosed = true;
            runingshift.TotalMin = totalmin;
            runingshift.Wage = Convert.ToDecimal(runingshift.TotalMin / 60) * wa.Wage;
            runingshift.EndAt = DateTime.Now;
            db.SaveChanges();
            return Json(new { id1 = runingshift.WaiterID }, JsonRequestBehavior.AllowGet);
        }


        // POST: Waiters/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "WaiterID,FirstName,LastName,Email,PhoneNumber,Address,StartTime,EndTime,Wage")] Waiter waiter, HttpPostedFileBase uploadFile)
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
        public ActionResult Edit([Bind(Include = "WaiterID,FirstName,LastName,Email,PhoneNumber,Address,StartTime,EndTime,Wage")] Waiter waiter, HttpPostedFileBase uploadFile)
        {

            if (ModelState.IsValid)
            {

                db.Entry(waiter).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(waiter);
        }

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
            db.SaveChanges();
            return Json(new { id1 = waiterwork.WaiterID }, JsonRequestBehavior.AllowGet);


        }
        public ActionResult Continue(int? shiftid)
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
            waiterwork.EndAt = DateTime.Now;
            db.SaveChanges();
            return Json(new { id1 = waiterwork.WaiterID }, JsonRequestBehavior.AllowGet);
        }

        // GET: Waiters/Delete/5
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
