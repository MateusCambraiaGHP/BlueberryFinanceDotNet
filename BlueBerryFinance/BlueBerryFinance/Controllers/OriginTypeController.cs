using BlueBerryFinance.API.Data.Entities;
using Microsoft.AspNetCore.Mvc;

namespace BlueBerryFinance.API.Controllers
{
    [Route("origin-type/")]
    public class OriginTypeController : Controller
    {
        public ActionResult<List<OriginType>> Get()
        {
            return new List<OriginType>();
        }
    }
}
