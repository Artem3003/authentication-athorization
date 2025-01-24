using System.IdentityModel.Tokens.Jwt;
using authentication_athorization.DTO;
using AutoMapper;
using EmailService;
using IdentityUserRegistration.DTO;
using IdentityUserRegistration.Entities;
using IdentityUserRegistration.JwtFeatures;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace authentication_athorization.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly UserManager<User> userManager;
    private readonly IMapper mapper;
    private readonly JwtHandler jwtHandler;
    private readonly IEmailSender emailSender;

    public AccountsController(UserManager<User> userManager, IMapper mapper, JwtHandler jwtHandler, IEmailSender emailSender)
    {
        this.userManager = userManager;
        this.mapper = mapper;
        this.jwtHandler = jwtHandler;
        this.emailSender = emailSender;
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterUser([FromBody] UserForRegistrationDto userForRegistrationDto)
    {
        if (userForRegistrationDto is null)
            return BadRequest("User object is null");

        var user = mapper.Map<User>(userForRegistrationDto);

        var result = await userManager.CreateAsync(user, userForRegistrationDto.Password!);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description);
        
            return BadRequest(new RegistrationResponseDto { IsSuccessfulRegistration = false, Errors = errors });
        }

        await userManager.AddToRoleAsync(user, "Visitor");

        return Created("", result);
    }

    [HttpPost("authenticate")]
    public async Task<IActionResult> Authenticate([FromBody] UserForAuthenticationDto userForAuthenticationDto)
    {
        var user = await userManager.FindByNameAsync(userForAuthenticationDto.Email!);

        if (user is null || !await userManager.CheckPasswordAsync(user, userForAuthenticationDto.Password!))
            return Unauthorized(new RegistrationResponseDto { IsSuccessfulRegistration = false, Errors = new[] { "Invalid Authentication" } });

        var roles = await userManager.GetRolesAsync(user);
        var token = jwtHandler.CreateToken(user, roles);

        return Ok(new AuthResponseDto { IsAuthSuccessful = true, Token = token });
    }

    [HttpPost("forgotpassword")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPassword)
    {
        if(!ModelState.IsValid)
            return BadRequest();

        var user = await userManager.FindByEmailAsync(forgotPassword.Email!);

        if (user is null)
            return BadRequest("Invalid Request. User is not registered.");

        var token = await userManager.GeneratePasswordResetTokenAsync(user);

        var param = new Dictionary<string, string>
        {
            {"token", token},
            {"email", forgotPassword.Email!}
        };

        // Encoding token
        var callback = QueryHelpers.AddQueryString(forgotPassword.ClientURI!, param);

        var message = new Message(new string[] { user.Email! }, "Reset password token", callback);

        await emailSender.SendEmailAsync(message);

        return Ok();
    }

    [HttpPost("resetpassword")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPassword)
    {
        if(!ModelState.IsValid)
            return BadRequest();

        var user = await userManager.FindByEmailAsync(resetPassword.Email!);

        if (user is null)
            return BadRequest("Invalid Request. User is not registered.");

        // Decoding token
        var decodedToken = QueryHelpers.ParseQuery(resetPassword.Token!);
        foreach (var item in decodedToken)
        {
            resetPassword.Token = item.Key;
        }

        System.Console.WriteLine(resetPassword.Token);

        var result = await userManager.ResetPasswordAsync(user, resetPassword.Token!, resetPassword.Password!);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description);
            return BadRequest(new { Errors = errors });
        }

        return Ok();        
    }
}