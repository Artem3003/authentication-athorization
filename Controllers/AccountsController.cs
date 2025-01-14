using System.IdentityModel.Tokens.Jwt;
using AutoMapper;
using IdentityUserRegistration.DTO;
using IdentityUserRegistration.Entities;
using IdentityUserRegistration.JwtFeatures;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace authentication_athorization.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly UserManager<User> userManager;
    private readonly IMapper mapper;
    private readonly JwtHandler jwtHandler;

    public AccountsController(UserManager<User> userManager, IMapper mapper, JwtHandler jwtHandler)
    {
        this.userManager = userManager;
        this.mapper = mapper;
        this.jwtHandler = jwtHandler;
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterUser([FromBody] UserForRegistrationDto userForRegistrationDto)
    {
        if (userForRegistrationDto is null)
            return BadRequest("User object is null");

        var user = mapper.Map<User>(userForRegistrationDto);

        var result = await userManager.CreateAsync(user, userForRegistrationDto.Password);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description);
        
            return BadRequest(new RegistrationResponseDto { IsSuccessfulRegistration = false, Errors = errors });
        }

        return Created("", result);
    }

    [HttpPost("authenticate")]
    public async Task<IActionResult> Authenticate([FromBody] UserForAuthenticationDto userForAuthenticationDto)
    {
        var user = await userManager.FindByNameAsync(userForAuthenticationDto.Email!);

        if (user is null || !await userManager.CheckPasswordAsync(user, userForAuthenticationDto.Password!))
            return Unauthorized(new RegistrationResponseDto { IsSuccessfulRegistration = false, Errors = new[] { "Invalid Authentication" } });

        var token = jwtHandler.CreateToken(user);

        return Ok(new AuthResponseDto { IsAuthSuccessful = true, Token = token });
    }
}