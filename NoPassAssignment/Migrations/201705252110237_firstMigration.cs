namespace NoPassAssignment.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class firstMigration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SessionRecords",
                c => new
                    {
                        SessionId = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(),
                        SessionExpired = c.Int(nullable: false),
                        ApplicationUser_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.SessionId)
                .ForeignKey("dbo.AspNetUsers", t => t.ApplicationUser_Id)
                .Index(t => t.ApplicationUser_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SessionRecords", "ApplicationUser_Id", "dbo.AspNetUsers");
            DropIndex("dbo.SessionRecords", new[] { "ApplicationUser_Id" });
            DropTable("dbo.SessionRecords");
        }
    }
}
