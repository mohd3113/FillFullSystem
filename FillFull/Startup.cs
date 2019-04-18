using Microsoft.Owin;
using Owin;
using System.Data.Entity.Migrations;

[assembly: OwinStartupAttribute(typeof(FillFull.Startup))]
namespace FillFull
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            //var configuration = new Migrations.Configuration();
            //var migrator = new DbMigrator(configuration);
            //migrator.Update();
        }
    }
}
