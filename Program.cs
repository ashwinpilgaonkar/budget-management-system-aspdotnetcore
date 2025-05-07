using budget_management_system_aspdotnetcore.Entities;
using budget_management_system_aspdotnetcore.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

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
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Login";  // Redirect to login if unauthorized
                    options.AccessDeniedPath = "/AccessDenied";
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // Set session timeout
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

            app.MapRazorPages();
            app.MapFallbackToPage("/Login");

            app.Run();
        }
    }
}
