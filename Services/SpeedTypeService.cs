using budget_management_system_aspdotnetcore.Entities;
using Microsoft.EntityFrameworkCore;

namespace budget_management_system_aspdotnetcore.Services
{
    public class SpeedTypeService
    {
        private readonly CasdbtestContext _context;

        public SpeedTypeService(CasdbtestContext context)
        {
            _context = context;
        }
        public async Task<List<SpeedType>> GetSpeedTypesAsync()
        {
            return await _context.SpeedTypes.ToListAsync();
        }
    }
}
