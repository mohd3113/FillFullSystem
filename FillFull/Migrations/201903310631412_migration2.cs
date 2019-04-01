namespace FillFull.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class migration2 : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.WaiterBreaks", "WaiterWorkID");
            CreateIndex("dbo.WaiterWorks", "WaiterID");
            AddForeignKey("dbo.WaiterBreaks", "WaiterWorkID", "dbo.WaiterWorks", "WaiterWorkID", cascadeDelete: true);
            AddForeignKey("dbo.WaiterWorks", "WaiterID", "dbo.Waiters", "WaiterID", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.WaiterWorks", "WaiterID", "dbo.Waiters");
            DropForeignKey("dbo.WaiterBreaks", "WaiterWorkID", "dbo.WaiterWorks");
            DropIndex("dbo.WaiterWorks", new[] { "WaiterID" });
            DropIndex("dbo.WaiterBreaks", new[] { "WaiterWorkID" });
        }
    }
}
