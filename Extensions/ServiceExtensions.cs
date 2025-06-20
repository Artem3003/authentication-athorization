using System.Text;
using EmailService;
using IdentityUserRegistration.Entities;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using UserLockoutIdentity;
using Casdoor.AspNetCore.Authentication;
using System.Security.Claims;
using Newtonsoft.Json.Linq;

namespace IdentityUserRegistration.Extensions;

public static class ServiceExtensions
{
    public static void ConfigureSQLContext(this IServiceCollection services, IConfiguration configuration) =>
        services.AddDbContext<DatabaseContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("sqlConnection"));
        });

    public static void ConfigureIdentity(this IServiceCollection services)
    {
        services.AddIdentity<User, Role>(opt =>
        {
            opt.Password.RequireNonAlphanumeric = false;
            opt.Password.RequiredLength = 7;
            opt.Password.RequireDigit = false;
            opt.Password.RequireUppercase = false;
            opt.Password.RequireLowercase = false;

            opt.User.RequireUniqueEmail = true;

            opt.SignIn.RequireConfirmedEmail = false;
            opt.SignIn.RequireConfirmedAccount = false;
            opt.SignIn.RequireConfirmedPhoneNumber = false;

            opt.Lockout.AllowedForNewUsers = true;
            opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(2);
            opt.Lockout.MaxFailedAccessAttempts = 3;
        }).AddEntityFrameworkStores<DatabaseContext>()
        .AddDefaultTokenProviders()
        .AddPasswordValidator<CustomPasswordValidator<User>>();

        services.Configure<DataProtectionTokenProviderOptions>(opt =>
        {
            opt.TokenLifespan = TimeSpan.FromHours(2);
        });
    }

    public static void ConfigureJWT(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JWTSettings");
        var secretKey = Environment.GetEnvironmentVariable("AUTH_SECRET_KEY") ?? throw new InvalidOperationException("Authentication secret key is not configured.");

        services.AddAuthentication(opt =>
        {
            opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(opt =>
        {
            opt.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["validIssuer"],
                ValidAudience = jwtSettings["validAudience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
            };
        });
    }
    
    public static void ConfigureAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(opt =>
        {
            opt.AddPolicy("OnlyAdminUsers", policy => policy.RequireRole("Admin"));
        });
    }

    public static void ConfigureEmailService(this IServiceCollection services, IConfiguration configuration)
    {
        var emailConfig = configuration.GetSection("EmailConfiguration").Get<EmailConfiguration>();
        services.AddSingleton(emailConfig!);
        services.AddScoped<IEmailSender, EmailSender>();
    }

    public static void ConfigureCasdoor(this IServiceCollection services, IConfiguration configuration)
    {
        var casdoorSection = configuration.GetSection("Casdoor");

        services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = "Casdoor";
        })
        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCasdoor(configuration.GetSection("Casdoor"));
    }
}