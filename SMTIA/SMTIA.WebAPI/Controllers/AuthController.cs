using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMTIA.Application.Features.Auth.ConfirmEmail;
using SMTIA.Application.Features.Auth.ForgotPassword;
using SMTIA.Application.Features.Auth.Login;
using SMTIA.Application.Features.Auth.Register;
using SMTIA.Application.Features.Auth.ResetPassword;
using SMTIA.WebAPI.Abstractions;

namespace SMTIA.WebAPI.Controllers
{
    [AllowAnonymous]
    public sealed class AuthController : ApiController
    {
        public AuthController(IMediator mediator) : base(mediator)
        {
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginCommand request, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(request, cancellationToken);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterCommand request, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(request, cancellationToken);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string email, [FromQuery] string token, CancellationToken cancellationToken)
        {
            var command = new ConfirmEmailCommand(email, token);
            var response = await _mediator.Send(command, cancellationToken);
            
            if (response.StatusCode == 200)
            {
                return Content(GetSuccessHtml(email), "text/html");
            }
            else if (response.StatusCode == 404)
            {
                return Content(GetErrorHtml("Kullanıcı Bulunamadı", "Belirtilen e-posta adresi ile kayıtlı bir kullanıcı bulunamadı."), "text/html");
            }
            else if (response.StatusCode == 400)
            {
                return Content(GetErrorHtml("Onay Başarısız", response.Data ?? "E-posta onayı başarısız oldu. Lütfen geçerli bir onay linki kullanın."), "text/html");
            }
            
            return Content(GetErrorHtml("Hata", "Beklenmeyen bir hata oluştu."), "text/html");
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(request, cancellationToken);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("reset-password")]
        public IActionResult ResetPasswordPage([FromQuery] string email, [FromQuery] string token)
        {
            return Content(GetResetPasswordFormHtml(email, token), "text/html");
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(request, cancellationToken);
            
            if (response.StatusCode == 200)
            {
                return Content(GetResetPasswordSuccessHtml(request.Email), "text/html");
            }
            else if (response.StatusCode == 404)
            {
                return Content(GetErrorHtml("Kullanıcı Bulunamadı", "Belirtilen e-posta adresi ile kayıtlı bir kullanıcı bulunamadı."), "text/html");
            }
            else if (response.StatusCode == 400)
            {
                var errorMessage = response.Data is ResetPasswordCommandResponse responseData 
                    ? responseData.Message 
                    : "Şifre sıfırlama başarısız oldu. Lütfen geçerli bir link kullanın veya yeni bir şifre sıfırlama talebinde bulunun.";
                return Content(GetErrorHtml("Şifre Sıfırlama Başarısız", errorMessage), "text/html");
            }
            
            return Content(GetErrorHtml("Hata", "Beklenmeyen bir hata oluştu."), "text/html");
        }

        private static string GetResetPasswordFormHtml(string email, string token)
        {
            return $@"<!DOCTYPE html>
<html lang=""tr"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Şifre Sıfırlama - SMTIA</title>
    <style>
        * {{
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }}
        
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
            background: linear-gradient(135deg, #f5f1eb 0%, #e8e3d9 100%);
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            padding: 20px;
        }}
        
        .container {{
            background: #ffffff;
            border-radius: 24px;
            box-shadow: 0 8px 32px rgba(0, 0, 0, 0.08);
            max-width: 500px;
            width: 100%;
            padding: 48px 40px;
            position: relative;
            overflow: hidden;
        }}
        
        .container::before {{
            content: '';
            position: absolute;
            top: -50%;
            right: -20%;
            width: 300px;
            height: 300px;
            background: radial-gradient(circle, rgba(200, 180, 160, 0.1) 0%, transparent 70%);
            border-radius: 50%;
        }}
        
        .icon {{
            width: 80px;
            height: 80px;
            margin: 0 auto 24px;
            background: linear-gradient(135deg, #a8c8a0 0%, #8db87d 100%);
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
            position: relative;
            z-index: 1;
        }}
        
        .icon::after {{
            content: '🔒';
            font-size: 40px;
        }}
        
        h1 {{
            color: #4a4a4a;
            font-size: 28px;
            font-weight: 600;
            margin-bottom: 12px;
            text-align: center;
            position: relative;
            z-index: 1;
        }}
        
        .subtitle {{
            color: #7a7a7a;
            font-size: 16px;
            line-height: 1.6;
            margin-bottom: 32px;
            text-align: center;
            position: relative;
            z-index: 1;
        }}
        
        .form-group {{
            margin-bottom: 24px;
            position: relative;
            z-index: 1;
        }}
        
        label {{
            display: block;
            color: #4a4a4a;
            font-weight: 500;
            margin-bottom: 8px;
            font-size: 14px;
        }}
        
        input[type=""password""] {{
            width: 100%;
            padding: 14px 16px;
            border: 2px solid #e8e3d9;
            border-radius: 12px;
            font-size: 16px;
            transition: all 0.3s ease;
            background: #ffffff;
            color: #4a4a4a;
        }}
        
        input[type=""password""]:focus {{
            outline: none;
            border-color: #a8c8a0;
            box-shadow: 0 0 0 3px rgba(168, 200, 160, 0.1);
        }}
        
        .button {{
            width: 100%;
            background: linear-gradient(135deg, #a8c8a0 0%, #8db87d 100%);
            color: white;
            border: none;
            padding: 16px 32px;
            border-radius: 12px;
            font-weight: 500;
            font-size: 16px;
            cursor: pointer;
            transition: all 0.3s ease;
            box-shadow: 0 4px 12px rgba(141, 184, 125, 0.3);
            position: relative;
            z-index: 1;
        }}
        
        .button:hover {{
            transform: translateY(-2px);
            box-shadow: 0 6px 16px rgba(141, 184, 125, 0.4);
        }}
        
        .button:active {{
            transform: translateY(0);
        }}
        
        .capsule-decoration {{
            position: absolute;
            bottom: -30px;
            left: 50%;
            transform: translateX(-50%) rotate(-15deg);
            width: 120px;
            height: 60px;
            border: 2px solid rgba(200, 180, 160, 0.3);
            border-radius: 60px;
            opacity: 0.5;
        }}
        
        .capsule-decoration::before {{
            content: '';
            position: absolute;
            left: 50%;
            top: 0;
            width: 2px;
            height: 100%;
            background: rgba(200, 180, 160, 0.3);
        }}
        
        .error-message {{
            color: #d4a5a5;
            font-size: 14px;
            margin-top: 8px;
            display: none;
        }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""icon""></div>
        <h1>Şifre Sıfırlama</h1>
        <p class=""subtitle"">
            Yeni şifrenizi belirleyin. Şifreniz en az 6 karakter olmalıdır.
        </p>
        
        <form method=""POST"" action=""/api/auth/reset-password"" id=""resetForm"">
            <input type=""hidden"" name=""Email"" value=""{email}"">
            <input type=""hidden"" name=""Token"" value=""{token}"">
            
            <div class=""form-group"">
                <label for=""newPassword"">Yeni Şifre</label>
                <input type=""password"" id=""newPassword"" name=""NewPassword"" required minlength=""6"" placeholder=""Yeni şifrenizi girin"">
            </div>
            
            <div class=""form-group"">
                <label for=""confirmPassword"">Şifre Tekrar</label>
                <input type=""password"" id=""confirmPassword"" name=""confirmPassword"" required minlength=""6"" placeholder=""Şifrenizi tekrar girin"">
            </div>
            
            <button type=""submit"" class=""button"">Şifremi Sıfırla</button>
        </form>
        
        <div class=""capsule-decoration""></div>
    </div>
    
    <script>
        document.getElementById('resetForm').addEventListener('submit', function(e) {{
            var password = document.getElementById('newPassword').value;
            var confirmPassword = document.getElementById('confirmPassword').value;
            
            if (password !== confirmPassword) {{
                e.preventDefault();
                alert('Şifreler eşleşmiyor. Lütfen tekrar kontrol edin.');
                return false;
            }}
            
            if (password.length < 6) {{
                e.preventDefault();
                alert('Şifre en az 6 karakter olmalıdır.');
                return false;
            }}
        }});
    </script>
</body>
</html>";
        }

        private static string GetResetPasswordSuccessHtml(string email)
        {
            return $@"<!DOCTYPE html>
<html lang=""tr"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Şifre Sıfırlandı - SMTIA</title>
    <style>
        * {{
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }}
        
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
            background: linear-gradient(135deg, #f5f1eb 0%, #e8e3d9 100%);
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            padding: 20px;
        }}
        
        .container {{
            background: #ffffff;
            border-radius: 24px;
            box-shadow: 0 8px 32px rgba(0, 0, 0, 0.08);
            max-width: 500px;
            width: 100%;
            padding: 48px 40px;
            text-align: center;
            position: relative;
            overflow: hidden;
        }}
        
        .container::before {{
            content: '';
            position: absolute;
            top: -50%;
            right: -20%;
            width: 300px;
            height: 300px;
            background: radial-gradient(circle, rgba(200, 180, 160, 0.1) 0%, transparent 70%);
            border-radius: 50%;
        }}
        
        .success-icon {{
            width: 80px;
            height: 80px;
            margin: 0 auto 24px;
            background: linear-gradient(135deg, #a8c8a0 0%, #8db87d 100%);
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
            position: relative;
            z-index: 1;
            animation: scaleIn 0.5s ease-out;
        }}
        
        @keyframes scaleIn {{
            from {{
                transform: scale(0);
                opacity: 0;
            }}
            to {{
                transform: scale(1);
                opacity: 1;
            }}
        }}
        
        .success-icon::after {{
            content: '✓';
            color: white;
            font-size: 48px;
            font-weight: bold;
        }}
        
        h1 {{
            color: #4a4a4a;
            font-size: 28px;
            font-weight: 600;
            margin-bottom: 12px;
            position: relative;
            z-index: 1;
        }}
        
        .subtitle {{
            color: #7a7a7a;
            font-size: 16px;
            line-height: 1.6;
            margin-bottom: 32px;
            position: relative;
            z-index: 1;
        }}
        
        .button {{
            display: inline-block;
            background: linear-gradient(135deg, #a8c8a0 0%, #8db87d 100%);
            color: white;
            text-decoration: none;
            padding: 14px 32px;
            border-radius: 12px;
            font-weight: 500;
            font-size: 16px;
            transition: all 0.3s ease;
            box-shadow: 0 4px 12px rgba(141, 184, 125, 0.3);
            position: relative;
            z-index: 1;
        }}
        
        .button:hover {{
            transform: translateY(-2px);
            box-shadow: 0 6px 16px rgba(141, 184, 125, 0.4);
        }}
        
        .capsule-decoration {{
            position: absolute;
            bottom: -30px;
            left: 50%;
            transform: translateX(-50%) rotate(-15deg);
            width: 120px;
            height: 60px;
            border: 2px solid rgba(200, 180, 160, 0.3);
            border-radius: 60px;
            opacity: 0.5;
        }}
        
        .capsule-decoration::before {{
            content: '';
            position: absolute;
            left: 50%;
            top: 0;
            width: 2px;
            height: 100%;
            background: rgba(200, 180, 160, 0.3);
        }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""success-icon""></div>
        <h1>Şifre Başarıyla Sıfırlandı!</h1>
        <p class=""subtitle"">
            <span class=""email"">{email}</span> adresiniz için şifreniz başarıyla sıfırlandı.
        </p>
        <p class=""subtitle"">
            Artık yeni şifrenizle giriş yapabilirsiniz.
        </p>
        <a href=""/swagger"" class=""button"">Giriş Yap</a>
        <div class=""capsule-decoration""></div>
    </div>
</body>
</html>";
        }

        private static string GetSuccessHtml(string email)
        {
            return $@"<!DOCTYPE html>
<html lang=""tr"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>E-posta Onaylandı - SMTIA</title>
    <style>
        * {{
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }}
        
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
            background: linear-gradient(135deg, #f5f1eb 0%, #e8e3d9 100%);
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            padding: 20px;
        }}
        
        .container {{
            background: #ffffff;
            border-radius: 24px;
            box-shadow: 0 8px 32px rgba(0, 0, 0, 0.08);
            max-width: 500px;
            width: 100%;
            padding: 48px 40px;
            text-align: center;
            position: relative;
            overflow: hidden;
        }}
        
        .container::before {{
            content: '';
            position: absolute;
            top: -50%;
            right: -20%;
            width: 300px;
            height: 300px;
            background: radial-gradient(circle, rgba(200, 180, 160, 0.1) 0%, transparent 70%);
            border-radius: 50%;
        }}
        
        .success-icon {{
            width: 80px;
            height: 80px;
            margin: 0 auto 24px;
            background: linear-gradient(135deg, #a8c8a0 0%, #8db87d 100%);
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
            position: relative;
            z-index: 1;
            animation: scaleIn 0.5s ease-out;
        }}
        
        @keyframes scaleIn {{
            from {{
                transform: scale(0);
                opacity: 0;
            }}
            to {{
                transform: scale(1);
                opacity: 1;
            }}
        }}
        
        .success-icon::after {{
            content: '✓';
            color: white;
            font-size: 48px;
            font-weight: bold;
        }}
        
        h1 {{
            color: #4a4a4a;
            font-size: 28px;
            font-weight: 600;
            margin-bottom: 12px;
            position: relative;
            z-index: 1;
        }}
        
        .subtitle {{
            color: #7a7a7a;
            font-size: 16px;
            line-height: 1.6;
            margin-bottom: 32px;
            position: relative;
            z-index: 1;
        }}
        
        .email {{
            color: #5a5a5a;
            font-weight: 500;
            word-break: break-all;
        }}
        
        .button {{
            display: inline-block;
            background: linear-gradient(135deg, #a8c8a0 0%, #8db87d 100%);
            color: white;
            text-decoration: none;
            padding: 14px 32px;
            border-radius: 12px;
            font-weight: 500;
            font-size: 16px;
            transition: all 0.3s ease;
            box-shadow: 0 4px 12px rgba(141, 184, 125, 0.3);
            position: relative;
            z-index: 1;
        }}
        
        .button:hover {{
            transform: translateY(-2px);
            box-shadow: 0 6px 16px rgba(141, 184, 125, 0.4);
        }}
        
        .capsule-decoration {{
            position: absolute;
            bottom: -30px;
            left: 50%;
            transform: translateX(-50%) rotate(-15deg);
            width: 120px;
            height: 60px;
            border: 2px solid rgba(200, 180, 160, 0.3);
            border-radius: 60px;
            opacity: 0.5;
        }}
        
        .capsule-decoration::before {{
            content: '';
            position: absolute;
            left: 50%;
            top: 0;
            width: 2px;
            height: 100%;
            background: rgba(200, 180, 160, 0.3);
        }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""success-icon""></div>
        <h1>E-posta Onaylandı!</h1>
        <p class=""subtitle"">
            <span class=""email"">{email}</span> adresiniz başarıyla onaylandı.
        </p>
        <p class=""subtitle"">
            Artık SMTIA'ya giriş yapabilir ve tüm özelliklerden faydalanabilirsiniz.
        </p>
        <a href=""/swagger"" class=""button"">Giriş Yap</a>
        <div class=""capsule-decoration""></div>
    </div>
</body>
</html>";
        }

        private static string GetErrorHtml(string title, string message)
        {
            return $@"<!DOCTYPE html>
<html lang=""tr"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>{title} - SMTIA</title>
    <style>
        * {{
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }}
        
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
            background: linear-gradient(135deg, #f5f1eb 0%, #e8e3d9 100%);
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            padding: 20px;
        }}
        
        .container {{
            background: #ffffff;
            border-radius: 24px;
            box-shadow: 0 8px 32px rgba(0, 0, 0, 0.08);
            max-width: 500px;
            width: 100%;
            padding: 48px 40px;
            text-align: center;
            position: relative;
            overflow: hidden;
        }}
        
        .container::before {{
            content: '';
            position: absolute;
            top: -50%;
            right: -20%;
            width: 300px;
            height: 300px;
            background: radial-gradient(circle, rgba(200, 180, 160, 0.1) 0%, transparent 70%);
            border-radius: 50%;
        }}
        
        .error-icon {{
            width: 80px;
            height: 80px;
            margin: 0 auto 24px;
            background: linear-gradient(135deg, #d4a5a5 0%, #c88a8a 100%);
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
            position: relative;
            z-index: 1;
        }}
        
        .error-icon::after {{
            content: '✕';
            color: white;
            font-size: 48px;
            font-weight: bold;
        }}
        
        h1 {{
            color: #4a4a4a;
            font-size: 28px;
            font-weight: 600;
            margin-bottom: 12px;
            position: relative;
            z-index: 1;
        }}
        
        .subtitle {{
            color: #7a7a7a;
            font-size: 16px;
            line-height: 1.6;
            margin-bottom: 32px;
            position: relative;
            z-index: 1;
        }}
        
        .button {{
            display: inline-block;
            background: linear-gradient(135deg, #d4a5a5 0%, #c88a8a 100%);
            color: white;
            text-decoration: none;
            padding: 14px 32px;
            border-radius: 12px;
            font-weight: 500;
            font-size: 16px;
            transition: all 0.3s ease;
            box-shadow: 0 4px 12px rgba(200, 138, 138, 0.3);
            position: relative;
            z-index: 1;
        }}
        
        .button:hover {{
            transform: translateY(-2px);
            box-shadow: 0 6px 16px rgba(200, 138, 138, 0.4);
        }}
        
        .capsule-decoration {{
            position: absolute;
            bottom: -30px;
            left: 50%;
            transform: translateX(-50%) rotate(-15deg);
            width: 120px;
            height: 60px;
            border: 2px solid rgba(200, 180, 160, 0.3);
            border-radius: 60px;
            opacity: 0.5;
        }}
        
        .capsule-decoration::before {{
            content: '';
            position: absolute;
            left: 50%;
            top: 0;
            width: 2px;
            height: 100%;
            background: rgba(200, 180, 160, 0.3);
        }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""error-icon""></div>
        <h1>{title}</h1>
        <p class=""subtitle"">
            {message}
        </p>
        <a href=""/swagger"" class=""button"">Ana Sayfaya Dön</a>
        <div class=""capsule-decoration""></div>
    </div>
</body>
</html>";
        }
    }
}