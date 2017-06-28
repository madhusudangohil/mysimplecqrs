namespace CQRS.ES.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class init : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.EventDescriptors",
                c => new
                    {
                        EventId = c.Int(nullable: false, identity: true),
                        EventData = c.String(),
                        EventName = c.String(),
                        Id = c.Guid(nullable: false),
                        Version = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.EventId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.EventDescriptors");
        }
    }
}
