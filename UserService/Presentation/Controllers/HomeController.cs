using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace UserService.Presentation.Controllers
{
    [ApiController]
    [Route("/[action]")]
    public class HomeController : Controller
    {
        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public string Index()
        {
            return "Hello World";
        }
    }
}
