using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppConfgDocumentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize]
    public class Controller_Base : ControllerBase
    {
    }
}