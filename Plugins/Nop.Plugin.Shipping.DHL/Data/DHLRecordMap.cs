using Nop.Data.Mapping;
using Nop.Plugin.Shipping.DHL.Domain;

namespace Nop.Plugin.Shipping.DHL.Data
{
    public partial class DHLRecordMap : NopEntityTypeConfiguration<DHLRecord>
    {
        public DHLRecordMap()
        {
            this.ToTable("ShippingByDHL");
            this.HasKey(x => x.Id);

            this.Property(x => x.Zip).HasMaxLength(400);
        }
    }
}