namespace FillFull.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class migration5 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.WaiterWorks", "TotalMin", c => c.Double(nullable: false));
            AddColumn("dbo.WaiterWorks", "TotalExtraMin", c => c.Double(nullable: false));
            AddColumn("dbo.WaiterWorks", "Wage", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.WaiterWorks", "ExtraTimeWage", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.Waiters", "MaxWorkingHours", c => c.Double(nullable: false));
            AddColumn("dbo.Waiters", "WageafterMaxHours", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.Waiters", "FirstName", c => c.String(nullable: false));
            AlterColumn("dbo.Waiters", "LastName", c => c.String(nullable: false));
            AlterColumn("dbo.Waiters", "StartTime", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Waiters", "StartTime", c => c.String());
            AlterColumn("dbo.Waiters", "LastName", c => c.String());
            AlterColumn("dbo.Waiters", "FirstName", c => c.String());
            DropColumn("dbo.Waiters", "WageafterMaxHours");
            DropColumn("dbo.Waiters", "MaxWorkingHours");
            DropColumn("dbo.WaiterWorks", "ExtraTimeWage");
            DropColumn("dbo.WaiterWorks", "Wage");
            DropColumn("dbo.WaiterWorks", "TotalExtraMin");
            DropColumn("dbo.WaiterWorks", "TotalMin");
        }
    }
}
