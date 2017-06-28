using System;
namespace CQRS.Common
{
    public class Event : Message
    {
        public int Version;
        public string Type;
    }
    
    public class InventoryItemDeactivated : Event {
        public Guid Id { get; set; }

        public InventoryItemDeactivated()
        {

        }
        public InventoryItemDeactivated(Guid id)
        {
            Id = id;
        }
    }

    public class InventoryItemCreated : Event {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public InventoryItemCreated()
        {
            Type = "InventoryItemCreated";
        }
        public InventoryItemCreated(Guid id, string name) {
            Id = id;
            Name = name;
            Type = "InventoryItemCreated";
        }
    }

    public class InventoryItemRenamed : Event
    {
        public Guid Id { get; set; }
        public string NewName { get; set; }

        public InventoryItemRenamed()
        {
            Type = "InventoryItemRenamed";
        }
        public InventoryItemRenamed(Guid id, string newName)
        {
            Id = id;
            NewName = newName;
            Type = "InventoryItemRenamed";
        }
    }

    public class ItemsCheckedInToInventory : Event
    {
        public Guid Id { get; set; }
        public int Count { get; set; }

        public ItemsCheckedInToInventory()
        {
            Type = "ItemsCheckedInToInventory";
        }
        public ItemsCheckedInToInventory(Guid id, int count) {
            Id = id;
            Count = count;
            Type = "ItemsCheckedInToInventory";
        }
    }

    public class ItemsRemovedFromInventory : Event
    {
        public Guid Id { get; set; }
        public int Count { get; set; }

        public ItemsRemovedFromInventory()
        {
            Type = "ItemsRemovedFromInventory";
        }
 
        public ItemsRemovedFromInventory(Guid id, int count) {
            Id = id;
            Count = count;
            Type = "ItemsRemovedFromInventory";
        }
    }
}

