namespace BlueBerryFinance.Common.ViewModels
{
    public class CurrencyViewModel
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Symbol { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        public CurrencyViewModel(
            Guid id,
            string code,
            string symbol,
            string name)
        {
            Id = id;
            Code = code;
            Symbol = symbol;
            Name = name;
        }
    }
}
