using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.Interfaces;
using ProductService.Application.Requests;
using ProductService.Domain.Entities;
using ProductService.Presentation.Options;
using System.Security.Claims;

namespace ProductService.Presentation.Controllers
{
    [ApiController]
    [Route("products")]
    public class ProductController(IProductService productService, JwtOptions jwtOptions) : ControllerBase
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

        [Authorize]
        [HttpPost]
        [ProducesResponseType<Product>(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<Product>> CreateProduct([FromBody] CreateProductReq req)
        {
            (int userId, _) = GetUserClaims();

            Product p = await productService.CreateProductAsync(userId, req);

            return CreatedAtAction(nameof(GetProduct), new { id = p.ProductId }, p);
        }

        [Authorize]
        [HttpPut("{productId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public async Task<NoContentResult> UpdateProduct([FromRoute] int productId, 
            [FromBody] UpdateProductReq req)
        {
            (int userId, bool isAdmin) = GetUserClaims();

            await productService.UpdateProductAsync(userId, isAdmin, productId, req);

            return NoContent();
        }

        [Authorize]
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public async Task<NoContentResult> DeleteProduct([FromRoute] int id)
        {
            (int userId, bool isAdmin) = GetUserClaims();

            await productService.DeleteProductAsync(userId, isAdmin, id);

            return NoContent();
        }

        [NonAction]
        private (int userId, bool isAdmin) GetUserClaims()
        {
            var identity = (ClaimsIdentity)(User.Identity ??
                throw new Exception("User identity is null"));

            string adminClaim = identity.FindFirst(jwtOptions.IsAdminClaimType)?.Value ??
                throw new Exception("No admin claim value");

            string subIdClaim = identity.FindFirst(jwtOptions.UserIdClaimType)?.Value ??
                throw new Exception("No subject id claim value");

            var isAdmin = bool.Parse(adminClaim);
            var subjectId = int.Parse(subIdClaim);

            return (subjectId, isAdmin);
        }
    }
}
