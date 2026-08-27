using System;

namespace FHP.Core.Models
{
    public static class UserRoles
    {
        public const string Admin = "Admin";
        public const string User = "User";
    }

    public static class UserStatuses
    {
        public const string Active = "Active";
        public const string Inactive = "Inactive";
    }

    // Department/team grouping shown on the User Setup screen — distinct from Role,
    // which drives login authorization. Managed separately under Login User > User Group.
    public static class UserGroups
    {
        public const string Admin = "ADMIN";
        public const string Stokist = "Stokist";
        public const string Marketing = "Marketing";
        public const string CustomerService = "Customer Service";
        public const string WarehouseStaff = "Warehouse Staff";

        public static readonly string[] All =
        {
            Admin, Stokist, Marketing, CustomerService, WarehouseStaff
        };
    }

    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }

        // PBKDF2 hash in the form "{iterations}.{saltBase64}.{hashBase64}" — see PasswordHasher.
        public string PasswordHash { get; set; }

        // Second, separate password used to authorize sensitive actions (e.g. fund
        // withdrawals). Null/empty until the user sets one via Change Security Password.
        public string SecurityPasswordHash { get; set; }

        public string Role { get; set; } = UserRoles.User;
        public string UserGroup { get; set; } = UserGroups.Admin;
        public string Status { get; set; } = UserStatuses.Active;
        public DateTime CreatedDate { get; set; }
        public string LastUpdateBy { get; set; }
        public DateTime LastUpdateDate { get; set; }
    }
}
