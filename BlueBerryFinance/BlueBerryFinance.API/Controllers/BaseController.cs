using Microsoft.AspNetCore.Mvc;

namespace BlueBerryFinance.API.Controllers
{
    [ApiController]
    [Route("api/v1.0/")]
    public abstract class BaseController : ControllerBase
    {
        protected BaseController() { }
    }
}
