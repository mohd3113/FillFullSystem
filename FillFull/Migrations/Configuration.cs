namespace FillFull.Migrations
{
    using FillFull.Models;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<FillFull.Models.FillFullDataContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(FillFull.Models.FillFullDataContext context)
        {
            //using (var db = new FillFullDataContext())
            //{
            //    Admin newadmin = new Admin
            //    {
            //        FirstName = "Manager",
            //        LastName = "Manager",
            //        Email = "Manager@gmail.com",
            //        PhoneNumber = "0097143432434",
            //        Address = "Girna",
            //    };
            //    db.Admins.Add(newadmin);
            //    db.SaveChanges();
            //}
            //CreateUser();
        }
        private IdentityResult CreateUser()
        {
            using (var context1 = new ApplicationDbContext())
            {
                var userstore = new UserStore<ApplicationUser>(context1);
                var usermanager = new UserManager<ApplicationUser>(userstore);
                ApplicationUser admin = new ApplicationUser();
                admin.Email = "Manager@gmail.com";
                admin.UserName = "Manager@gmail.com";
                var result = usermanager.Create(admin, "Super_Admin1");
                AddtoRole(admin.Id);
                return result;
            }
        }
        private IdentityResult AddtoRole(string adminid)
        {
            using (var context1 = new ApplicationDbContext())
            {

                var userstore = new UserStore<ApplicationUser>(context1);
                var usermanager = new UserManager<ApplicationUser>(userstore);
                var store = new RoleStore<IdentityRole>(context1);
                var manager = new RoleManager<IdentityRole>(store);
                // RoleTypes is a class containing constant string values for different roles
                var role = new IdentityRole();
                role.Name = "Manager";
                manager.Create(role);
                // Initialize default user          
                var result = usermanager.AddToRole(adminid, "Manager");
                return result;
            }
        }
    }
}
