using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using UserService.Application.DTOs;
using UserService.Application.Interfaces;
using UserService.Application.Options;
using UserService.Application.Requests;
using UserService.Presentation.Attributes;

namespace UserService.Presentation.Controllers
{
    [ApiController]
    [Route("users")]
    public class UserController(IUserService userService) : ControllerBase
    {
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public async Task<UserDTO> GetUser([FromRoute] int id)
        {
            return await userService.GetUserAsync(id);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<UserDTO>> CreateUser([FromBody] CreateUserReq req,
            IOptions<UserCreationOptions> options)
        {
            UserDTO user = await userService.CreateUserAsync(req, options.Value);

            return CreatedAtAction(nameof(GetUser), new { id = user.UserId }, user);
        }

        [Authorize, OtherUserAdminOnly]
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public async Task<NoContentResult> DeleteUser([FromRoute] int id)
        {
            await userService.DeleteUserAsync(id);
            return NoContent();
        }

        [Authorize, OtherUserAdminOnly]
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
        public async Task<NoContentResult> UpdateUser([FromRoute] int id,
            [FromBody] UpdateUserReq req)
        {
            await userService.UpdateUserAsync(id, req);
            return NoContent();
        }

        [HttpPost("/login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
        public async Task<LoginDTO> Login([FromBody] LoginReq req,
            [FromServices] RsaSecurityKey key,
            [FromServices] JwtSecurityTokenHandler handler)
        {
            return await userService.Login(req, key, handler);
        }

        [HttpDelete("/logout")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public NoContentResult Logout()
        {
            Response.Headers.Remove("Authorization");

            return NoContent();
        }
    }
}
