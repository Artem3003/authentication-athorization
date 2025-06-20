using IdentityUserRegistration.JwtFeatures;
using IdentityUserRegistration.Extensions;
using IdentityUserRegistration.Interfaces;
using IdentityUserRegistration.Services;
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
builder.Services.ConfigureCasdoor(builder.Configuration);

builder.Services.AddSingleton<ITotpService, TotpService>();

builder.Services.AddSingleton<JwtHandler>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseWebSockets();

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
