using System;
using System.ComponentModel.DataAnnotations;
using FHP.Core.Models;
using FHP.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace FHP.Web.Pages
{
    public class UserGroupFormModel : SecurePageModel
    {
        private readonly IUserGroupRepository _groups;

        public UserGroupFormModel(IUserGroupRepository groups)
        {
            _groups = groups;
        }

        [BindProperty(SupportsGet = true)]
        public int? Id { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Mode { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "User group name is required.")]
        public string Name { get; set; }

        [BindProperty]
        public string Description { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Status is required.")]
        public string Status { get; set; } = UserStatuses.Active;

        public string Heading { get; private set; }
        public string ErrorMessage { get; private set; }

        public bool IsViewMode => string.Equals(Mode, "view", StringComparison.OrdinalIgnoreCase);
        public bool IsAddMode => !Id.HasValue;

        public IActionResult OnGet()
        {
            if (IsAddMode)
            {
                Heading = "Add User Group";
                return Page();
            }

            FHP.Core.Models.UserGroup group = _groups.GetById(Id.Value);
            if (group == null)
                return RedirectToPage("/UserGroup");

            Name = group.Name;
            Description = group.Description;
            Status = group.Status;

            Heading = IsViewMode ? "View User Group" : "Edit User Group";
            return Page();
        }

        public IActionResult OnPost()
        {
            Heading = IsAddMode ? "Add User Group" : "Edit User Group";

            if (!ModelState.IsValid)
                return Page();

            string name = Name.Trim();
            string description = (Description ?? string.Empty).Trim();

            if (_groups.ExistsByName(name, Id))
            {
                ErrorMessage = "A user group with that name already exists.";
                return Page();
            }

            if (IsAddMode)
            {
                _groups.Add(new FHP.Core.Models.UserGroup
                {
                    Name = name,
                    Description = description,
                    Status = Status,
                    LastUpdateBy = CurrentUser.Username
                });
            }
            else
            {
                FHP.Core.Models.UserGroup existing = _groups.GetById(Id.Value);
                if (existing == null)
                    return RedirectToPage("/UserGroup");

                existing.Name = name;
                existing.Description = description;
                existing.Status = Status;
                existing.LastUpdateBy = CurrentUser.Username;

                _groups.Update(existing);
            }

            return RedirectToPage("/UserGroup");
        }
    }
}
