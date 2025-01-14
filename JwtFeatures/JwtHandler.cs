using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using IdentityUserRegistration.Entities;
using Microsoft.IdentityModel.Tokens;

namespace IdentityUserRegistration.JwtFeatures;

public class JwtHandler
{
    private readonly IConfiguration configuration;
    private readonly IConfigurationSection jwtSettings;

    public JwtHandler(IConfiguration configuration)
    {
        this.configuration = configuration;
        jwtSettings = configuration.GetSection("JwtSettings");
    }

    public string CreateToken(User user)
    {
        var signingCredentials = GetSigningCredentials();
        var claims = GetClaims(user);
        var tokenOptions = GenerateTokenOptions(signingCredentials, claims);
        return new JwtSecurityTokenHandler().WriteToken(tokenOptions);
    }
    
    private SigningCredentials GetSigningCredentials()
    {
        var securityKey = Environment.GetEnvironmentVariable("AUTH_SECRET_KEY");
        if (string.IsNullOrEmpty(securityKey))
        {
            throw new InvalidOperationException("Security key is not configured properly.");
        }
        var key = Encoding.UTF8.GetBytes(securityKey);
        var secret = new SymmetricSecurityKey(key);
        return new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
    }

    private List<Claim> GetClaims(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName)
        };
        return claims;
    }

    private JwtSecurityToken GenerateTokenOptions(SigningCredentials signingCredentials, List<Claim> claims)
    {
        var tokenOptions = new JwtSecurityToken(
            issuer: jwtSettings.GetSection("validIssuer").Value,
            audience: jwtSettings.GetSection("validAudience").Value,
            claims: claims,
            expires: DateTime.Now.AddMinutes(Convert.ToDouble(jwtSettings.GetSection("expiryInMinutes").Value)),
            signingCredentials: signingCredentials
        );

        return tokenOptions;
    }
}
