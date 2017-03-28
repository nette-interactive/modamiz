using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.Shipping.DHL
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.Shipping.DHL.SaveGeneralSettings",
                 "Plugins/DHL/SaveGeneralSettings",
                 new { controller = "DHL", action = "SaveGeneralSettings", },
                 new[] { "Nop.Plugin.Shipping.DHL.Controllers" }
            );

            routes.MapRoute("Plugin.Shipping.DHL.AddPopup",
                 "Plugins/DHL/AddPopup",
                 new { controller = "DHL", action = "AddPopup" },
                 new[] { "Nop.Plugin.Shipping.DHL.Controllers" }
            );
            routes.MapRoute("Plugin.Shipping.DHL.EditPopup",
                 "Plugins/DHL/EditPopup",
                 new { controller = "DHL", action = "EditPopup" },
                 new[] { "Nop.Plugin.Shipping.DHL.Controllers" }
            );
        }
        public int Priority
        {
            get
            {
                return 0;
            }
        }
    }
}
