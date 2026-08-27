using System;
using System.Collections.Generic;
using System.Linq;
using FHP.Core.Models;
using FHP.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace FHP.Web.Pages
{
    public class UsersModel : SecurePageModel
    {
        private readonly IUserRepository _users;

        public UsersModel(IUserRepository users)
        {
            _users = users;
        }

        [BindProperty(SupportsGet = true)]
        public string Search { get; set; }

        [BindProperty(SupportsGet = true)]
        public string RoleFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string StatusFilter { get; set; }

        public List<User> Users { get; private set; } = new List<User>();
        public string Message { get; private set; }

        public void OnGet()
        {
            BindList();
        }

        public IActionResult OnPostDelete(int id)
        {
            _users.Delete(id);
            Message = "User deleted.";
            BindList();
            return Page();
        }

        private void BindList()
        {
            List<User> users = _users.GetAll();

            string keyword = (Search ?? string.Empty).Trim();
            if (!string.IsNullOrEmpty(keyword))
            {
                users = users.Where(u =>
                    u.FullName.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    u.Username.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    u.Email.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0
                ).ToList();
            }

            if (!string.IsNullOrEmpty(RoleFilter))
                users = users.Where(u => u.Role == RoleFilter).ToList();

            if (!string.IsNullOrEmpty(StatusFilter))
                users = users.Where(u => u.Status == StatusFilter).ToList();

            Users = users.OrderBy(u => u.Id).ToList();
        }
    }
}
