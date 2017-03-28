using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Nop.Plugin.Shipping.DHL.Models
{
    public class DHLListModel : BaseNopModel
    {
        public DHLListModel()
        {
            AvailableCurrency = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Plugins.Shipping.DHL.Fields.LimitMethodsToCreated")]
        public bool LimitMethodsToCreated { get; set; }
        [NopResourceDisplayName("Plugins.Shipping.DHL.Fields.VolumetricDivisor")]
        public decimal VolumetricDivisor { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.DHL.Fields.CurrencyCode")]
        public string CurrencyCode { get; set; }

        public IList<SelectListItem> AvailableCurrency { get; set; }
    }
}