using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using yazlab1proje3webapi.Tools;

namespace yazlab1proje3webapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenCreateController : ControllerBase
    {
        [HttpPost]
        public IActionResult CreateToken(GetCheckAppUserViewModel model)
        {
            var values = JwtTokenGenerator.GenerateToken(model);
            return Ok(values);  
        }
    }
}
