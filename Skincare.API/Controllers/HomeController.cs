using Microsoft.AspNetCore.Mvc;

namespace Skincare.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HomeController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetHome()
        {
            return Ok("Welcome to the Skincare API!");
        }
    }
}
