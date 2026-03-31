using Microsoft.Extensions.AI;

namespace BlueBerryFinance.API.Infrastructure.Utils.Agents.Tools.Interfaces
{
    /// <summary>
    /// Tool that classifies, validates, and queues income/expense entries for user approval.
    /// Exposes: save_income_expense, delete_income_expense.
    /// </summary>
    public interface IIncomeExpenseSaverTool
    {
        IList<AITool> GetTools();
    }
}
