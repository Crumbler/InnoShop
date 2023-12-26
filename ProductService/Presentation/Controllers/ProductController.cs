using Microsoft.AspNetCore.Mvc;
using ProductService.Application.Interfaces;
using ProductService.Application.Requests;
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

        [HttpGet("categories")]
        [ProducesResponseType<Category[]>(StatusCodes.Status200OK)]
        public Task<Category[]> GetCategories()
        {
            return productService.GetCategoriesAsync();
        }

        [HttpGet]
        [ProducesResponseType<Product[]>(StatusCodes.Status200OK)]
        [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
        public Task<Product[]> GetProducts([FromQuery] GetProductsReq req)
        {
            return productService.GetProductsAsync(req);
        }
    }
}
