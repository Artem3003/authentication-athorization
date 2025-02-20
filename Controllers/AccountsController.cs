using System.IdentityModel.Tokens.Jwt;
using System.Text;
using authentication_athorization.DTO;
using authentication_athorization.Interfaces;
using AutoMapper;
using EmailService;
using IdentityUserRegistration.DTO;
using IdentityUserRegistration.Entities;
using IdentityUserRegistration.JwtFeatures;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Org.BouncyCastle.Asn1.X509;

namespace authentication_athorization.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly UserManager<User> userManager;
    private readonly IMapper mapper;
    private readonly JwtHandler jwtHandler;
    private readonly IEmailSender emailSender;
    private readonly  ITotpService totpService;

    public AccountsController(UserManager<User> userManager, IMapper mapper, JwtHandler jwtHandler, IEmailSender emailSender, ITotpService totpService)
    {
        this.userManager = userManager;
        this.mapper = mapper;
        this.jwtHandler = jwtHandler;
        this.emailSender = emailSender;
        this.totpService = totpService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterUser([FromBody] UserForRegistrationDto userForRegistrationDto)
    {
        if (userForRegistrationDto is null)
            return BadRequest("User object is null");

        var user = mapper.Map<User>(userForRegistrationDto);

        var secretKey = totpService.GenerateSecretKey();
        user.EnctyptedSecretKey = EncryptSecretKey(secretKey);
        var uri = totpService.GenerateQrCodeUrl(user.Email!, secretKey);
        var qrCodeImage = totpService.GenerateQRCode(uri);

        var result = await userManager.CreateAsync(user, userForRegistrationDto.Password!);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description);
        
            return BadRequest(new RegistrationResponseDto { IsSuccessfulRegistration = false, Errors = errors });
        }

        await userManager.SetTwoFactorEnabledAsync(user, true);

        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var param = new Dictionary<string, string>
        {
            {"token", token},
            {"email", user.Email!}
        };

        var callback = QueryHelpers.AddQueryString(userForRegistrationDto.ClientUri!, param!);

        var message = new Message(new string[] { user.Email! }, "Email Confirmation token", callback);

        await emailSender.SendEmailAsync(message);

        await userManager.AddToRoleAsync(user, "Visitor");

        return Created("", new { result, qrCodeImage} );
    }    

    [HttpPost("authenticate")]
    public async Task<IActionResult> Authenticate([FromBody] UserForAuthenticationDto userForAuthenticationDto)
    {
        var user = await userManager.FindByNameAsync(userForAuthenticationDto.Email!);

        if (user is null)
            return BadRequest("Invalid Request. User is not registered.");

        if (await userManager.IsLockedOutAsync(user))
            return Unauthorized(new AuthResponseDto { IsAuthSuccessful = false, ErrorMessage = "User account is locked out." });

        if (!await userManager.IsEmailConfirmedAsync(user))
            return Unauthorized(new AuthResponseDto { IsAuthSuccessful = false, ErrorMessage = "Email is not confirmed" });

        if (!await userManager.CheckPasswordAsync(user, userForAuthenticationDto.Password!))
        {
            await userManager.AccessFailedAsync(user);
            if (await userManager.IsLockedOutAsync(user))
            {
                var content = $"Your account is locked out. If you want to reset the password, " + $"you can use the Forgot Password link on the login page";

                var message = new Message(new string[] { userForAuthenticationDto.Email! }, "Account Locked", content);

                await emailSender.SendEmailAsync(message);

                return Unauthorized(new AuthResponseDto { IsAuthSuccessful = false, ErrorMessage = "User account is locked out." });
            }

            return Unauthorized(new AuthResponseDto { IsAuthSuccessful = false, ErrorMessage = "Invalid Authentication" });
        }

        if (await userManager.GetTwoFactorEnabledAsync(user))
            return await GenerateOTPFor2StepVerification(user);

        var roles = await userManager.GetRolesAsync(user);
        var token = jwtHandler.CreateToken(user, roles);

        await userManager.ResetAccessFailedCountAsync(user);

        return Ok(new AuthResponseDto { IsAuthSuccessful = true, Token = token });
    }

    [HttpPost("twofactor")]
    public async Task<IActionResult> TwoFactor([FromBody] TwoFactorDto twoFactorDto)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        var user = await userManager.FindByEmailAsync(twoFactorDto.Email!);

        if (user is null)
            return BadRequest("Invalid Request. User is not registered.");

        var validVerification = await userManager.VerifyTwoFactorTokenAsync(user, twoFactorDto.Provider!, twoFactorDto.Token!);

        var roles = await userManager.GetRolesAsync(user);
        var token = jwtHandler.CreateToken(user, roles);

        return Ok(new AuthResponseDto { IsAuthSuccessful = true, Token = token});
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
        var callback = QueryHelpers.AddQueryString(forgotPassword.ClientURI!, param!);

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

        await userManager.SetLockoutEndDateAsync(user, null);

        return Ok();        
    }

    [HttpPost("validateotp")]
    public async Task<IActionResult> ValidateOtp([FromBody] ValidateOtpDto validateOtpDto)
    {
        var user = await userManager.FindByEmailAsync(validateOtpDto.Email!);

        if (user is null)
            return BadRequest("Invalid Request. User is not registered.");  

        var decryptedSecretKey = DecryptSecretKey(user.EnctyptedSecretKey!);
        var isValidOtp = totpService.ValidateOTP(decryptedSecretKey, validateOtpDto.Code!);

        if (!isValidOtp)
            return BadRequest("Invalid OTP");

        return Ok();
    }
    
    [HttpGet("emailconfirmation")]
    public async Task<IActionResult> EmailConfirmation([FromQuery] string email, [FromQuery] string token)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
            return BadRequest("Invalid Request. User is not registered.");

        var result = await userManager.ConfirmEmailAsync(user, token);

        if (!result.Succeeded)
            return BadRequest("Invalid Request. Email cannot be confirmed.");

        return Ok();
    }

    private async Task<IActionResult> GenerateOTPFor2StepVerification(User user)
    {
        var providers = await userManager.GetValidTwoFactorProvidersAsync(user);

        if (!providers.Contains("Email"))
            return Unauthorized(new AuthResponseDto { IsAuthSuccessful = false, Is2FactorRequired = false, ErrorMessage = "Invalid 2-Factor Provider." });            
    
        var token = await userManager.GenerateTwoFactorTokenAsync(user, "Email");
        
        var message = new Message(new string[] { user.Email! }, "Authentication token", token);

        await emailSender.SendEmailAsync(message);

        return Ok(new AuthResponseDto { Is2FactorRequired = true, Provider = "Email" });
    }

    private static string EncryptSecretKey(string secretKey)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(secretKey));
    }
    private static string DecryptSecretKey(string encryptedSecretKey)
    {
        return Encoding.UTF8.GetString(Convert.FromBase64String(encryptedSecretKey));
    }
}