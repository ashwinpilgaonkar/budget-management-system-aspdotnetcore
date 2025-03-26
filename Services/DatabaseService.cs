using budget_management_system_aspdotnetcore.Entities;
using Microsoft.EntityFrameworkCore;

namespace budget_management_system_aspdotnetcore.Services
{
    public class DatabaseService : IDatabaseService
    {
        private readonly CasdbtestContext _context;
        public DatabaseService(CasdbtestContext context)
        {
            _context = context;
        }

        public List<User> GetUserList()
        {
            var dbResult = _context.Users.ToList();

            if(dbResult == null)
            {
                return new List<User>();
            } else
            {
                return dbResult;
            }
        }

        public async Task<bool> AddUpdate(string jobTitle)
        {

            try
            {
                var dbResult = _context.Users.FirstOrDefault();

/*                if (dbResult == null)
                {
                    await _context.Users.AddAsync(new User()
                    {
                        FirstName = "John",
                        LastName = "Doe2",
                        Email = "john.doe@example.com",
                        PhoneNumber = "12334567890",
                        HireDate = DateTime.Now,
                        JobTitle = jobTitle,
                        Salary = 50000,
                        Role = "admin"
                    });
                } else
                {
                    dbResult.JobTitle = jobTitle;
                }*/

                await _context.SaveChangesAsync();
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}
