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

        public bool IsAdmin(HttpContext httpContext)
        {
            string? roleString = httpContext.Session.GetString("Role");

            if (string.IsNullOrEmpty(roleString))
            {
                throw new Exception("User is not authenticated or session has expired.");
            }

            // Convert the string back to the enum safely
            if (Enum.TryParse<UserRole>(roleString, out var role))
            {
                return role == UserRole.admin;
            }

            throw new Exception("Invalid user role stored in session.");
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