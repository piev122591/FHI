using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FHP.Web.Pages
{
    public class IndexModel : PageModel
    {
        public IActionResult OnGet()
        {
            return RedirectToPage(User.Identity?.IsAuthenticated == true ? "/Dashboard" : "/Login");
        }
    }
}
