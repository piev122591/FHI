using System;
using System.ComponentModel.DataAnnotations;
using FHP.Core.Models;
using FHP.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace FHP.Web.Pages
{
    public class UserFormModel : SecurePageModel
    {
        private readonly IUserRepository _users;

        public UserFormModel(IUserRepository users)
        {
            _users = users;
        }

        [BindProperty(SupportsGet = true)]
        public int? Id { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Mode { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Full name is required.")]
        public string FullName { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Username is required.")]
        public string Username { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; }

        [BindProperty]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [BindProperty]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }

        [BindProperty]
        public string Role { get; set; } = UserRoles.Admin;

        [BindProperty]
        public string Status { get; set; } = UserStatuses.Active;

        public string Heading { get; private set; }
        public string ErrorMessage { get; private set; }

        public bool IsViewMode => string.Equals(Mode, "view", StringComparison.OrdinalIgnoreCase);
        public bool IsAddMode => !Id.HasValue;

        public IActionResult OnGet()
        {
            if (IsAddMode)
            {
                Heading = "Add User";
                return Page();
            }

            User user = _users.GetById(Id.Value);
            if (user == null)
                return RedirectToPage("/Users");

            FullName = user.FullName;
            Username = user.Username;
            Email = user.Email;
            Role = user.Role;
            Status = user.Status;

            Heading = IsViewMode ? "View User" : "Edit User";
            return Page();
        }

        public IActionResult OnPost()
        {
            Heading = IsAddMode ? "Add User" : "Edit User";

            if (!ModelState.IsValid)
                return Page();

            string fullName = FullName.Trim();
            string username = Username.Trim();
            string email = Email.Trim();

            if (IsAddMode && string.IsNullOrEmpty(Password))
            {
                ErrorMessage = "Password is required for a new user.";
                return Page();
            }

            if (_users.ExistsByUsernameOrEmail(username, email, Id))
            {
                ErrorMessage = "A user with that username or email already exists.";
                return Page();
            }

            if (IsAddMode)
            {
                _users.Add(new User
                {
                    FullName = fullName,
                    Username = username,
                    Email = email,
                    PasswordHash = PasswordHasher.Hash(Password),
                    Role = Role,
                    Status = Status
                });
            }
            else
            {
                User existing = _users.GetById(Id.Value);
                if (existing == null)
                    return RedirectToPage("/Users");

                existing.FullName = fullName;
                existing.Username = username;
                existing.Email = email;
                existing.Role = Role;
                existing.Status = Status;

                if (!string.IsNullOrEmpty(Password))
                    existing.PasswordHash = PasswordHasher.Hash(Password);

                _users.Update(existing);
            }

            return RedirectToPage("/Users");
        }
    }
}
