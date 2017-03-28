using Nop.Core;

namespace Nop.Plugin.Shipping.DHL.Domain
{
    /// <summary>
    /// Represents a shipping by weight record
    /// </summary>
    public partial class DHLRecord : BaseEntity
    {
        /// <summary>
        /// Gets or sets the store identifier
        /// </summary>
        public int StoreId { get; set; }

        /// <summary>
        /// Gets or sets the warehouse identifier
        /// </summary>
        public int WarehouseId { get; set; }

        /// <summary>
        /// Gets or sets the country identifier
        /// </summary>
        public int CountryId { get; set; }

        /// <summary>
        /// Gets or sets the state/province identifier
        /// </summary>
        public int StateProvinceId { get; set; }

        /// <summary>
        /// Gets or sets the zip
        /// </summary>
        public string Zip { get; set; }

        /// <summary>
        /// Gets or sets the shipping method identifier
        /// </summary>
        public int ShippingMethodId { get; set; }

        /// <summary>
        /// Gets or sets the "from" value
        /// </summary>
        public decimal From { get; set; }

        /// <summary>
        /// Gets or sets the "to" value
        /// </summary>
        public decimal To { get; set; }

        public decimal Factor { get; set; }

        public bool IsFixedRate { get; set; }

        public decimal FixedRate { get; set; }

        public bool FreeShippingOverXEnabled { get; set; }
        public decimal FreeShippingOverXValue { get; set; }

    }
}