using System.Collections.Generic;
using FHP.Core.Models;

namespace FHP.Core.Services
{
    /// <summary>
    /// Storage-agnostic contract for dashboard group master data (Dashboard Setup screen).
    /// </summary>
    public interface IDashboardGroupRepository
    {
        List<DashboardGroup> GetAll();
        DashboardGroup GetById(int id);
        bool ExistsByName(string name, int? excludeId = null);
        DashboardGroup Add(DashboardGroup group);
        void Update(DashboardGroup group);
        void Delete(int id);
    }
}
