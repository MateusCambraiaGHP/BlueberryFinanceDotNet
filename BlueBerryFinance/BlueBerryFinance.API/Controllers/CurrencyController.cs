using BlueBerryFinance.API.Data.Entities;
using BlueBerryFinance.API.Data.Entities.enums;
using Microsoft.AspNetCore.Mvc;

namespace BlueBerryFinance.API.Controllers
{
    [Route("currency/")]
    public class CurrencyController : Controller
    {
        public CurrencyController() { }

        [HttpGet]
        public ActionResult<List<Currency>> Get()
        {
            return new List<Currency>();
        }
    }
}
