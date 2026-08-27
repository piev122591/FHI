using System;

namespace FHP.Core.Models
{
    public class UserGroup
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Status { get; set; } = UserStatuses.Active;
        public DateTime CreatedDate { get; set; }
        public string LastUpdateBy { get; set; }
        public DateTime LastUpdateDate { get; set; }
    }
}
