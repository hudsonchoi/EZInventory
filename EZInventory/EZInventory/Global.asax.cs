using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace EZInventory
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            //WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteTable.Routes.MapRoute("Product", "product/edit/{sku}", new { Controller = "Product", Action = "Edit" });
            RouteTable.Routes.MapRoute("GetQty", "inventory/getqty/{id}", new { Controller = "Inventory", Action = "GetQty" });
            RouteTable.Routes.MapRoute("GetInventoriesByCompany", "inventory/getinv/{id}", new { Controller = "Inventory", Action = "GetInventoriesByCompany" });
            RouteTable.Routes.MapRoute("GetShipTos", "company/getshiptos/{id}", new { Controller = "Company", Action = "GetShipTosByCompany" });
            RouteTable.Routes.MapRoute("SetBarcode", "product/create/{barcode}", new { Controller = "Item", Action = "Create" });
            RouteTable.Routes.MapRoute("GetBarcode", "product/{sku}", new { Controller = "Item", Action = "GetBarcode" });
            RouteTable.Routes.MapRoute("CreatInventoryWithBarcode", "inventory/create/{barcode}", new { Controller = "Inventory", Action = "Create" });
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            //DbInterception.Add(new SchoolInterceptorTransientErrors());
            //DbInterception.Add(new SchoolInterceptorLogging());
        }
    }
}