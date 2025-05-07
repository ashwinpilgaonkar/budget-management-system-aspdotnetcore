namespace budget_management_system_aspdotnetcore.Services
{
    public interface IAuthenticationService
    {
        bool IsAuthenticated(HttpContext httpContext);

        bool IsAdmin(HttpContext httpContext);

        int GetAuthenticatedUserID(HttpContext httpContext);
    }
}
