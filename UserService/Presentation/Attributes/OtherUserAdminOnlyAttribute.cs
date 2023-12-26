using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using UserService.Domain.Exceptions;

namespace UserService.Presentation.Attributes
{
    /// <summary>
    /// Ensures that only users with admin privileges can affect other users
    /// </summary>
    public class OtherUserAdminOnlyAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var identity = (ClaimsIdentity)(context.HttpContext.User.Identity ??
                throw new Exception("User identity is null"));

            string adminClaim = identity.FindFirst("admin")?.Value ??
                throw new Exception("No admin claim value");

            string subIdClaim = identity.FindFirst("sub_id")?.Value ??
                throw new Exception("No subject id claim value");

            var isAdmin = bool.Parse(adminClaim);
            var subjectId = int.Parse(subIdClaim);
            int affectedUserId = context.ActionArguments["id"] as int? ??
                throw new Exception("No id parameter");

            if (subjectId != affectedUserId && !isAdmin)
            {
                throw new OtherUserAdminOnlyException();
            }
        }
    }
}
