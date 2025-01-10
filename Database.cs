using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace IdentityManagement;

public class Database
{
    private static string UserHash(string username) => Convert.ToBase64String(MD5.HashData(Encoding.UTF8.GetBytes(username)));

    public async Task<User?> GetUserAsync(string username)
    {
        var userHash = UserHash(username);

        if (!File.Exists(userHash))
        {
            return null;
        }

        await using var reader = File.OpenRead(userHash);
        return await JsonSerializer.DeserializeAsync<User>(reader);
    }

    public async Task PutAsync(User user)
    {
        var userHash = UserHash(user.Username);

        await using var writer = File.OpenWrite(userHash);
        await JsonSerializer.SerializeAsync(writer, user);
    }
}

public class User
{
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public List<UserClaim> Claims { get; set; } = new();
}

public class UserClaim
{
    public string Type { get; set; }
    public string Value { get; set; }
}
