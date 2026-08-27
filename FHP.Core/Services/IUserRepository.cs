using System.Collections.Generic;
using FHP.Core.Models;

namespace FHP.Core.Services
{
    /// <summary>
    /// Storage-agnostic contract for user data. FHP.Web and FHP.AdminSetup depend on
    /// this interface only, so the JSON-backed implementation can later be swapped
    /// for a SqlUserRepository without touching any calling code.
    /// </summary>
    public interface IUserRepository
    {
        List<User> GetAll();
        User GetById(int id);
        User GetByUsernameOrEmail(string usernameOrEmail);
        bool ExistsByUsernameOrEmail(string username, string email, int? excludeId = null);
        User Add(User user);
        void Update(User user);
        void Delete(int id);
    }
}
