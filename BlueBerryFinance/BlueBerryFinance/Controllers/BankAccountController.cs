using BlueBerryFinance.API.Data.Entities;
using Microsoft.AspNetCore.Mvc;

namespace BlueBerryFinance.API.Controllers
{

    [Route("bank-account/")]
    public class BankAccountController : BaseController
    {
        public ActionResult<List<BankAccount>> Get()
        {
            return new List<BankAccount>();
        }
    }
}
