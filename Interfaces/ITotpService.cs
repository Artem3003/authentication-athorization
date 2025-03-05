namespace IdentityUserRegistration.Interfaces;

public interface ITotpService
{
    string GenerateSecretKey();
    string GenerateQrCodeUrl(string email, string secretKey);
    byte[] GenerateQRCode(string uri);
    bool ValidateOTP(string secretKey, string otp);
}