namespace FillFull.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class migration1 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Admins",
                c => new
                    {
                        AdminID = c.Int(nullable: false, identity: true),
                        FirstName = c.String(),
                        LastName = c.String(),
                        Email = c.String(),
                        PhoneNumber = c.String(),
                        Address = c.String(),
                    })
                .PrimaryKey(t => t.AdminID);
            
            CreateTable(
                "dbo.WaiterBreaks",
                c => new
                    {
                        WaiterBreakID = c.Int(nullable: false, identity: true),
                        WaiterWorkID = c.Int(nullable: false),
                        StartAt = c.DateTime(nullable: false),
                        EndAt = c.DateTime(),
                    })
                .PrimaryKey(t => t.WaiterBreakID)
                .ForeignKey("dbo.WaiterWorks", t => t.WaiterWorkID, cascadeDelete: true)
                .Index(t => t.WaiterWorkID);
            
            CreateTable(
                "dbo.WaiterWorks",
                c => new
                    {
                        WaiterWorkID = c.Int(nullable: false, identity: true),
                        WaiterID = c.Int(nullable: false),
                        StartAt = c.DateTime(nullable: false),
                        EndAt = c.DateTime(),
                        TotalMin = c.Double(nullable: false),
                        IsClosed = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.WaiterWorkID)
                .ForeignKey("dbo.Waiters", t => t.WaiterID, cascadeDelete: true)
                .Index(t => t.WaiterID);
            
            CreateTable(
                "dbo.Waiters",
                c => new
                    {
                        WaiterID = c.Int(nullable: false, identity: true),
                        FirstName = c.String(nullable: false),
                        LastName = c.String(nullable: false),
                        Email = c.String(),
                        PhoneNumber = c.String(),
                        Address = c.String(),
                        StartTime = c.String(nullable: false),
                        EndTime = c.String(),
                        ImagePath = c.String(),
                        Wage = c.Decimal(nullable: false, precision: 18, scale: 2),
                        MaxWorkingHours = c.Double(nullable: false),
                        WageafterMaxHours = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.WaiterID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.WaiterBreaks", "WaiterWorkID", "dbo.WaiterWorks");
            DropForeignKey("dbo.WaiterWorks", "WaiterID", "dbo.Waiters");
            DropIndex("dbo.WaiterWorks", new[] { "WaiterID" });
            DropIndex("dbo.WaiterBreaks", new[] { "WaiterWorkID" });
            DropTable("dbo.Waiters");
            DropTable("dbo.WaiterWorks");
            DropTable("dbo.WaiterBreaks");
            DropTable("dbo.Admins");
        }
    }
}
