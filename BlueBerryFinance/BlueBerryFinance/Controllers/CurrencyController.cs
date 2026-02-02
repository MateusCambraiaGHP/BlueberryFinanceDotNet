using BlueBerryFinance.API.Data.Entities;
using Microsoft.AspNetCore.Mvc;

namespace BlueBerryFinance.API.Controllers
{
    public class CurrencyController : Controller
    {
        [Route("currency/")]
        public ActionResult<List<Currency>> Get()
        {
            return new List<Currency>();
        }
    }
}
