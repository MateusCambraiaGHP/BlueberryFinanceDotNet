using BlueBerryFinance.API.Data.Entities;
using BlueBerryFinance.API.Data.Entities.enums;
using Microsoft.AspNetCore.Mvc;

namespace BlueBerryFinance.API.Controllers
{
    [Route("origin-type/")]
    public class OriginTypeController : Controller
    {
        public OriginTypeController() { }

        [HttpGet]
        public ActionResult<List<OriginType>> Get()
        {
            return new List<OriginType>();
        }
    }
}
