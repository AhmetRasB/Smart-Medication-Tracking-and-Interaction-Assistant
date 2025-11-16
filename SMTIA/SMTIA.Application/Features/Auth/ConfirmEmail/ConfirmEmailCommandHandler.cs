using MediatR;
using Microsoft.AspNetCore.Identity;
using SMTIA.Domain.Entities;
using TS.Result;

namespace SMTIA.Application.Features.Auth.ConfirmEmail
{
    internal sealed class ConfirmEmailCommandHandler(
        UserManager<AppUser> userManager) : IRequestHandler<ConfirmEmailCommand, Result<string>>
    {
        public async Task<Result<string>> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            
            if (user == null)
            {
                return (404, "Kullanıcı bulunamadı");
            }

            if (user.EmailConfirmed)
            {
                return "E-posta zaten onaylanmış";
            }

            var result = await userManager.ConfirmEmailAsync(user, request.Token);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return (400, $"E-posta onayı başarısız: {errors}");
            }

            return "E-posta başarıyla onaylandı";
        }
    }
}

