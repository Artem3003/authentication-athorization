using authentication_athorization.Interfaces;
using OtpNet;
using QRCoder;

namespace authentication_athorization.Services;

public class TotpService : ITotpService
{
    public string GenerateSecretKey()
    {
        var secretKey = KeyGeneration.GenerateRandomKey(20);

        return Base32Encoding.ToString(secretKey);
    }

    public string GenerateQrCodeUrl(string email, string secretKey)
    {
        var issuer = Uri.EscapeDataString("authentication-athorization");
        var userEmail = Uri.EscapeDataString(email);

        return $"otpauth://totp/{issuer}:{userEmail}?secret={secretKey}&issuer={issuer}&algotithm=SHA1&digits=6&period=30";
    }

    public byte[] GenerateQRCode(string uri)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(uri, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);

        return qrCode.GetGraphic(20);
    }

    public bool ValidateOTP(string secretKey, string otp)
    {
        var totp = new Totp(Base32Encoding.ToBytes(secretKey));

        return totp.VerifyTotp(otp, out _, VerificationWindow.RfcSpecifiedNetworkDelay);
    }

}
