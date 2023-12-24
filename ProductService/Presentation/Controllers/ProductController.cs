using Microsoft.AspNetCore.Mvc;

namespace ProductService.Presentation.Controllers
{
    [ApiController]
    [Route("products")]
    public class ProductController : ControllerBase
    {
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public void GetUser([FromRoute] int id)
        {

        }
    }
}
