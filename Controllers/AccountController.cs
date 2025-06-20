using System.Security.Claims;
using AutoMapper;
using IdentityUserRegistration.Entities;
using IdentityUserRegistration.VM;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityUserRegistration.Controllers;

[Route("[controller]")]
public class AccountController : Controller
{
    private readonly UserManager<User> userManager;
    private readonly IMapper mapper;
    private readonly SignInManager<User> signInManager;
    private readonly ILogger<AccountController> logger;

    public AccountController(UserManager<User> userManager, IMapper mapper, SignInManager<User> signInManager, ILogger<AccountController> logger)
    {
        this.userManager = userManager;
        this.mapper = mapper;
        this.signInManager = signInManager;
        this.logger = logger;
    }

    [HttpGet("Login")]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost("Login")]
    public async Task<IActionResult> Login(LoginViewModel loginViewModel)
    {
        if (ModelState.IsValid)
        {
            var result = await signInManager.PasswordSignInAsync(loginViewModel.Email!, loginViewModel.Password!, loginViewModel.RememberMe, false);
        
            if (result.Succeeded)
            {
                logger.LogInformation("User logged in.");
                return RedirectToAction("Index", "Home");
            }
            else 
            {
                logger.LogError("Error logging in user.");
                ModelState.AddModelError("", "Email or password is incorrect.");
                return View(loginViewModel);
            }
        }
        return View(loginViewModel);
    }

    [HttpGet("Register")]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost("Register")]
    public async Task<IActionResult> Register(RegisterViewModel registerViewModel)
    {
        if (ModelState.IsValid)
        {
            var user = mapper.Map<User>(registerViewModel);

            // Save user to database
            var result = await userManager.CreateAsync(user, registerViewModel.Password!);

            if (result.Succeeded)
            {
                logger.LogInformation("User created a new account with password.");
                return RedirectToAction("Login", "Account");
            }
            else
            {
                logger.LogError("Error creating user account.");
                foreach(var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(registerViewModel);
            }
        }
        return View(registerViewModel);
    }

    [HttpGet("VerifyEmail")]
    public IActionResult VerifyEmail()
    {
        return View();
    }

    [HttpPost("VerifyEmail")]
    public async Task<IActionResult> VerifyEmail(VerifyEmailViewModel verifyEmailViewModel)
    {
        if (ModelState.IsValid)
        {
            var user = await userManager.FindByEmailAsync(verifyEmailViewModel.Email!);

            if (user is null)
            {
                ModelState.AddModelError("", "Email not found.");
                return View(verifyEmailViewModel);
            }
            else 
            {
                return RedirectToAction("ChangePassword", "Account", new { username = user.Email });
            }
        }
        return View(verifyEmailViewModel);
    }

    [HttpGet("ChangePassword")]
    public IActionResult ChangePassword(string username)
    {
        if (string.IsNullOrEmpty(username))
        {
            return RedirectToAction("VerifyEmail", "Account");
        }

        return View(new ChangePasswordViewModel { Email = username });
    }

    [HttpPost("ChangePassword")]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel changePasswordViewModel)
    {
        if (ModelState.IsValid)
        {
            var user = await userManager.FindByEmailAsync(changePasswordViewModel.Email!);
            if (user != null)
            {
                var result = await userManager.RemovePasswordAsync(user);
                if (result.Succeeded)
                {
                    logger.LogInformation("Password removed.");
                    _ = await userManager.AddPasswordAsync(user, changePasswordViewModel.NewPassword!);
                    return RedirectToAction("Login", "Account");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(changePasswordViewModel);
                }
            }
            else
            {
                ModelState.AddModelError("", "Email not found.");
                return View(changePasswordViewModel);
            }
        }
        else
        {
            ModelState.AddModelError("", "Something went wrong. Please try again.");
            return View(changePasswordViewModel);
        }
    }

    [HttpGet("Logout")]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        logger.LogInformation("User logged out.");
        return RedirectToAction("Index", "Home");
    }

    [HttpGet("ExternalLogin")]
    public IActionResult ExternalLogin(string provider, string returnUrl = "/")
    {
        var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
        var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return Challenge(properties, provider);
    }

    [HttpGet("/callback")]
    public async Task<IActionResult> ExternalLoginCallback(string returnUrl = "/")
    {
        var info = await signInManager.GetExternalLoginInfoAsync();
        if (info == null) return RedirectToAction(nameof(Login));

        var result = await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
        if (result.Succeeded) return LocalRedirect(returnUrl);

        // Extract claims
        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        var name = info.Principal.FindFirstValue(ClaimTypes.Name);
        var firstName = info.Principal.FindFirstValue(ClaimTypes.GivenName);
        var lastName = info.Principal.FindFirstValue(ClaimTypes.Surname);

        // Create Identity User if not exists
        var user = new User
        {
            UserName = email,
            Email = email,
            FirstName = firstName ?? name?.Split(' ').FirstOrDefault(),
            LastName = lastName ?? name?.Split(' ').Skip(1).FirstOrDefault(),
            EmailConfirmed = true
        };

        var identityResult = await userManager.CreateAsync(user);
        if (!identityResult.Succeeded)
        {
            foreach (var error in identityResult.Errors)
                ModelState.AddModelError("", error.Description);
            return RedirectToAction(nameof(Login));
        }

        identityResult = await userManager.AddLoginAsync(user, info);
        if (!identityResult.Succeeded)
        {
            foreach (var error in identityResult.Errors)
                ModelState.AddModelError("", error.Description);
            return RedirectToAction(nameof(Login));
        }

        await signInManager.SignInAsync(user, isPersistent: false);
        return LocalRedirect(returnUrl);
    }
}