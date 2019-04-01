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
                .PrimaryKey(t => t.WaiterBreakID);
            
            CreateTable(
                "dbo.Waiters",
                c => new
                    {
                        WaiterID = c.Int(nullable: false, identity: true),
                        FirstName = c.String(),
                        LastName = c.String(),
                        Email = c.String(),
                        PhoneNumber = c.String(),
                        Address = c.String(),
                        StartTime = c.String(),
                        EndTime = c.String(),
                        ImagePath = c.String(),
                        Wage = c.Decimal(nullable: false, precision: 18, scale: 2),
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
                        IsClosed = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.WaiterWorkID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.WaiterWorks");
            DropTable("dbo.Waiters");
            DropTable("dbo.WaiterBreaks");
            DropTable("dbo.Admins");
        }
    }
}
