using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SMTIA.Domain.Entities;
using TS.Result;

namespace SMTIA.Application.Features.Auth.ResetPassword
{
    internal sealed class ResetPasswordCommandHandler(
        UserManager<AppUser> userManager) : IRequestHandler<ResetPasswordCommand, Result<ResetPasswordCommandResponse>>
    {
        public async Task<Result<ResetPasswordCommandResponse>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await userManager.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

            if (user == null)
            {
                return (404, "Kullanıcı bulunamadı");
            }

            var result = await userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return (400, $"Şifre sıfırlama başarısız: {errors}");
            }

            return new ResetPasswordCommandResponse("Şifreniz başarıyla sıfırlandı. Artık yeni şifrenizle giriş yapabilirsiniz.");
        }
    }
}

