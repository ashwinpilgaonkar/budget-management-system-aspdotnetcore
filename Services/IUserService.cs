using budget_management_system_aspdotnetcore.Entities;

namespace budget_management_system_aspdotnetcore.Services
{
    public interface IUserService
    {
        User? ValidateUser(string email, string password);
    }
}