using BlueBerryFinance.API.Data.Entities;
using Microsoft.AspNetCore.Mvc;

namespace BlueBerryFinance.API.Controllers
{
    [Route("fixed-expense/")]
    public class FixedExpenseController : Controller
    {
        public ActionResult<List<FixedExpense>> Get()
        {
            return new List<FixedExpense>();
        }
    }
}
