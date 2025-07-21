using budget_management_system_aspdotnetcore.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace budget_management_system_aspdotnetcore.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        public bool IsAuthenticated(HttpContext httpContext)
        {
            bool isAuthenticated = !string.IsNullOrEmpty(httpContext.Session.GetString("UserId")) || !string.IsNullOrEmpty(httpContext.Session.GetString("Email")) || !string.IsNullOrEmpty(httpContext.Session.GetString("FirstName")) || !string.IsNullOrEmpty(httpContext.Session.GetString("LastName")) || !string.IsNullOrEmpty(httpContext.Session.GetString("Role"));
            return isAuthenticated;
        }

        public string GetUserRole(HttpContext httpContext)
        {
            string? roleString = httpContext.Session.GetString("RoleID");

            if (string.IsNullOrEmpty(roleString))
            {
                throw new Exception("User is not authenticated or session has expired.");
            }

            return roleString;
        }


        public int GetAuthenticatedUserID(HttpContext httpContext)
        {
            int? userId = httpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                // Handle the missing session case — e.g., throw, redirect, or return a default value
                throw new Exception("User is not authenticated.");
            }

            return userId.Value;
        }
    }
}