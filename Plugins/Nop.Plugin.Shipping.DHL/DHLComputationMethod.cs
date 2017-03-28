using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Routing;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Shipping;
using Nop.Core.Plugins;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Tracking;
using Nop.Plugin.Shipping.DHL.Data;
using Nop.Plugin.Shipping.DHL.Services;
using Nop.Services.Catalog;
using Nop.Services.Logging;
using Nop.Core.Domain.Logging;
using System.Collections.Generic;
using Nop.Services.Orders;
using Nop.Core.Domain.Discounts;
using System.Linq;
using Nop.Core.Domain.Orders;

namespace Nop.Plugin.Shipping.DHL
{
    /// <summary>
    /// DHL post computation method
    /// </summary>
    public class DHLComputationMethod : BasePlugin, IShippingRateComputationMethod
    {
        #region Constants

        private const int MIN_LENGTH = 50; // 5 cm
        private const int MIN_WEIGHT = 1; // 1 g
        private const int MAX_LENGTH = 1050; // 105 cm
        private const int MAX_WEIGHT = 20000; // 20 Kg
        private const int MIN_GIRTH = 160; // 16 cm
        private const int MAX_GIRTH = 1050; // 105 cm

        #endregion

        #region Fields

        private readonly IMeasureService _measureService;
        private readonly IShippingService _shippingService;
        private readonly ISettingService _settingService;
        private readonly DHLSettings _dhlSettings;
        private readonly IDHLService _dhlService;
        private readonly DHLObjectContext _objectContext;
        private readonly IStoreContext _storeContext;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly ILogger _logger;
        private readonly ICurrencyService _currencyService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IWorkContext _workContext;


        #endregion

        #region Ctor
        public DHLComputationMethod(IMeasureService measureService,
            IShippingService shippingService,
            ISettingService settingService,
            DHLSettings dhlSettings,
            DHLObjectContext objectContext,
            IDHLService dhlService,
            IStoreContext storeContext,
            IPriceCalculationService priceCalculationService,
            ILogger logger,
            ICurrencyService currencyService,
            IOrderTotalCalculationService orderTotalCalculationService,
            IWorkContext workContext
            )
        {
            this._measureService = measureService;
            this._shippingService = shippingService;
            this._settingService = settingService;
            this._dhlSettings = dhlSettings;
            this._objectContext = objectContext;
            this._storeContext = storeContext;
            this._priceCalculationService = priceCalculationService;
            this._logger = logger;
            this._dhlService = dhlService;
            this._currencyService = currencyService;
            this._orderTotalCalculationService = orderTotalCalculationService;
            this._workContext = workContext;
        }
        #endregion

        #region Utilities


        private decimal? GetRate(decimal weight, int shippingMethodId,
            int storeId, int warehouseId, int countryId, int stateProvinceId, string zip, GetShippingOptionRequest getShippingOptionRequest)
        {

            decimal queryWeight = weight;
            decimal realWeight = _shippingService.GetTotalWeight(getShippingOptionRequest);

            if (realWeight > weight)
                queryWeight = realWeight;

            var dhlRecord = _dhlService.FindRecord(shippingMethodId,
                storeId, warehouseId, countryId, stateProvinceId, zip, queryWeight);

            if (dhlRecord == null)
            {
                if (_dhlSettings.LimitMethodsToCreated)
                    return null;

                return decimal.Zero;
            }

            #region X Üzeri Ücretsiz Kargo

            decimal shippingTotal = decimal.Zero;
            decimal? orderTotal = decimal.Zero;


            foreach (var packageItem in getShippingOptionRequest.Items)
            {
                //TODO we should use getShippingOptionRequest.Items.GetQuantity() method to get subtotal
                orderTotal += _priceCalculationService.GetSubTotal(packageItem.ShoppingCartItem);
            }

            if (orderTotal.HasValue)
            {
                var trueOrderTotal = _currencyService.ConvertToPrimaryStoreCurrency(orderTotal.Value, _workContext.WorkingCurrency);
                if (dhlRecord.FreeShippingOverXEnabled && trueOrderTotal >= dhlRecord.FreeShippingOverXValue)
                {
                    return decimal.Zero;
                }
            }

            #endregion X Üzeri Ücretsiz Kargo

            if (dhlRecord.IsFixedRate)
                return dhlRecord.FixedRate;

            shippingTotal += dhlRecord.Factor;

            if (shippingTotal < decimal.Zero)
                shippingTotal = decimal.Zero;
            return shippingTotal;
        }


        #endregion

        #region Methods

        /// <summary>
        ///  Gets available shipping options
        /// </summary>
        /// <param name="getShippingOptionRequest">A request for getting shipping options</param>
        /// <returns>Represents a response of getting shipping rate options</returns>
        public GetShippingOptionResponse GetShippingOptions(GetShippingOptionRequest getShippingOptionRequest)
        {
            if (getShippingOptionRequest == null)
                throw new ArgumentNullException("getShippingOptionRequest");

            var response = new GetShippingOptionResponse();

            if (getShippingOptionRequest.Items == null || getShippingOptionRequest.Items.Count == 0)
            {
                response.AddError("Kargolanacak bir ürün bulunamadı.");
                return response;
            }
            if (getShippingOptionRequest.ShippingAddress == null)
            {
                response.AddError("Kargo adresi bulunamadı");
                return response;
            }

            var storeId = getShippingOptionRequest.StoreId;
            if (storeId == 0)
                storeId = _storeContext.CurrentStore.Id;

            int countryId = getShippingOptionRequest.ShippingAddress.CountryId.HasValue ? getShippingOptionRequest.ShippingAddress.CountryId.Value : 0;
            int stateProvinceId = getShippingOptionRequest.ShippingAddress.StateProvinceId.HasValue ? getShippingOptionRequest.ShippingAddress.StateProvinceId.Value : 0;
            int warehouseId = getShippingOptionRequest.WarehouseFrom != null ? getShippingOptionRequest.WarehouseFrom.Id : 0;
            string zip = getShippingOptionRequest.ShippingAddress.ZipPostalCode;

            decimal length = decimal.Zero;
            decimal height = decimal.Zero;
            decimal width = decimal.Zero;

            _shippingService.GetDimensions(getShippingOptionRequest.Items, out width, out length, out height);

            decimal volumetricWeight = (length * height * width) / _dhlSettings.VolumetricDivisor;

            var shippingMethods = _shippingService.GetAllShippingMethods(countryId);

            foreach (var shippingMethod in shippingMethods)
            {
                decimal? rate = GetRate(volumetricWeight, shippingMethod.Id,
                    storeId, warehouseId, countryId, stateProvinceId, zip, getShippingOptionRequest);

                if (rate.HasValue)
                {
                    var shippingOption = new ShippingOption();
                    shippingOption.Name = shippingMethod.GetLocalized(x => x.Name);
                    shippingOption.Description = shippingMethod.GetLocalized(x => x.Description);
                    shippingOption.Rate = _currencyService.ConvertToPrimaryStoreCurrency(rate.Value, _currencyService.GetCurrencyByCode(_dhlSettings.CurrencyCode));
                    response.ShippingOptions.Add(shippingOption);
                }
            }

            return response;
        }

        public decimal? GetFixedRate(GetShippingOptionRequest getShippingOptionRequest)
        {
            return null;
        }

        /// <summary>
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "DHL";
            routeValues = new RouteValueDictionary { { "Namespaces", "Nop.Plugin.Shipping.DHL.Controllers" }, { "area", null } };
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
            var settings = new DHLSettings
            {
                LimitMethodsToCreated = false,
                CurrencyCode = "EUR",
                VolumetricDivisor = 5000
            };
            _settingService.SaveSetting(settings);
            _objectContext.Install();

            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.DHL.Fields.Store", "Mağaza");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.DHL.Fields.Store.Hint", "* seçilirse tüm mağazalar için geçerli olacaktır.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.DHL.Fields.Warehouse", "Depo");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.DHL.Fields.Warehouse.Hint", "* seçilirse bu gönderim oranı tüm depolar için geçerli olacaktır");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.DHL.Fields.Country", "Ülke");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.DHL.Fields.Country.Hint", "* seçilirse tüm ülkelerin müşterileri için gönderim bedeli geçerli olacaktır.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.DHL.Fields.StateProvince", "Şehir / Eyalet");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.DHL.Fields.StateProvince.Hint", "* seçilirse tüm şehirlerin ya da eyaletlerin tüm müşterileri için bu gönderim bedeli geçerli olacaktır.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.DHL.Fields.Zip", "Posta Kodu");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.DHL.Fields.Zip.Hint", "Boş bırakılırsa posta kodu ne olursa olsun bu gönderim bedeli tüm müşteriler için geçerli olacaktır.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.DHL.Fields.ShippingMethod", "Kargo Methodu");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.DHL.Fields.ShippingMethod.Hint", "The shipping method.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.DHL.Fields.From", "Volumetrik Ağırlık Başlangıç");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.DHL.Fields.From.Hint", "Volumetrik Ağırlık: Uzunluk x Genişlik x Yükseklik(cm3) / [Volumetrik Böleni] = Volumetrik Ağırlık(kg)");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.DHL.Fields.To", "Volumetrik Ağırlık Bitiş");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.DHL.Fields.To.Hint", "Volumetrik Ağırlık: Uzunluk x Genişlik x Yükseklik(cm3) / [Volumetrik Böleni] = Volumetrik Ağırlık(kg)");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.DHL.Fields.LimitMethodsToCreated", "Limit shipping methods to configured ones");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.DHL.Fields.LimitMethodsToCreated.Hint", "If you check this option, then your customers will be limited to shipping options configured here. Otherwise, they'll be able to choose any existing shipping options even they've not configured here (zero shipping fee in this case).");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.DHL.Fields.DataHtml", "Data");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.DHL.AddRecord", "Kayıt ekle");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.DHL.Formula", "Oran hesaplama formülü");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.DHL.Formula.Value", " Uzunluk x Genişlik x Yükseklik (cm3) / [Volumetrik Böleni] = Volumetrik Ağırlık (kg) ");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.DHL.Fields.VolumetricDivisor", "Volumetrik Böleni");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.DHL.Fields.Factor", "Hesaplama Çarpanı");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.DHL.Fields.CurrencyCode", "Varsayılan Kur");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.DHL.Fields.IsFixedRate", "Sabit Ücret ?");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.DHL.Fields.Fixed", "Ücret");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.DHL.Fields.FreeShippingOverXEnabled", "'X' üzerinden ücretsiz gönderim");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.DHL.Fields.FreeShippingOverXValue", "Değer 'X'");

            base.Install();
        }

        public override void Uninstall()
        {
            _settingService.DeleteSetting<DHLSettings>();
            _objectContext.Uninstall();

            this.DeletePluginLocaleResource("Plugins.Shipping.DHL.Fields.Store");
            this.DeletePluginLocaleResource("Plugins.Shipping.DHL.Fields.Store.Hint");
            this.DeletePluginLocaleResource("Plugins.Shipping.DHL.Fields.Warehouse");
            this.DeletePluginLocaleResource("Plugins.Shipping.DHL.Fields.Warehouse.Hint");
            this.DeletePluginLocaleResource("Plugins.Shipping.DHL.Fields.Country");
            this.DeletePluginLocaleResource("Plugins.Shipping.DHL.Fields.Country.Hint");
            this.DeletePluginLocaleResource("Plugins.Shipping.DHL.Fields.StateProvince");
            this.DeletePluginLocaleResource("Plugins.Shipping.DHL.Fields.StateProvince.Hint");
            this.DeletePluginLocaleResource("Plugins.Shipping.DHL.Fields.Zip");
            this.DeletePluginLocaleResource("Plugins.Shipping.DHL.Fields.Zip.Hint");
            this.DeletePluginLocaleResource("Plugins.Shipping.DHL.Fields.ShippingMethod");
            this.DeletePluginLocaleResource("Plugins.Shipping.DHL.Fields.ShippingMethod.Hint");
            this.DeletePluginLocaleResource("Plugins.Shipping.DHL.Fields.From");
            this.DeletePluginLocaleResource("Plugins.Shipping.DHL.Fields.From.Hint");
            this.DeletePluginLocaleResource("Plugins.Shipping.DHL.Fields.To");
            this.DeletePluginLocaleResource("Plugins.Shipping.DHL.Fields.To.Hint");
            this.DeletePluginLocaleResource("Plugins.Shipping.DHL.Fields.LimitMethodsToCreated");
            this.DeletePluginLocaleResource("Plugins.Shipping.DHL.Fields.LimitMethodsToCreated.Hint");
            this.DeletePluginLocaleResource("Plugins.Shipping.DHL.Fields.DataHtml");
            this.DeletePluginLocaleResource("Plugins.Shipping.DHL.AddRecord");
            this.DeletePluginLocaleResource("Plugins.Shipping.DHL.Formula");
            this.DeletePluginLocaleResource("Plugins.Shipping.DHL.Formula.Value");
            this.DeletePluginLocaleResource("Plugins.Shipping.DHL.VolumetricDivisor");
            this.DeletePluginLocaleResource("Plugins.Shipping.DHL.Factor");
            this.DeletePluginLocaleResource("Plugins.Shipping.DHL.Fields.CurrencyCode");
            this.DeletePluginLocaleResource("Plugins.Shipping.DHL.Fields.FreeShippingOverXEnabled");
            this.DeletePluginLocaleResource("Plugins.Shipping.DHL.Fields.FreeShippingOverXValue");


            base.Uninstall();
        }
        #endregion

        #region Properties

        /// <summary>
        /// Gets a shipping rate computation method type
        /// </summary>
        public ShippingRateComputationMethodType ShippingRateComputationMethodType
        {
            get
            {
                return ShippingRateComputationMethodType.Offline;
            }
        }

        /// <summary>
        /// Gets a shipment tracker
        /// </summary>
        public IShipmentTracker ShipmentTracker
        {
            get { return null; }
        }

        #endregion
    }
}