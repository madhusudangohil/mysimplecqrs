using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using CQRS.Common;
using CQRS.ES;
using SimpleCQRS;

namespace CQRSGui
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterRoutes(RouteTable.Routes);

            var bus = new FakeBus();
            var ebus = new EventBus();

            var storage = new EventStore(ebus);
            var rep = new Repository<InventoryItem>(storage);
            var commands = new InventoryCommandHandlers(rep);
            bus.RegisterHandler<CheckInItemsToInventory>(commands.Handle);
            bus.RegisterHandler<CreateInventoryItem>(commands.Handle);
            bus.RegisterHandler<DeactivateInventoryItem>(commands.Handle);
            bus.RegisterHandler<RemoveItemsFromInventory>(commands.Handle);
            bus.RegisterHandler<RenameInventoryItem>(commands.Handle);
            var detail = new InventoryItemDetailView();
            ebus.RegisterHandler<InventoryItemCreated>(detail.Handle);
            ebus.RegisterHandler<InventoryItemDeactivated>(detail.Handle);
            ebus.RegisterHandler<InventoryItemRenamed>(detail.Handle);
            ebus.RegisterHandler<ItemsCheckedInToInventory>(detail.Handle);
            ebus.RegisterHandler<ItemsRemovedFromInventory>(detail.Handle);
            var list = new InventoryListView();
            ebus.RegisterHandler<InventoryItemCreated>(list.Handle);
            ebus.RegisterHandler<InventoryItemRenamed>(list.Handle);
            ebus.RegisterHandler<InventoryItemDeactivated>(list.Handle);
            ServiceLocator.Bus = bus;
        }
    }
}