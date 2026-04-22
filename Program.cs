using budget_management_system_aspdotnetcore.Entities;
using budget_management_system_aspdotnetcore.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace budget_management_system_aspdotnetcore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<CasdbtestContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddTransient<IDatabaseService, DatabaseService>();

            builder.Services.AddScoped<IUserService, UserService>();

            builder.Services.AddScoped<SpeedTypeService>();

            // Add services to the container.
            builder.Services.AddRazorPages();

            // Add authentication services
            builder.Services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "DataProtectionKeys")));

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Login";
                    options.AccessDeniedPath = "/AccessDenied";
                    options.ExpireTimeSpan = TimeSpan.FromDays(30);
                    options.SlidingExpiration = false;
                });

            builder.Services.AddAuthorization();

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<UserService>();

            builder.Services.AddSession();

            builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            builder.Services.AddSingleton<IAuthenticationService, AuthenticationService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseSession();

            // Enable authentication & authorization middleware
            app.UseAuthentication();

            app.UseAuthorization();

            app.Use(async (context, next) =>
            {
                if (context.User.Identity?.IsAuthenticated == true &&
                    string.IsNullOrEmpty(context.Session.GetString("Email")))
                {
                    var uid = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (uid != null)
                    {
                        context.Session.SetInt32("UserId",    int.Parse(uid));
                        context.Session.SetString("Email",     context.User.FindFirstValue(ClaimTypes.Email)     ?? "");
                        context.Session.SetString("FirstName", context.User.FindFirstValue(ClaimTypes.GivenName) ?? "");
                        context.Session.SetString("LastName",  context.User.FindFirstValue(ClaimTypes.Surname)   ?? "");
                        context.Session.SetString("RoleID",    context.User.FindFirstValue("RoleID")             ?? "");
                        context.Session.SetString("RoleName",  context.User.FindFirstValue(ClaimTypes.Role)      ?? "");
                    }
                }
                await next();
            });

            app.MapRazorPages();
            app.MapFallbackToPage("/Login");

            app.Run();
        }
    }
}
