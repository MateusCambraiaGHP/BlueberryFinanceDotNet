namespace BlueBerryFinance.API.Infrastructure.Messaging
{
    public static class QueueNames
    {
        public const string MonthlyReport  = "finance.report.monthly";
        public const string BankSync       = "finance.bank.sync";
        public const string FiscalNote     = "finance.fiscal.note";
        public const string Notification   = "finance.notification";
    }
}
