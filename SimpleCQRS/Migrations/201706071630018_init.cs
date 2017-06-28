namespace SimpleCQRS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class init : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.InventoryItemDetailsDtoes",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(),
                        CurrentCount = c.Int(nullable: false),
                        Version = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.InventoryItemListDtoes",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.InventoryItemListDtoes");
            DropTable("dbo.InventoryItemDetailsDtoes");
        }
    }
}
