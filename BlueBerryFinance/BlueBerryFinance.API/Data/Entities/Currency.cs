using BlueBerryFinance.API.Data.Entities.Enums;
using BlueBerryFinance.Common.DomainObjects;

namespace BlueBerryFinance.API.Data.Entities
{
    public class Currency : EntityBase
    {
        public CurrencyCode Code { get; set; }
        public string Symbol { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        public Currency() { }
    }
}
