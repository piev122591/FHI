using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using FHP.Core.Models;
using FHP.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FHP.Web.Pages
{
    public class UserSetupFormModel : SecurePageModel
    {
        private readonly IUserRepository _users;
        private readonly IUserGroupRepository _userGroups;

        public UserSetupFormModel(IUserRepository users, IUserGroupRepository userGroups)
        {
            _users = users;
            _userGroups = userGroups;
        }

        [BindProperty(SupportsGet = true)]
        public int? Id { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Mode { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "User ID is required.")]
        public string UserId { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Staff name is required.")]
        public string StaffName { get; set; }

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
        [Required(ErrorMessage = "User group is required.")]
        public string UserGroup { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Status is required.")]
        public string Status { get; set; } = UserStatuses.Active;

        public string Heading { get; private set; }
        public string ErrorMessage { get; private set; }

        public bool IsViewMode => string.Equals(Mode, "view", StringComparison.OrdinalIgnoreCase);
        public bool IsAddMode => !Id.HasValue;

        public System.Collections.Generic.List<SelectListItem> UserGroupOptions => _userGroups.GetAll()
            .OrderBy(g => g.Name, StringComparer.OrdinalIgnoreCase)
            .Select(g => new SelectListItem(g.Name, g.Name))
            .ToList();

        public IActionResult OnGet()
        {
            if (IsAddMode)
            {
                Heading = "Add User";
                return Page();
            }

            User user = _users.GetById(Id.Value);
            if (user == null)
                return RedirectToPage("/UserSetup");

            UserId = user.Username;
            StaffName = user.FullName;
            Email = user.Email;
            UserGroup = user.UserGroup;
            Status = user.Status;

            Heading = IsViewMode ? "View User" : "Edit User";
            return Page();
        }

        public IActionResult OnPost()
        {
            Heading = IsAddMode ? "Add User" : "Edit User";

            if (!ModelState.IsValid)
                return Page();

            string userId = UserId.Trim();
            string staffName = StaffName.Trim();
            string email = Email.Trim();

            if (IsAddMode && string.IsNullOrEmpty(Password))
            {
                ErrorMessage = "Password is required for a new user.";
                return Page();
            }

            if (_users.ExistsByUsernameOrEmail(userId, email, Id))
            {
                ErrorMessage = "A user with that User ID or email already exists.";
                return Page();
            }

            if (IsAddMode)
            {
                _users.Add(new User
                {
                    FullName = staffName,
                    Username = userId,
                    Email = email,
                    PasswordHash = PasswordHasher.Hash(Password),
                    UserGroup = UserGroup,
                    Status = Status,
                    LastUpdateBy = CurrentUser.Username
                });
            }
            else
            {
                User existing = _users.GetById(Id.Value);
                if (existing == null)
                    return RedirectToPage("/UserSetup");

                existing.FullName = staffName;
                existing.Username = userId;
                existing.Email = email;
                existing.UserGroup = UserGroup;
                existing.Status = Status;
                existing.LastUpdateBy = CurrentUser.Username;

                if (!string.IsNullOrEmpty(Password))
                    existing.PasswordHash = PasswordHasher.Hash(Password);

                _users.Update(existing);
            }

            return RedirectToPage("/UserSetup");
        }
    }
}
