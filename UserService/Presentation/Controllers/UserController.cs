using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using UserService.Application.DTOs;
using UserService.Application.Interfaces;
using UserService.Application.Options;
using UserService.Application.Requests;

namespace UserService.Presentation.Controllers
{
    [ApiController]
    [Route("users")]
    public class UserController(IUserService userService) : Controller
    {
        [HttpGet("{id:int}")]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<UserDTO>(StatusCodes.Status200OK)]
        public async Task<ActionResult<UserDTO>> GetUser([FromRoute] int id)
        {
            return Ok(await userService.GetUserAsync(id));
        }

        [HttpPost]
        [ProducesResponseType<UserDTO>(StatusCodes.Status201Created)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
        [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UserDTO>> CreateUser([FromBody] CreateUserReq req,
            IOptions<UserCreationOptions> options)
        {
            UserDTO user = await userService.CreateUserAsync(req, options.Value);

            return CreatedAtAction(nameof(GetUser), new { id = user.UserId }, user);
        }
    }
}
