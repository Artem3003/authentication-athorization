using IdentityUserRegistration.JwtFeatures;
using IdentityUserRegistration.Extensions;
using IdentityUserRegistration.Interfaces;
using IdentityUserRegistration.Services;
using System.Net.Security;
using authentication_athorization.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddAutoMapper(typeof(Program));
builder.Services.ConfigureSQLContext(builder.Configuration);
builder.Services.ConfigureEmailService(builder.Configuration);
builder.Services.ConfigureIdentity();
builder.Services.AddSignalR();
// builder.Services.ConfigureJWT(builder.Configuration);
builder.Services.ConfigureAuthorization();

builder.Services.AddSingleton<ITotpService, TotpService>();

builder.Services.AddSingleton<JwtHandler>();

builder.Services.AddControllersWithViews();

if (OperatingSystem.IsLinux())
{
    builder.WebHost.ConfigureKestrel(serverOptions =>
    {
        serverOptions.ConfigureHttpsDefaults(httpsOptions =>
        {
            httpsOptions.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
            httpsOptions.OnAuthenticate = (context, sslOptions) =>
            {
                sslOptions.CipherSuitesPolicy = new CipherSuitesPolicy(
                    new[] { TlsCipherSuite.TLS_RSA_WITH_AES_256_CBC_SHA256 });
            };
        });
    });
}

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<PriceHub>("/priceHub");

app.MapControllers();

await app.RunAsync();
