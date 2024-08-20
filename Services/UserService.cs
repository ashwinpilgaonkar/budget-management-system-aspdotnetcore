using budget_management_system_aspdotnetcore.Entities;
using System.Diagnostics;

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

            Debug.WriteLine("====== SERVICE ======");
            Debug.WriteLine(email);
            Debug.WriteLine(password);
            Debug.WriteLine(_context.Users.FirstOrDefault(u => u.Email == email && u.Password == password));
            Debug.WriteLine("====================");

            return _context.Users.FirstOrDefault(_context.Users.FirstOrDefault(u => u.Email.Equals(email) && u.Password.Equals(password)));
        }
    }
}