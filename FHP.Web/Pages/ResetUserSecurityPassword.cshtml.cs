using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using FHP.Core.Models;
using FHP.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace FHP.Web.Pages
{
    public class ResetUserSecurityPasswordModel : SecurePageModel
    {
        private readonly IUserRepository _users;

        public ResetUserSecurityPasswordModel(IUserRepository users)
        {
            _users = users;
        }

        [BindProperty]
        [Required(ErrorMessage = "User ID is required.")]
        public string UserId { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "New security password is required.")]
        [MinLength(8, ErrorMessage = "New security password must be at least 8 characters.")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Please retype the new security password.")]
        [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
        [DataType(DataType.Password)]
        public string RetypePassword { get; set; }

        public string ErrorMessage { get; private set; }
        public string SuccessMessage { get; private set; }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
                return Page();

            string userId = UserId.Trim();
            User user = _users.GetAll()
                .FirstOrDefault(u => string.Equals(u.Username, userId, StringComparison.OrdinalIgnoreCase));

            if (user == null)
            {
                ErrorMessage = $"No user found with User ID \"{userId}\".";
                return Page();
            }

            user.SecurityPasswordHash = PasswordHasher.Hash(NewPassword);
            user.LastUpdateBy = CurrentUser.Username;
            _users.Update(user);

            SuccessMessage = $"Security password for \"{user.Username}\" has been reset.";
            UserId = null;
            NewPassword = null;
            RetypePassword = null;
            ModelState.Clear();
            return Page();
        }
    }
}
