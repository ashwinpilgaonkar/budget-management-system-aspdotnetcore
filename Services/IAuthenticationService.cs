namespace budget_management_system_aspdotnetcore.Services
{
    public interface IAuthenticationService
    {
        bool IsAuthenticated(HttpContext httpContext);

        string GetUserRole(HttpContext httpContext);

        bool IsAFO(HttpContext httpContext);

        bool IsAdminRole(HttpContext httpContext);

        int GetAuthenticatedUserID(HttpContext httpContext);
    }
}
