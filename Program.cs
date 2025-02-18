using IdentityUserRegistration.JwtFeatures;
using IdentityUserRegistration.Extensions;
using authentication_athorization.Interfaces;
using authentication_athorization.Services;
using IdentityUserRegistration;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddAutoMapper(typeof(Program));

builder.Services.ConfigureSQLContext(builder.Configuration);
builder.Services.ConfigureEmailService(builder.Configuration);
builder.Services.ConfigureIdentity();
builder.Services.ConfigureJWT(builder.Configuration);
builder.Services.ConfigureAuthorization();

builder.Services.AddSingleton<ITotpService, TotpService>();

builder.Services.AddSingleton<JwtHandler>();

builder.Services.AddControllers();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
