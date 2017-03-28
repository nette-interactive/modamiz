using Nop.Core.Configuration;

namespace Nop.Plugin.Shipping.DHL
{
    public class DHLSettings : ISettings
    {
        public bool LimitMethodsToCreated { get; set; }
        public decimal VolumetricDivisor { get; set; }
        public string  CurrencyCode { get; set; }
    }
}