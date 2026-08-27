using System;
using System.Collections.Generic;
using System.Linq;
using FHP.Core.Models;
using FHP.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace FHP.Web.Pages
{
    public class DashboardSetupModel : SecurePageModel
    {
        private readonly IDashboardGroupRepository _groups;

        public DashboardSetupModel(IDashboardGroupRepository groups)
        {
            _groups = groups;
        }

        [BindProperty(SupportsGet = true)]
        public string Name { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Description { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Status { get; set; } = UserStatuses.Active;

        public List<DashboardGroup> Results { get; private set; } = new List<DashboardGroup>();
        public string Message { get; private set; }

        public void OnGet()
        {
            BindResults();
        }

        public IActionResult OnPostDelete(int id)
        {
            _groups.Delete(id);
            Message = "Dashboard group deleted.";
            BindResults();
            return Page();
        }

        private void BindResults()
        {
            List<DashboardGroup> groups = _groups.GetAll();

            string nameKeyword = (Name ?? string.Empty).Trim();
            if (!string.IsNullOrEmpty(nameKeyword))
            {
                groups = groups.Where(g =>
                    g.Name.IndexOf(nameKeyword, StringComparison.OrdinalIgnoreCase) >= 0
                ).ToList();
            }

            string descriptionKeyword = (Description ?? string.Empty).Trim();
            if (!string.IsNullOrEmpty(descriptionKeyword))
            {
                groups = groups.Where(g =>
                    (g.Description ?? string.Empty).IndexOf(descriptionKeyword, StringComparison.OrdinalIgnoreCase) >= 0
                ).ToList();
            }

            if (!string.IsNullOrEmpty(Status))
                groups = groups.Where(g => string.Equals(g.Status, Status, StringComparison.OrdinalIgnoreCase)).ToList();

            Results = groups.OrderBy(g => g.Name, StringComparer.OrdinalIgnoreCase).ToList();
        }
    }
}
