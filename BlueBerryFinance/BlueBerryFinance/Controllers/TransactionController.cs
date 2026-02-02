using Microsoft.AspNetCore.Mvc;
using System.Transactions;

namespace BlueBerryFinance.API.Controllers
{
    [Route("transaction/")]
    public class TransactionController : Controller
    {
        public ActionResult<List<Transaction>> Get()
        {
            return new List<Transaction>();
        }
    }
}
