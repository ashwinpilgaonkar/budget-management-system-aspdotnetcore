
using budget_management_system_aspdotnetcore.Entities;

namespace budget_management_system_aspdotnetcore.Services
{
    public interface IDatabaseService
    {
        Task<bool> AddUpdate(string jobTitle);
        List<User> GetUserList();
    }
}
