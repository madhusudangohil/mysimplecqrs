using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using CQRS.Common;
using Newtonsoft.Json.Linq;

namespace CQRS.ES
{
    public interface IEventStore
    {
        void SaveEvents(Guid aggregateId, IEnumerable<Event> events, int expectedVersion);
        List<Event> GetEventsForAggregate(Guid aggregateId);
    }

    public class EventDescriptor
    {

        public String EventData { get; set; }
        public string EventName { get; set; }
        public Guid Id { get; set; }
        public int Version { get; set; }

        public int EventId { get; set; }
        public EventDescriptor()
        {

        }
        public EventDescriptor(Guid id, String eventName, String eventData, int version)
        {
            EventData = eventData;
            Version = version;
            EventName = eventName;
            Id = id;
        }
    }

    public class EventStoreContext : DbContext
    {
        public DbSet<EventDescriptor> eventDescriptor { get; set; }

        public EventStoreContext()
        {

        }

        public EventStoreContext(string connectionString): base(connectionString)
        {

        }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EventDescriptor>().HasKey(x => x.EventId).Property(p => p.EventId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            base.OnModelCreating(modelBuilder);
        }
    }


    public class EventStore : IEventStore
    {
        private readonly IEventPublisher _publisher;

        
        public EventStore(IEventPublisher publisher)
        {
            _publisher = publisher;
        }

        //private readonly Dictionary<Guid, List<EventDescriptor>> _current = new Dictionary<Guid, List<EventDescriptor>>();
        EventStoreContext _current = new EventStoreContext("EventStoreContext");

        public void SaveEvents(Guid aggregateId, IEnumerable<Event> events, int expectedVersion)
        {
            List<EventDescriptor> eventDescriptors;

            // try to get event descriptors list for given aggregate id
            // otherwise -> create empty dictionary
            //if(!_current.TryGetValue(aggregateId, out eventDescriptors))
            //{
            //    eventDescriptors = new List<EventDescriptor>();
            //_current.Add(aggregateId,eventDescriptors);
            //}
            eventDescriptors = _current.eventDescriptor.Where(x => x.Id == aggregateId).ToList();
            
            // check whether latest event version matches current aggregate version
            // otherwise -> throw exception
            if(eventDescriptors!= null && eventDescriptors.Count > 0 && eventDescriptors[eventDescriptors.Count - 1].Version != expectedVersion && expectedVersion != -1)
            {
                throw new ConcurrencyException();
            }
            var i = expectedVersion;

            // iterate through current aggregate events increasing version with each processed event
            foreach (var @event in events)
            {
                i++;
                @event.Version = i;

                // push event to the event descriptors list for current aggregate
                //eventDescriptors.Add(new EventDescriptor(aggregateId,@event.GetType().Name, @event,i));
                _current.eventDescriptor.Add(new EventDescriptor(aggregateId, @event.GetType().Name, JsonConvert.SerializeObject(@event), i));

                // publish current event to the bus for further processing by subscribers
                _publisher.Publish(@event);
            }
            _current.SaveChanges();
        }

        // collect all processed events for given aggregate and return them as a list
        // used to build up an aggregate from its history (Domain.LoadsFromHistory)
        public  List<Event> GetEventsForAggregate(Guid aggregateId)
        {
            List<EventDescriptor> eventDescriptors = _current.eventDescriptor.Where(x => x.Id == aggregateId).ToList();
            //if (!_current.TryGetValue(aggregateId, out eventDescriptors))
            if(eventDescriptors == null || eventDescriptors.Count == 0)
            {
                throw new AggregateNotFoundException();
            }
            var settings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            };

            return eventDescriptors.Select(desc => JsonConvert.DeserializeObject<Event>(desc.EventData, new EventConverter())).ToList();
        }
    }


    public class EventConverter : Newtonsoft.Json.Converters.CustomCreationConverter<Event>
    {
        public override Event Create(Type objectType)
        {
            throw new NotImplementedException();
        }

        public Event Create(Type objectType, JObject jObject)
        {
            var type = (string)jObject.Property("Type");

            switch (type)
            {
                case "InventoryItemDeactivated":
                    return new InventoryItemDeactivated();
                case "InventoryItemCreated":
                    return new InventoryItemCreated();
                case "InventoryItemRenamed":
                    return new InventoryItemRenamed();
                case "ItemsCheckedInToInventory":
                    return new ItemsCheckedInToInventory();
                case "ItemsRemovedFromInventory":
                    return new ItemsRemovedFromInventory();
            }

            throw new ApplicationException(String.Format("The animal type {0} is not supported!", type));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // Load JObject from stream 
            JObject jObject = JObject.Load(reader);

            // Create target object based on JObject 
            var target = Create(objectType, jObject);

            // Populate the object properties 
            serializer.Populate(jObject.CreateReader(), target);

            return target;
        }
    }


    public class AggregateNotFoundException : Exception
    {
    }

    public class ConcurrencyException : Exception
    {
    }
}
