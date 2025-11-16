using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SMTIA.Application.Services;
using SMTIA.Domain.Entities;
using TS.Result;

namespace SMTIA.Application.Features.Auth.ForgotPassword
{
    internal sealed class ForgotPasswordCommandHandler(
        UserManager<AppUser> userManager,
        IEmailService emailService) : IRequestHandler<ForgotPasswordCommand, Result<ForgotPasswordCommandResponse>>
    {
        public async Task<Result<ForgotPasswordCommandResponse>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await userManager.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

            // Güvenlik için: Kullanıcı yoksa bile başarılı mesaj döndür
            if (user == null)
            {
                return new ForgotPasswordCommandResponse("Eğer bu e-posta adresi ile kayıtlı bir hesap varsa, şifre sıfırlama linki gönderilmiştir.");
            }

            // Şifre sıfırlama token'ı oluştur
            var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);

            // Şifre sıfırlama linki oluştur
            var resetLink = $"https://localhost:7054/api/auth/reset-password?email={Uri.EscapeDataString(user.Email!)}&token={Uri.EscapeDataString(resetToken)}";

            // E-posta gönder
            var emailBody = $@"<!DOCTYPE html>
<html lang=""tr"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>SMTIA - Şifre Sıfırlama</title>
    <style>
        * {{
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }}
        
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
            background: linear-gradient(135deg, #f5f1eb 0%, #e8e3d9 100%);
            padding: 40px 20px;
            line-height: 1.6;
        }}
        
        .email-container {{
            max-width: 600px;
            margin: 0 auto;
            background: #ffffff;
            border-radius: 24px;
            box-shadow: 0 8px 32px rgba(0, 0, 0, 0.08);
            overflow: hidden;
        }}
        
        .email-header {{
            background: linear-gradient(135deg, #a8c8a0 0%, #8db87d 100%);
            padding: 40px 30px;
            text-align: center;
            position: relative;
        }}
        
        .email-header::before {{
            content: '';
            position: absolute;
            top: -50%;
            right: -20%;
            width: 300px;
            height: 300px;
            background: radial-gradient(circle, rgba(255, 255, 255, 0.1) 0%, transparent 70%);
            border-radius: 50%;
        }}
        
        .capsule-icon {{
            width: 80px;
            height: 40px;
            margin: 0 auto 20px;
            border: 3px solid rgba(255, 255, 255, 0.8);
            border-radius: 40px;
            position: relative;
            background: rgba(255, 255, 255, 0.2);
        }}
        
        .capsule-icon::before {{
            content: '';
            position: absolute;
            left: 50%;
            top: 0;
            width: 2px;
            height: 100%;
            background: rgba(255, 255, 255, 0.8);
            transform: translateX(-50%);
        }}
        
        .email-header h1 {{
            color: #ffffff;
            font-size: 28px;
            font-weight: 600;
            margin-bottom: 8px;
            position: relative;
            z-index: 1;
        }}
        
        .email-header p {{
            color: rgba(255, 255, 255, 0.95);
            font-size: 16px;
            position: relative;
            z-index: 1;
        }}
        
        .email-body {{
            padding: 48px 40px;
        }}
        
        .greeting {{
            color: #4a4a4a;
            font-size: 18px;
            font-weight: 500;
            margin-bottom: 20px;
        }}
        
        .content {{
            color: #5a5a5a;
            font-size: 16px;
            margin-bottom: 32px;
            line-height: 1.8;
        }}
        
        .button-container {{
            text-align: center;
            margin: 40px 0;
        }}
        
        .button {{
            display: inline-block;
            background: linear-gradient(135deg, #a8c8a0 0%, #8db87d 100%);
            color: white !important;
            text-decoration: none;
            padding: 16px 40px;
            border-radius: 12px;
            font-weight: 500;
            font-size: 16px;
            box-shadow: 0 4px 12px rgba(141, 184, 125, 0.3);
            transition: all 0.3s ease;
        }}
        
        .link-text {{
            color: #7a7a7a;
            font-size: 14px;
            margin-top: 24px;
            padding: 16px;
            background: #f8f6f3;
            border-radius: 8px;
            word-break: break-all;
            font-family: monospace;
        }}
        
        .footer {{
            margin-top: 40px;
            padding-top: 32px;
            border-top: 1px solid #e8e3d9;
            text-align: center;
        }}
        
        .footer p {{
            color: #7a7a7a;
            font-size: 14px;
            margin-bottom: 8px;
        }}
        
        .footer .team-name {{
            color: #a8c8a0;
            font-weight: 600;
            font-size: 16px;
        }}
        
        .expiry-note {{
            color: #9a9a9a;
            font-size: 13px;
            font-style: italic;
            margin-top: 20px;
        }}
        
        .warning {{
            color: #d4a5a5;
            font-size: 14px;
            margin-top: 20px;
            padding: 12px;
            background: #fef5f5;
            border-radius: 8px;
            border-left: 3px solid #d4a5a5;
        }}
    </style>
</head>
<body>
    <div class=""email-container"">
        <div class=""email-header"">
            <div class=""capsule-icon""></div>
            <h1>SMTIA</h1>
            <p>Şifre Sıfırlama</p>
        </div>
        
        <div class=""email-body"">
            <p class=""greeting"">Merhaba {user.FirstName},</p>
            
            <p class=""content"">
                Şifrenizi sıfırlamak için aşağıdaki butona tıklayın. Bu işlemi siz yapmadıysanız, bu e-postayı görmezden gelebilirsiniz.
            </p>
            
            <div class=""button-container"">
                <a href=""{resetLink}"" class=""button"">Şifremi Sıfırla</a>
            </div>
            
            <p class=""content"">
                Veya bu linki tarayıcınıza yapıştırın:
            </p>
            
            <div class=""link-text"">
                {resetLink}
            </div>
            
            <p class=""expiry-note"">
                ⏰ Bu link 24 saat geçerlidir.
            </p>
            
            <div class=""warning"">
                ⚠️ Güvenliğiniz için: Bu linki başka biriyle paylaşmayın. Eğer şifre sıfırlama talebinde bulunmadıysanız, hesabınızı kontrol edin.
            </div>
            
            <div class=""footer"">
                <p>Saygılarımızla,</p>
                <p class=""team-name"">SMTIA Ekibi</p>
            </div>
        </div>
    </div>
</body>
</html>";

            try
            {
                await emailService.SendEmailAsync(
                    user.Email!,
                    "SMTIA - Şifre Sıfırlama",
                    emailBody,
                    cancellationToken);
            }
            catch
            {
                // Log the error but don't fail the request
            }

            return new ForgotPasswordCommandResponse("Eğer bu e-posta adresi ile kayıtlı bir hesap varsa, şifre sıfırlama linki gönderilmiştir.");
        }
    }
}

