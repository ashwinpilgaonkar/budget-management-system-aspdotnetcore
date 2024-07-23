using budget_management_system_aspdotnetcore.Entities;

namespace budget_management_system_aspdotnetcore.Services
{
    public class UserService : IUserService
    {
        private readonly CasdbtestContext _context;

        public UserService(CasdbtestContext context)
        {
            _context = context;
        }

        public User? ValidateUser(string email, string password)
        {
            // In a real application, you should hash the password and compare it

            return _context.Users.FirstOrDefault(u => u.Email.Equals(email) && u.Password.Equals(password));
        }
    }
}