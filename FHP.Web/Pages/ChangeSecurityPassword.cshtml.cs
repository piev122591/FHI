using System.ComponentModel.DataAnnotations;
using FHP.Core.Models;
using FHP.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace FHP.Web.Pages
{
    public class ChangeSecurityPasswordModel : SecurePageModel
    {
        private readonly IUserRepository _users;

        public ChangeSecurityPasswordModel(IUserRepository users)
        {
            _users = users;
        }

        [BindProperty]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

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
            User user = _users.GetById(CurrentUser.Id);
            if (user == null)
            {
                ErrorMessage = "Unable to load your account. Please log in again.";
                return Page();
            }

            bool hasExistingSecurityPassword = !string.IsNullOrEmpty(user.SecurityPasswordHash);
            if (hasExistingSecurityPassword && string.IsNullOrEmpty(CurrentPassword))
            {
                ModelState.AddModelError(nameof(CurrentPassword), "Current security password is required.");
            }

            if (!ModelState.IsValid)
                return Page();

            if (hasExistingSecurityPassword && !PasswordHasher.Verify(CurrentPassword, user.SecurityPasswordHash))
            {
                ErrorMessage = "Current security password is incorrect.";
                return Page();
            }

            user.SecurityPasswordHash = PasswordHasher.Hash(NewPassword);
            user.LastUpdateBy = CurrentUser.Username;
            _users.Update(user);

            SuccessMessage = "Your security password has been updated.";
            CurrentPassword = null;
            NewPassword = null;
            RetypePassword = null;
            ModelState.Clear();
            return Page();
        }
    }
}
