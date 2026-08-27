using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using FHP.Core.Models;
using FHP.Core.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FHP.Web.Pages
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly IUserRepository _users;

        public LoginModel(IUserRepository users)
        {
            _users = users;
        }

        [BindProperty]
        [Required(ErrorMessage = "Username or email is required.")]
        public string Username { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [BindProperty]
        public bool RememberMe { get; set; }

        public string ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToPage("/Dashboard");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            User user = _users.GetByUsernameOrEmail(Username.Trim());

            // Generic message on every failure path — never reveal whether the
            // username/email exists or the password was wrong.
            const string genericError = "Invalid username/email or password.";

            if (user == null || !PasswordHasher.Verify(Password, user.PasswordHash))
            {
                ErrorMessage = genericError;
                return Page();
            }

            if (!string.Equals(user.Status, UserStatuses.Active, StringComparison.OrdinalIgnoreCase))
            {
                ErrorMessage = "This account is inactive. Contact an administrator.";
                return Page();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("FullName", user.FullName),
                new Claim(ClaimTypes.Role, user.Role)
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties { IsPersistent = RememberMe });

            return RedirectToPage("/Dashboard");
        }
    }
}
