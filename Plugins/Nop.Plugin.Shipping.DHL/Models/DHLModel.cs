using System.Collections.Generic;
using System.Web.Mvc;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Shipping.DHL.Models
{
    public class DHLModel : BaseNopEntityModel
    {
        public DHLModel()
        {
            AvailableCountries = new List<SelectListItem>();
            AvailableStates = new List<SelectListItem>();
            AvailableShippingMethods = new List<SelectListItem>();
            AvailableStores = new List<SelectListItem>();
            AvailableWarehouses = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Plugins.Shipping.DHL.Fields.Store")]
        public int StoreId { get; set; }
        [NopResourceDisplayName("Plugins.Shipping.DHL.Fields.Store")]
        public string StoreName { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.DHL.Fields.Warehouse")]
        public int WarehouseId { get; set; }
        [NopResourceDisplayName("Plugins.Shipping.DHL.Fields.Warehouse")]
        public string WarehouseName { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.DHL.Fields.Country")]
        public int CountryId { get; set; }
        [NopResourceDisplayName("Plugins.Shipping.DHL.Fields.Country")]
        public string CountryName { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.DHL.Fields.StateProvince")]
        public int StateProvinceId { get; set; }
        [NopResourceDisplayName("Plugins.Shipping.DHL.Fields.StateProvince")]
        public string StateProvinceName { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.DHL.Fields.Zip")]
        public string Zip { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.DHL.Fields.ShippingMethod")]
        public int ShippingMethodId { get; set; }
        [NopResourceDisplayName("Plugins.Shipping.DHL.Fields.ShippingMethod")]
        public string ShippingMethodName { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.DHL.Fields.From")]
        public decimal From { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.DHL.Fields.To")]
        public decimal To { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.DHL.Fields.IsFixedRate")]
        public bool IsFixedRate { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.DHL.Fields.FixedRate")]
        public decimal FixedRate { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.DHL.Fields.Factor")]
        public decimal Factor { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.DHL.Fields.FreeShippingOverXEnabled")]
        public bool FreeShippingOverXEnabled { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.DHL.Fields.FreeShippingOverXValue")]
        public decimal FreeShippingOverXValue { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.DHL.Fields.DataHtml")]
        public string DataHtml { get; set; }
        public string DefaultCurrencyCode { get; set; }

        public IList<SelectListItem> AvailableCountries { get; set; }
        public IList<SelectListItem> AvailableStates { get; set; }
        public IList<SelectListItem> AvailableShippingMethods { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }
        public IList<SelectListItem> AvailableWarehouses { get; set; }
    }
}