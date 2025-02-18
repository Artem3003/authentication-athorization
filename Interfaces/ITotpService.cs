using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace authentication_athorization.Interfaces;

public interface ITotpService
{
    string GenerateSecretKey();
    string GenerateQrCodeUrl(string email, string secretKey);
    byte[] GenerateQRCode(string uri);
    bool ValidateOTP(string secretKey, string otp);
}