using System;
using System.ComponentModel.DataAnnotations;
using FHP.Core.Models;
using FHP.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace FHP.Web.Pages
{
    public class DashboardSetupFormModel : SecurePageModel
    {
        private readonly IDashboardGroupRepository _groups;

        public DashboardSetupFormModel(IDashboardGroupRepository groups)
        {
            _groups = groups;
        }

        [BindProperty(SupportsGet = true)]
        public int? Id { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Mode { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Dashboard group name is required.")]
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
                Heading = "Add Dashboard Group";
                return Page();
            }

            DashboardGroup group = _groups.GetById(Id.Value);
            if (group == null)
                return RedirectToPage("/DashboardSetup");

            Name = group.Name;
            Description = group.Description;
            Status = group.Status;

            Heading = IsViewMode ? "View Dashboard Group" : "Edit Dashboard Group";
            return Page();
        }

        public IActionResult OnPost()
        {
            Heading = IsAddMode ? "Add Dashboard Group" : "Edit Dashboard Group";

            if (!ModelState.IsValid)
                return Page();

            string name = Name.Trim();
            string description = (Description ?? string.Empty).Trim();

            if (_groups.ExistsByName(name, Id))
            {
                ErrorMessage = "A dashboard group with that name already exists.";
                return Page();
            }

            if (IsAddMode)
            {
                _groups.Add(new DashboardGroup
                {
                    Name = name,
                    Description = description,
                    Status = Status,
                    LastUpdateBy = CurrentUser.Username
                });
            }
            else
            {
                DashboardGroup existing = _groups.GetById(Id.Value);
                if (existing == null)
                    return RedirectToPage("/DashboardSetup");

                existing.Name = name;
                existing.Description = description;
                existing.Status = Status;
                existing.LastUpdateBy = CurrentUser.Username;

                _groups.Update(existing);
            }

            return RedirectToPage("/DashboardSetup");
        }
    }
}
