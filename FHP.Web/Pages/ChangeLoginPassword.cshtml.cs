using System.ComponentModel.DataAnnotations;
using FHP.Core.Models;
using FHP.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace FHP.Web.Pages
{
    public class ChangeLoginPasswordModel : SecurePageModel
    {
        private readonly IUserRepository _users;

        public ChangeLoginPasswordModel(IUserRepository users)
        {
            _users = users;
        }

        [BindProperty]
        [Required(ErrorMessage = "Current login password is required.")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "New login password is required.")]
        [MinLength(8, ErrorMessage = "New login password must be at least 8 characters.")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Please retype the new login password.")]
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

            User user = _users.GetById(CurrentUser.Id);
            if (user == null)
            {
                ErrorMessage = "Unable to load your account. Please log in again.";
                return Page();
            }

            if (!PasswordHasher.Verify(CurrentPassword, user.PasswordHash))
            {
                ErrorMessage = "Current login password is incorrect.";
                return Page();
            }

            user.PasswordHash = PasswordHasher.Hash(NewPassword);
            user.LastUpdateBy = CurrentUser.Username;
            _users.Update(user);

            SuccessMessage = "Your login password has been updated.";
            CurrentPassword = null;
            NewPassword = null;
            RetypePassword = null;
            ModelState.Clear();
            return Page();
        }
    }
}
