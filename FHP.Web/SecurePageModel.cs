using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FHP.Web
{
    /// <summary>
    /// Base class for every page that requires a logged-in user (Dashboard, Users,
    /// UserForm). [Authorize] here is enforced framework-wide before OnGet/OnPost
    /// ever runs — equivalent to the classic Web.config &lt;location&gt; deny rules.
    /// </summary>
    [Authorize]
    public abstract class SecurePageModel : PageModel
    {
        protected AuthenticatedUser CurrentUser => new AuthenticatedUser
        {
            Id = int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int id) ? id : 0,
            FullName = User.FindFirst("FullName")?.Value ?? string.Empty,
            Username = User.Identity?.Name ?? string.Empty,
            Role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty
        };
    }
}
