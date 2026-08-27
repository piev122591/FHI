using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using FHP.Core.Models;
using FHP.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace FHP.Web.Pages
{
    // Passwords are stored as one-way PBKDF2 hashes (see PasswordHasher) and cannot be
    // decrypted or displayed, so this screen — despite its name — never reveals a real
    // password. It re-authenticates the caller with their own security password, looks
    // up the target user, and always returns a safe, non-recoverable result pointing to
    // Reset User Login Password instead.
    public class ViewUserPasswordModel : SecurePageModel
    {
        private readonly IUserRepository _users;

        public ViewUserPasswordModel(IUserRepository users)
        {
            _users = users;
        }

        [BindProperty]
        public string UserId { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Security password is required.")]
        [DataType(DataType.Password)]
        public string SecurityPassword { get; set; }

        public string ErrorMessage { get; private set; }
        public bool HasSearched { get; private set; }
        public string ResultMessage { get; private set; }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
                return Page();

            User currentUser = _users.GetById(CurrentUser.Id);
            if (currentUser == null || string.IsNullOrEmpty(currentUser.SecurityPasswordHash))
            {
                ErrorMessage = "You must set a security password before using this feature. Go to Change Security Password.";
                return Page();
            }

            if (!PasswordHasher.Verify(SecurityPassword, currentUser.SecurityPasswordHash))
            {
                ErrorMessage = "Incorrect security password.";
                return Page();
            }

            string userId = (UserId ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(userId))
            {
                ErrorMessage = "Enter a User ID to search.";
                return Page();
            }

            User target = _users.GetAll()
                .FirstOrDefault(u => string.Equals(u.Username, userId, StringComparison.OrdinalIgnoreCase));

            HasSearched = true;
            SecurityPassword = null;

            if (target == null)
            {
                ResultMessage = $"No user found with User ID \"{userId}\".";
                return Page();
            }

            ResultMessage = "Passwords are stored securely and cannot be displayed. Use Reset User Login Password to set a new one for this user.";
            return Page();
        }
    }
}
