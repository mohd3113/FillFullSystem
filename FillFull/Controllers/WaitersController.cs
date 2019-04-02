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
                if (ifstillworking.WaiterBreaks != null)
                {
                    startmodel.waiterBreaks = ifstillworking.WaiterBreaks.ToList();
                }

                startmodel.Total_Hour = (DateTime.Now - startmodel.Start.Value).TotalHours;
                startmodel.TotalMin = (DateTime.Now - startmodel.Start.Value).TotalMinutes;
                startmodel.Total_Wage = Convert.ToDecimal(startmodel.Total_Hour) * wa.Wage;
                //var employeeexittime = employee.Posta.End.ToShortTimeString().ToString();
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
                var employeeentrytime = startWorkViewModel.Start.Value.ToShortTimeString().ToString();
                startWorkViewModel.Total_Hour = DateTime.Parse(DateTime.Now.ToShortTimeString()).Subtract(DateTime.Parse(employeeentrytime)).TotalHours;
                startWorkViewModel.Total_Wage = Convert.ToDecimal(startWorkViewModel.Total_Wage) * db.Waiters.SingleOrDefault(p => p.WaiterID == startWorkViewModel.WaiterID).Wage;
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
            runingshift.IsClosed = true;
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
