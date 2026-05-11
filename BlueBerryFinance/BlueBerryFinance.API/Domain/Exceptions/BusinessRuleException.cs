namespace BlueBerryFinance.API.Domain.Exceptions
{
    public class BusinessRuleException : Exception
    {
        public IReadOnlyList<string> Errors { get; }

        public BusinessRuleException(string message)
            : base(message)
        {
            Errors = new List<string> { message };
        }

        public BusinessRuleException(IEnumerable<string> errors)
            : base(string.Join(", ", errors))
        {
            Errors = errors.ToList();
        }
    }
}
