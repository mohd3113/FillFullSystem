using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
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
    public class AdminsController : Controller
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
        // GET: Admins
        public ActionResult Index()
        {
            return View(db.Admins.ToList());
        }

        // GET: Admins/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Admin admin = db.Admins.Find(id);
            if (admin == null)
            {
                return HttpNotFound();
            }
            return View(admin);
        }

        // GET: Admins/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admins/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "AdminID,FirstName,LastName,Email,PhoneNumber,Address")] Admin admin)
        {
            if (ModelState.IsValid)
            {
                string password = admin.LastName.Substring(0, 2).Trim() + "_Ad_12345";
                if (ModelState.IsValid)
                {
                    var user = new ApplicationUser { UserName = admin.Email.Trim(), Email = admin.Email.Trim() };
                    var result = UserManager.Create(user, password);
                    if (result.Succeeded)
                    {
                        var result1 = CreateRolesandUsers("admin", user);
                        if (result1.Succeeded)
                        {
                            db.Admins.Add(admin);
                            db.SaveChanges();
                            return RedirectToAction("Index");
                        }
                    }
                }
            }

            return View(admin);
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
        // GET: Admins/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Admin admin = db.Admins.Find(id);
            if (admin == null)
            {
                return HttpNotFound();
            }
            return View(admin);
        }

        // POST: Admins/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "AdminID,FirstName,LastName,Email,PhoneNumber,Address")] Admin admin)
        {
            if (ModelState.IsValid)
            {
                db.Entry(admin).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(admin);
        }

        // GET: Admins/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Admin admin = db.Admins.Find(id);
            if (admin == null)
            {
                return HttpNotFound();
            }
            return View(admin);
        }

        // POST: Admins/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Admin admin = db.Admins.Find(id);
            db.Admins.Remove(admin);
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
