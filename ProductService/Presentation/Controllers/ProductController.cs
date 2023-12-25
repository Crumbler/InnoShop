using Microsoft.AspNetCore.Mvc;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;

namespace ProductService.Presentation.Controllers
{
    [ApiController]
    [Route("products")]
    public class ProductController(IProductService productService) : ControllerBase
    {
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public Task<Product> GetProduct([FromRoute] int id)
        {
            return productService.GetProductAsync(id);
        }
    }
}
