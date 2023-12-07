using Microsoft.AspNetCore.Mvc;
using UserService.Application.DTOs;
using UserService.Application.Interfaces;
using UserService.Domain.Exceptions;

namespace UserService.Presentation.Controllers
{
    [ApiController]
    [Route("Users")]
    public class UserController(IUserService userService) : Controller
    {
        [HttpGet("{id:int}")]
        [ProducesResponseType<string>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<UserDTO>(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetUser([FromRoute] int id)
        {
            try
            {
                return Ok(await userService.GetUserAsync(id));
            }
            catch (UserNotFoundException e)
            {
                return NotFound(e.Message);
            }
        }
    }
}
