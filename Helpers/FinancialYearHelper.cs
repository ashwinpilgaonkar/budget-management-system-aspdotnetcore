namespace budget_management_system_aspdotnetcore.Helpers
{
    public static class FinancialYearHelper
    {
        public static List<string> GetOptions()
        {
            var now = DateTime.Now;
            int fyStartYear = now.Month >= 7 ? now.Year : now.Year - 1;
            return new List<string>
            {
                $"FY {fyStartYear}-{fyStartYear + 1}",
                $"FY {fyStartYear - 1}-{fyStartYear}",
                "Custom"
            };
        }

        public static (DateTime start, DateTime end) GetDateRange(string? selectedFY, DateTime? customStart, DateTime? customEnd)
        {
            if (selectedFY == "Custom" && customStart.HasValue && customEnd.HasValue)
                return (customStart.Value, customEnd.Value);

            if (selectedFY?.StartsWith("FY") == true)
            {
                var parts = selectedFY.Substring(3).Split('-');
                return (
                    new DateTime(int.Parse(parts[0]), 7, 1),
                    new DateTime(int.Parse(parts[1]), 6, 30)
                );
            }

            return (DateTime.MinValue, DateTime.Today.AddDays(1).AddTicks(-1));
        }
    }
}
