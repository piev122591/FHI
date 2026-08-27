using System;
using System.Collections.Generic;
using System.Linq;
using FHP.Core.Models;
using FHP.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FHP.Web.Pages
{
    public class UserSetupModel : SecurePageModel
    {
        private readonly IUserRepository _users;
        private readonly IUserGroupRepository _userGroups;

        public UserSetupModel(IUserRepository users, IUserGroupRepository userGroups)
        {
            _users = users;
            _userGroups = userGroups;
        }

        [BindProperty(SupportsGet = true)]
        public string UserId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string StaffName { get; set; }

        [BindProperty(SupportsGet = true)]
        public string UserGroup { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Status { get; set; } = UserStatuses.Active;

        public List<User> Results { get; private set; } = new List<User>();
        public string Message { get; private set; }

        public List<SelectListItem> UserGroupOptions => _userGroups.GetAll()
            .OrderBy(g => g.Name, StringComparer.OrdinalIgnoreCase)
            .Select(g => new SelectListItem(g.Name, g.Name))
            .ToList();

        public void OnGet()
        {
            BindResults();
        }

        public IActionResult OnPostDelete(int id)
        {
            _users.Delete(id);
            Message = "User deleted.";
            BindResults();
            return Page();
        }

        private void BindResults()
        {
            List<User> users = _users.GetAll();

            string userIdKeyword = (UserId ?? string.Empty).Trim();
            if (!string.IsNullOrEmpty(userIdKeyword))
            {
                users = users.Where(u =>
                    u.Username.IndexOf(userIdKeyword, StringComparison.OrdinalIgnoreCase) >= 0
                ).ToList();
            }

            string staffNameKeyword = (StaffName ?? string.Empty).Trim();
            if (!string.IsNullOrEmpty(staffNameKeyword))
            {
                users = users.Where(u =>
                    u.FullName.IndexOf(staffNameKeyword, StringComparison.OrdinalIgnoreCase) >= 0
                ).ToList();
            }

            if (!string.IsNullOrEmpty(UserGroup))
                users = users.Where(u => string.Equals(u.UserGroup, UserGroup, StringComparison.OrdinalIgnoreCase)).ToList();

            if (!string.IsNullOrEmpty(Status))
                users = users.Where(u => string.Equals(u.Status, Status, StringComparison.OrdinalIgnoreCase)).ToList();

            Results = users.OrderBy(u => u.Username, StringComparer.OrdinalIgnoreCase).ToList();
        }
    }
}
