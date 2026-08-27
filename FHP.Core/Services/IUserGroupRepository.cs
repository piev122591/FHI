using System.Collections.Generic;
using FHP.Core.Models;

namespace FHP.Core.Services
{
    /// <summary>
    /// Storage-agnostic contract for user group master data (the groups assigned to
    /// login users on the User Setup screen).
    /// </summary>
    public interface IUserGroupRepository
    {
        List<UserGroup> GetAll();
        UserGroup GetById(int id);
        bool ExistsByName(string name, int? excludeId = null);
        UserGroup Add(UserGroup group);
        void Update(UserGroup group);
        void Delete(int id);
    }
}
