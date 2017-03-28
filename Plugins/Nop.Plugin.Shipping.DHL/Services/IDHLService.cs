using Nop.Core;
using Nop.Plugin.Shipping.DHL.Domain;

namespace Nop.Plugin.Shipping.DHL.Services
{
    public partial interface IDHLService
    {
        void DeleteDHLRecord(DHLRecord dhlRecord);
        IPagedList<DHLRecord> GetAll(int pageIndex = 0, int pageSize = int.MaxValue);
        DHLRecord FindRecord(int shippingMethodId,
             int storeId, int warehouseId,
             int countryId, int stateProvinceId, string zip, decimal weight);
        DHLRecord GetById(int dhlRecordId);
        void InsertDHLRecord(DHLRecord dhlRecord);
        void UpdateDHLRecord(DHLRecord dhlRecord);
    }
}
