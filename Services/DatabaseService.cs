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

        public List<Employee> GetEmployeeList()
        {
            var dbResult = _context.Employees.ToList();

            if(dbResult == null)
            {
                return new List<Employee>();
            } else
            {
                return dbResult;
            }
        }

        public async Task<bool> AddUpdate(string jobTitle)
        {

            try
            {
                var dbResult = _context.Employees.FirstOrDefault();

                if (dbResult == null)
                {
                    await _context.Employees.AddAsync(new Employee()
                    {
                        FirstName = "John",
                        LastName = "Doe2",
                        DateOfBirth = DateTime.Now,
                        Email = "john.doe@example.com",
                        PhoneNumber = "12334567890",
                        HireDate = DateTime.Now,
                        JobTitle = jobTitle,
                        Salary = 50000
                    });
                } else
                {
                    dbResult.JobTitle = jobTitle;
                }

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
