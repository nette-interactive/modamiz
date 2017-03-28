using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Plugin.Shipping.DHL.Domain;
using Nop.Services.Logging;
using Nop.Core.Domain.Logging;

namespace Nop.Plugin.Shipping.DHL.Services
{
    public partial class DHLService : IDHLService
    {
        #region Constants
        private const string DHL_ALL_KEY = "Nop.dhl.all-{0}-{1}";
        private const string DHL_PATTERN_KEY = "Nop.dhl.";
        #endregion

        #region Fields

        private readonly IRepository<DHLRecord> _sbwRepository;
        private readonly ICacheManager _cacheManager;
        private readonly ILogger _logger;

        #endregion

        #region Ctor

        public DHLService(ICacheManager cacheManager,
            IRepository<DHLRecord> sbwRepository,
            ILogger logger)
        {
            this._cacheManager = cacheManager;
            this._sbwRepository = sbwRepository;
            this._logger = logger;
        }

        #endregion

        #region Methods

        public virtual void DeleteDHLRecord(DHLRecord dhlRecord)
        {
            if (dhlRecord == null)
                throw new ArgumentNullException("dhlRecord");

            _sbwRepository.Delete(dhlRecord);
            _cacheManager.RemoveByPattern(DHL_PATTERN_KEY);
        }

        public virtual IPagedList<DHLRecord> GetAll(int pageIndex = 0, int pageSize = int.MaxValue)
        {
            string key = string.Format(DHL_ALL_KEY, pageIndex, pageSize);
            return _cacheManager.Get(key, () =>
            {
                var query = from sbw in _sbwRepository.Table
                            orderby sbw.StoreId, sbw.CountryId, sbw.StateProvinceId, sbw.Zip, sbw.ShippingMethodId, sbw.From
                            select sbw;

                var records = new PagedList<DHLRecord>(query, pageIndex, pageSize);
                return records;
            });
        }

        public virtual DHLRecord FindRecord(int shippingMethodId,
            int storeId, int warehouseId, 
            int countryId, int stateProvinceId, string zip, decimal weight)
        {
            if (zip == null)
                zip = string.Empty;
            zip = zip.Trim();

            //filter by weight and shipping method
            var existingRates = GetAll()
                .Where(sbw => sbw.ShippingMethodId == shippingMethodId && weight > sbw.From && weight <= sbw.To)
                .ToList();

            //filter by store
            var matchedByStore = new List<DHLRecord>();
            foreach (var sbw in existingRates)
                if (storeId == sbw.StoreId)
                    matchedByStore.Add(sbw);
            if (matchedByStore.Count == 0)
                foreach (var sbw in existingRates)
                    if (sbw.StoreId == 0)
                        matchedByStore.Add(sbw);

            //filter by warehouse
            var matchedByWarehouse = new List<DHLRecord>();
            foreach (var sbw in matchedByStore)
                if (warehouseId == sbw.WarehouseId)
                    matchedByWarehouse.Add(sbw);
            if (matchedByWarehouse.Count == 0)
                foreach (var sbw in matchedByStore)
                    if (sbw.WarehouseId == 0)
                        matchedByWarehouse.Add(sbw);

            //filter by country
            var matchedByCountry = new List<DHLRecord>();
            foreach (var sbw in matchedByWarehouse)
                if (countryId == sbw.CountryId)
                    matchedByCountry.Add(sbw);
            if (matchedByCountry.Count == 0)
                foreach (var sbw in matchedByWarehouse)
                    if (sbw.CountryId == 0)
                        matchedByCountry.Add(sbw);

            //filter by state/province
            var matchedByStateProvince = new List<DHLRecord>();
            foreach (var sbw in matchedByCountry)
                if (stateProvinceId == sbw.StateProvinceId)
                    matchedByStateProvince.Add(sbw);
            if (matchedByStateProvince.Count == 0)
                foreach (var sbw in matchedByCountry)
                    if (sbw.StateProvinceId == 0)
                        matchedByStateProvince.Add(sbw);

            //filter by zip
            var matchedByZip = new List<DHLRecord>();
            foreach (var sbw in matchedByStateProvince)
                if ((String.IsNullOrEmpty(zip) && String.IsNullOrEmpty(sbw.Zip)) ||
                    (zip.Equals(sbw.Zip, StringComparison.InvariantCultureIgnoreCase)))
                    matchedByZip.Add(sbw);

            if (matchedByZip.Count == 0)
                foreach (var taxRate in matchedByStateProvince)
                    if (String.IsNullOrWhiteSpace(taxRate.Zip))
                        matchedByZip.Add(taxRate);

            return matchedByZip.FirstOrDefault();
        }

        public virtual DHLRecord GetById(int dhlRecordId)
        {
            if (dhlRecordId == 0)
                return null;

            return _sbwRepository.GetById(dhlRecordId);
        }

        public virtual void InsertDHLRecord(DHLRecord dhlRecord)
        {
            if (dhlRecord == null)
                throw new ArgumentNullException("dhlRecord");

            _sbwRepository.Insert(dhlRecord);
            _cacheManager.RemoveByPattern(DHL_PATTERN_KEY);
        }

        public virtual void UpdateDHLRecord(DHLRecord dhlRecord)
        {
            if (dhlRecord == null)
                throw new ArgumentNullException("dhlRecord");

            _sbwRepository.Update(dhlRecord);
            _cacheManager.RemoveByPattern(DHL_PATTERN_KEY);
        }

        #endregion
    }
}
