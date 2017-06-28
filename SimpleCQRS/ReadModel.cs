using System;
using System.Collections.Generic;
using System.Data.Entity;
using CQRS.Common;
namespace SimpleCQRS
{
    public interface IReadModelFacade
    {
        IEnumerable<InventoryItemListDto> GetInventoryItems();
        InventoryItemDetailsDto GetInventoryItemDetails(Guid id);
    }

    public class InventoryItemDetailsDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int CurrentCount { get; set; }
        public int Version { get; set; }

        public InventoryItemDetailsDto()
        {

        }
        public InventoryItemDetailsDto(Guid id, string name, int currentCount, int version)
        {
            Id = id;
            Name = name;
            CurrentCount = currentCount;
            Version = version;
        }
    }

    public class InventoryItemListDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public InventoryItemListDto()
        {

        }
        public InventoryItemListDto(Guid id, string name)
        {
            Id = id;
            Name = name;
        }
    }

    public class InventoryListView : Handles<InventoryItemCreated>, Handles<InventoryItemRenamed>, Handles<InventoryItemDeactivated>
    {
        private ReadDBContext Readcontext
        {
            get
            {
                return new ReadDBContext("ReadDBContext");
            }
        }
        public void Handle(InventoryItemCreated message)
        {
            //BullShitDatabase.list.Add(new InventoryItemListDto(message.Id, message.Name));
            ReadDBContext dbcontext = Readcontext;
            dbcontext.InventoryItemList.Add(new InventoryItemListDto(message.Id, message.Name));
            dbcontext.SaveChanges();
        }

        public void Handle(InventoryItemRenamed message)
        {
            //var item = BullShitDatabase.list.Find(x => x.Id == message.Id);
            ReadDBContext dbcontext = Readcontext;
            var item = dbcontext.InventoryItemList.FirstOrDefaultAsync(x => x.Id == message.Id).Result;
            item.Name = message.NewName;
            dbcontext.SaveChanges();
        }

        public void Handle(InventoryItemDeactivated message)
        {
            //BullShitDatabase.list.RemoveAll(x => x.Id == message.Id);
            ReadDBContext dbcontext = Readcontext;
            var item = dbcontext.InventoryItemList.FirstOrDefaultAsync(x => x.Id == message.Id).Result;
            dbcontext.InventoryItemList.Remove(item);
            dbcontext.SaveChanges();
        }
    }

    public class InventoryItemDetailView : Handles<InventoryItemCreated>, Handles<InventoryItemDeactivated>, Handles<InventoryItemRenamed>, Handles<ItemsRemovedFromInventory>, Handles<ItemsCheckedInToInventory>
    {
        private ReadDBContext Readcontext
        {
            get
            {
                return new ReadDBContext("ReadDBContext");
            }
        }
        
        public void Handle(InventoryItemCreated message)
        {
            //BullShitDatabase.details.Add(message.Id, new InventoryItemDetailsDto(message.Id, message.Name, 0,0));
            ReadDBContext dbcontext = Readcontext;
            dbcontext.InventoryItemDetails.Add(new InventoryItemDetailsDto(message.Id, message.Name, 0, 0));
            dbcontext.SaveChanges();
        }

        public void Handle(InventoryItemRenamed message)
        {
            ReadDBContext dbcontext = Readcontext;
            InventoryItemDetailsDto d = GetDetailsItem(message.Id, dbcontext);
            d.Name = message.NewName;
            d.Version = message.Version;
            dbcontext.SaveChanges();
        }

        private InventoryItemDetailsDto GetDetailsItem(Guid id, ReadDBContext dbcontext)
        {
            //InventoryItemDetailsDto d;
            var item = dbcontext.InventoryItemDetails.FirstOrDefaultAsync(x => x.Id == id).Result;
            //if (!BullShitDatabase.details.TryGetValue(id, out d))
            if(item == null)
            {
                throw new InvalidOperationException("did not find the original inventory this shouldnt happen");
            }

            return item;
        }

        public void Handle(ItemsRemovedFromInventory message)
        {
            ReadDBContext dbcontext = Readcontext;
            InventoryItemDetailsDto d = GetDetailsItem(message.Id, dbcontext);
            d.CurrentCount -= message.Count;
            d.Version = message.Version;
            dbcontext.InventoryItemDetails.Remove(d);
            dbcontext.SaveChanges();
        }

        public void Handle(ItemsCheckedInToInventory message)
        {
            ReadDBContext dbcontext = Readcontext;
            InventoryItemDetailsDto d = GetDetailsItem(message.Id, dbcontext);
            d.CurrentCount += message.Count;
            d.Version = message.Version;
            dbcontext.SaveChanges();
        }

        public void Handle(InventoryItemDeactivated message)
        {
            //BullShitDatabase.details.Remove(message.Id);
            ReadDBContext dbcontext = Readcontext;
            var item = dbcontext.InventoryItemList.FirstOrDefaultAsync(x => x.Id == message.Id).Result;
            dbcontext.InventoryItemList.Remove(item);
            dbcontext.SaveChanges();
        }
    }

    public class ReadModelFacade : IReadModelFacade
    {        
        private ReadDBContext dbcontext
        {
            get
            {
                return new ReadDBContext("ReadDBContext");
            }
        }
        public IEnumerable<InventoryItemListDto> GetInventoryItems()
        {
            //return BullShitDatabase.list;
            return dbcontext.InventoryItemList.ToListAsync().Result;
        }

        public InventoryItemDetailsDto GetInventoryItemDetails(Guid id)
        {
            //return BullShitDatabase.details[id];
            return dbcontext.InventoryItemDetails.FirstOrDefaultAsync(x => x.Id == id).Result;
        }
    }

    public class ReadDBContext : DbContext
    {
        public ReadDBContext(string connectionString) : base(connectionString) { }
        public ReadDBContext()
        {

        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<InventoryItemListDto>().HasKey(x => x.Id);
            modelBuilder.Entity<InventoryItemDetailsDto>().HasKey(x => x.Id);
            base.OnModelCreating(modelBuilder);
        }
        public DbSet<InventoryItemListDto> InventoryItemList { get; set; }
        public DbSet<InventoryItemDetailsDto> InventoryItemDetails { get; set; }

    }

    public static class BullShitDatabase
    {
        public static Dictionary<Guid, InventoryItemDetailsDto> details = new Dictionary<Guid,InventoryItemDetailsDto>();
        public static List<InventoryItemListDto> list = new List<InventoryItemListDto>();
    }
}
