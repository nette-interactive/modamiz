using System.Web.Mvc;
using Nop.Plugin.Shipping.DHL.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Web.Framework.Controllers;
using Nop.Plugin.Shipping.DHL;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Services.Directory;
using Nop.Plugin.Shipping.DHL.Services;
using Nop.Services.Security;
using Nop.Core.Domain.Directory;
using Nop.Core;
using Nop.Web.Framework.Security;
using Nop.Web.Framework.Kendoui;
using System.Linq;
using System;
using System.Text;
using Nop.Web.Framework.Mvc;
using Nop.Plugin.Shipping.DHL.Domain;
using System.Collections.Generic;

namespace Nop.Plugin.Shipping.DHL.Controllers
{
    [AdminAuthorize]
    public class DHLController : BasePluginController
    {
        private readonly IShippingService _shippingService;
        private readonly IStoreService _storeService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly DHLSettings _dhlSettings;
        private readonly IDHLService _dhlService;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;

        private readonly ICurrencyService _currencyService;
        private readonly CurrencySettings _currencySettings;
        private readonly IMeasureService _measureService;
        private readonly MeasureSettings _measureSettings;

        public DHLController(IShippingService shippingService,
            IStoreService storeService,
            ICountryService countryService,
            IStateProvinceService stateProvinceService,
            DHLSettings dhlSettings,
            IDHLService dhlService,
            ISettingService settingService,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            ICurrencyService currencyService,
            CurrencySettings currencySettings,
            IMeasureService measureService,
            MeasureSettings measureSettings)
        {
            this._shippingService = shippingService;
            this._storeService = storeService;
            this._countryService = countryService;
            this._stateProvinceService = stateProvinceService;
            this._dhlSettings = dhlSettings;
            this._dhlService = dhlService;
            this._settingService = settingService;
            this._localizationService = localizationService;
            this._permissionService = permissionService;
            this._currencyService = currencyService;
            this._currencySettings = currencySettings;
            this._measureService = measureService;
            this._measureSettings = measureSettings;
        }

        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            //little hack here
            //always set culture to 'en-US' (Telerik has a bug related to editing decimal values in other cultures). Like currently it's done for admin area in Global.asax.cs
            CommonHelper.SetTelerikCulture();

            base.Initialize(requestContext);
        }


        [ChildActionOnly]
        public ActionResult Configure()
        {
            var model = new DHLListModel();

            model.LimitMethodsToCreated = _dhlSettings.LimitMethodsToCreated;
            model.VolumetricDivisor = _dhlSettings.VolumetricDivisor;
            model.CurrencyCode = _dhlSettings.CurrencyCode;


            foreach (var currency in _currencyService.GetAllCurrencies())
                model.AvailableCurrency.Add(new SelectListItem { Text = currency.Name, Value = currency.CurrencyCode });

            return View("~/Plugins/Shipping.DHL/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAntiForgery]
        public ActionResult SaveGeneralSettings(DHLListModel model)
        {
            _dhlSettings.LimitMethodsToCreated = model.LimitMethodsToCreated;
            _dhlSettings.VolumetricDivisor = model.VolumetricDivisor;
            _dhlSettings.CurrencyCode = model.CurrencyCode;

            _settingService.SaveSetting(_dhlSettings);

            return Json(new { Result = true });
        }

        [HttpPost]
        [AdminAntiForgery]
        public ActionResult RatesList(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings))
                return Content("Bu sayfaya erişim yetkiniz yoktur!");

            var records = _dhlService.GetAll(command.Page - 1, command.PageSize);
            var sbwModel = records.Select(x =>
            {
                var m = new DHLModel
                {
                    Id = x.Id,
                    StoreId = x.StoreId,
                    WarehouseId = x.WarehouseId,
                    ShippingMethodId = x.ShippingMethodId,
                    CountryId = x.CountryId,
                    From = x.From,
                    To = x.To,
                    DefaultCurrencyCode = _dhlSettings.CurrencyCode,
                    Factor = x.Factor,
                    FreeShippingOverXEnabled = x.FreeShippingOverXEnabled,
                    FreeShippingOverXValue = x.FreeShippingOverXValue
                };


                //shipping method
                var shippingMethod = _shippingService.GetShippingMethodById(x.ShippingMethodId);
                m.ShippingMethodName = (shippingMethod != null) ? shippingMethod.Name : "Yok";
                //store
                var store = _storeService.GetStoreById(x.StoreId);
                m.StoreName = (store != null) ? store.Name : "*";
                //warehouse
                var warehouse = _shippingService.GetWarehouseById(x.WarehouseId);
                m.WarehouseName = (warehouse != null) ? warehouse.Name : "*";
                //country
                var c = _countryService.GetCountryById(x.CountryId);
                m.CountryName = (c != null) ? c.Name : "*";
                //state
                var s = _stateProvinceService.GetStateProvinceById(x.StateProvinceId);
                m.StateProvinceName = (s != null) ? s.Name : "*";
                //zip
                m.Zip = (!String.IsNullOrEmpty(x.Zip)) ? x.Zip : "*";


                var htmlSb = new StringBuilder("<div>");
                htmlSb.AppendFormat("{0}: {1}", _localizationService.GetResource("Plugins.Shipping.DHL.Fields.To"), m.To);
                htmlSb.Append("<br />");
                htmlSb.AppendFormat("{0}: {1}", _localizationService.GetResource("Plugins.Shipping.DHL.Fields.Factor"), m.Factor);
                htmlSb.Append("</div>");
                m.DataHtml = htmlSb.ToString();

                return m;
            })
                .ToList();
            var gridModel = new DataSourceResult
            {
                Data = sbwModel,
                Total = records.TotalCount
            };

            return Json(gridModel);
        }

        [HttpPost]
        [AdminAntiForgery]
        public ActionResult RateDelete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings))
                return Content("Bu sayfaya erişim yetkiniz yoktur!");

            var sbw = _dhlService.GetById(id);
            if (sbw != null)
                _dhlService.DeleteDHLRecord(sbw);

            return new NullJsonResult();
        }

        public ActionResult AddPopup()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings))
                return Content("Bu sayfaya erişim yetkiniz yoktur!");

            var model = new DHLModel();
            model.DefaultCurrencyCode = _dhlSettings.CurrencyCode;
            model.From = 0;
            model.To = 1000000;

            var shippingMethods = _shippingService.GetAllShippingMethods();
            if (shippingMethods.Count == 0)
                return Content("Kargo methodları yüklenemedi.");

            //stores
            model.AvailableStores.Add(new SelectListItem { Text = "*", Value = "0" });
            foreach (var store in _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = store.Name, Value = store.Id.ToString() });
            //warehouses
            model.AvailableWarehouses.Add(new SelectListItem { Text = "*", Value = "0" });
            foreach (var warehouses in _shippingService.GetAllWarehouses())
                model.AvailableWarehouses.Add(new SelectListItem { Text = warehouses.Name, Value = warehouses.Id.ToString() });
            //shipping methods
            foreach (var sm in shippingMethods)
                model.AvailableShippingMethods.Add(new SelectListItem { Text = sm.Name, Value = sm.Id.ToString() });
            //countries
            model.AvailableCountries.Add(new SelectListItem { Text = "*", Value = "0" });
            var countries = _countryService.GetAllCountries(showHidden: true);
            foreach (var c in countries)
                model.AvailableCountries.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString() });
            //states
            model.AvailableStates.Add(new SelectListItem { Text = "*", Value = "0" });

            return View("~/Plugins/Shipping.DHL/Views/AddPopup.cshtml", model);
        }

        [HttpPost]
        [AdminAntiForgery]
        public ActionResult AddPopup(string btnId, string formId, DHLModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings))
                return Content("Bu sayfaya erişim yetkiniz yoktur!");

            var sbw = new DHLRecord
            {
                StoreId = model.StoreId,
                WarehouseId = model.WarehouseId,
                CountryId = model.CountryId,
                StateProvinceId = model.StateProvinceId,
                Zip = model.Zip == "*" ? null : model.Zip,
                ShippingMethodId = model.ShippingMethodId,
                From = model.From,
                To = model.To,
                Factor = model.Factor,
                IsFixedRate = model.IsFixedRate,
                FixedRate = model.FixedRate,
                FreeShippingOverXEnabled = model.FreeShippingOverXEnabled,
                FreeShippingOverXValue = model.FreeShippingOverXValue
            };

            _dhlService.InsertDHLRecord(sbw);

            ViewBag.RefreshPage = true;
            ViewBag.btnId = btnId;
            ViewBag.formId = formId;

            return View("~/Plugins/Shipping.DHL/Views/AddPopup.cshtml", model);
        }

        public ActionResult EditPopup(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings))
                return Content("Bu sayfaya erişim yetkiniz yoktur!");

            var sbw = _dhlService.GetById(id);
            if (sbw == null)
                //No record found with the specified id
                return RedirectToAction("Configure");

            var model = new DHLModel
            {
                Id = sbw.Id,
                StoreId = sbw.StoreId,
                WarehouseId = sbw.WarehouseId,
                CountryId = sbw.CountryId,
                StateProvinceId = sbw.StateProvinceId,
                Zip = sbw.Zip,
                ShippingMethodId = sbw.ShippingMethodId,
                From = sbw.From,
                To = sbw.To,
                Factor = sbw.Factor,
                DefaultCurrencyCode = _dhlSettings.CurrencyCode,
                IsFixedRate = sbw.IsFixedRate,
                FixedRate = sbw.FixedRate,
                FreeShippingOverXEnabled = sbw.FreeShippingOverXEnabled,
                FreeShippingOverXValue = sbw.FreeShippingOverXValue
            };

            var shippingMethods = _shippingService.GetAllShippingMethods();
            if (shippingMethods.Count == 0)
                return Content("Kargo methodları yüklenemedi.");

            var selectedStore = _storeService.GetStoreById(sbw.StoreId);
            var selectedWarehouse = _shippingService.GetWarehouseById(sbw.WarehouseId);
            var selectedShippingMethod = _shippingService.GetShippingMethodById(sbw.ShippingMethodId);
            var selectedCountry = _countryService.GetCountryById(sbw.CountryId);
            var selectedState = _stateProvinceService.GetStateProvinceById(sbw.StateProvinceId);
            //stores
            model.AvailableStores.Add(new SelectListItem { Text = "*", Value = "0" });
            foreach (var store in _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = store.Name, Value = store.Id.ToString(), Selected = (selectedStore != null && store.Id == selectedStore.Id) });
            //warehouses
            model.AvailableWarehouses.Add(new SelectListItem { Text = "*", Value = "0" });
            foreach (var warehouse in _shippingService.GetAllWarehouses())
                model.AvailableWarehouses.Add(new SelectListItem { Text = warehouse.Name, Value = warehouse.Id.ToString(), Selected = (selectedWarehouse != null && warehouse.Id == selectedWarehouse.Id) });
            //shipping methods
            foreach (var sm in shippingMethods)
                model.AvailableShippingMethods.Add(new SelectListItem { Text = sm.Name, Value = sm.Id.ToString(), Selected = (selectedShippingMethod != null && sm.Id == selectedShippingMethod.Id) });
            //countries
            model.AvailableCountries.Add(new SelectListItem { Text = "*", Value = "0" });
            var countries = _countryService.GetAllCountries(showHidden: true);
            foreach (var c in countries)
                model.AvailableCountries.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString(), Selected = (selectedCountry != null && c.Id == selectedCountry.Id) });
            //states
            var states = selectedCountry != null ? _stateProvinceService.GetStateProvincesByCountryId(selectedCountry.Id, showHidden: true).ToList() : new List<StateProvince>();
            model.AvailableStates.Add(new SelectListItem { Text = "*", Value = "0" });
            foreach (var s in states)
                model.AvailableStates.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString(), Selected = (selectedState != null && s.Id == selectedState.Id) });

            return View("~/Plugins/Shipping.DHL/Views/EditPopup.cshtml", model);
        }

        [HttpPost]
        [AdminAntiForgery]
        public ActionResult EditPopup(string btnId, string formId, DHLModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings))
                return Content("Bu sayfaya erişim yetkiniz yoktur!");

            var sbw = _dhlService.GetById(model.Id);
            if (sbw == null)
                //No record found with the specified id
                return RedirectToAction("Configure");

            sbw.StoreId = model.StoreId;
            sbw.WarehouseId = model.WarehouseId;
            sbw.CountryId = model.CountryId;
            sbw.StateProvinceId = model.StateProvinceId;
            sbw.Zip = model.Zip == "*" ? null : model.Zip;
            sbw.ShippingMethodId = model.ShippingMethodId;
            sbw.From = model.From;
            sbw.To = model.To;
            sbw.Factor = model.Factor;
            sbw.IsFixedRate = model.IsFixedRate;
            sbw.FixedRate = model.FixedRate;
            sbw.FreeShippingOverXEnabled = model.FreeShippingOverXEnabled;
            sbw.FreeShippingOverXValue = model.FreeShippingOverXValue;

            _dhlService.UpdateDHLRecord(sbw);

            ViewBag.RefreshPage = true;
            ViewBag.btnId = btnId;
            ViewBag.formId = formId;

            return View("~/Plugins/Shipping.DHL/Views/EditPopup.cshtml", model);
        }

    }
}
