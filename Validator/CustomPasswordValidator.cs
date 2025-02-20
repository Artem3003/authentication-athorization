using Microsoft.AspNetCore.Identity;

namespace UserLockoutIdentity;

public class CustomPasswordValidator<TUser> : IPasswordValidator<TUser> where TUser : class
{
    public async Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user, string? password)
    {
        var username = await manager.GetUserNameAsync(user);
        if (string.Equals(username, password, StringComparison.OrdinalIgnoreCase))
            return IdentityResult.Failed(new IdentityError 
            { 
                Code = "PasswordMatchesUsername", 
                Description = "Password cannot be the same as username." 
            });

        if (password!.ToLower().Contains("password"))
            return IdentityResult.Failed(new IdentityError 
            { 
                Code = "PasswordContainsPassword", 
                Description = "Password cannot contain the word 'password'." 
            });

        return IdentityResult.Success;
    }
}
