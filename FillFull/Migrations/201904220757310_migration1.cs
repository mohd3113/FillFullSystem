namespace FillFull.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class migration1 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Activities",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        ActivityText = c.String(),
                        ActivityDate = c.DateTime(nullable: false),
                        WaiterID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
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
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.WaiterBreaks", "WaiterWorkID", "dbo.WaiterWorks");
            DropForeignKey("dbo.WaiterWorks", "WaiterID", "dbo.Waiters");
            DropForeignKey("dbo.Activities", "WaiterID", "dbo.Waiters");
            DropIndex("dbo.WaiterBreaks", new[] { "WaiterWorkID" });
            DropIndex("dbo.WaiterWorks", new[] { "WaiterID" });
            DropIndex("dbo.Activities", new[] { "WaiterID" });
            DropTable("dbo.Admins");
            DropTable("dbo.WaiterBreaks");
            DropTable("dbo.WaiterWorks");
            DropTable("dbo.Waiters");
            DropTable("dbo.Activities");
        }
    }
}
