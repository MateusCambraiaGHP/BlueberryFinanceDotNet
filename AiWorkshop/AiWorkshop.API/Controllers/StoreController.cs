using BlueBerryFinance.API.Data.Entities;
using Microsoft.AspNetCore.Mvc;

namespace BlueBerryFinance.API.Controllers
{
    [Route("store/")]
    public class StoreController : Controller
    {
        public StoreController() { }

        [HttpGet]
        public ActionResult<List<Store>> Get()
        {
            return new List<Store>();
        }
    }
}
