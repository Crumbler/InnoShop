﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.DTOs;
using UserService.Application.Interfaces;
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
        public Task<UserDTO> GetUser([FromRoute] int id)
        {
            return userService.GetUserAsync(id);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<UserDTO>> CreateUser([FromBody] CreateUserReq req)
        {
            UserDTO user = await userService.CreateUserAsync(req);

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
        public Task<LoginDTO> Login([FromBody] LoginReq req)
        {
            return userService.LoginAsync(req);
        }

        [HttpDelete("/logout")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public NoContentResult Logout()
        {
            Response.Headers.Remove("Authorization");

            return NoContent();
        }

        [HttpPost("/confirm/{token}", Name = "ConfirmEmail")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
        public async Task<NoContentResult> ConfirmEmail([FromRoute] string token)
        {
            await userService.ConfirmUserAsync(token);

            return NoContent();
        }

        [HttpPost("/forgotpassword")]
        [ProducesResponseType<string>(StatusCodes.Status200OK)]
        [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
        public async Task<string> ForgotPassword([FromBody] ForgotPasswordReq req)
        {
            await userService.ForgotPasswordAsync(req);

            return "If a user with the specified email exists, instructions on how to reset the password will have been sent.";
        }

        [HttpPost("/resetpassword/{token}", Name = "ResetPassword")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
        public async Task<NoContentResult> ResetPassword([FromRoute] string token, [FromBody] ResetPasswordReq req)
        {
            await userService.ResetPasswordAsync(token, req);

            return NoContent();
        }
    }
}
