using Microsoft.AspNetCore.Mvc;
using UserService.Application.DTOs;
using UserService.Application.Interfaces;
using UserService.Domain.Exceptions;

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
    }
}
