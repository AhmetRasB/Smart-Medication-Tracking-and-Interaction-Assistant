using MediatR;
using Microsoft.AspNetCore.Identity;
using SMTIA.Domain.Entities;
using TS.Result;

namespace SMTIA.Application.Features.Auth.Register
{
    internal sealed class RegisterCommandHandler(
        UserManager<AppUser> userManager) : IRequestHandler<RegisterCommand, Result<RegisterCommandResponse>>
    {
        public async Task<Result<RegisterCommandResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            AppUser appUser = new()
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                UserName = request.UserName,
                DateOfBirth = request.DateOfBirth,
                Weight = request.Weight
            };

            IdentityResult result = await userManager.CreateAsync(appUser, request.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return (500, $"Kullanıcı kaydı başarısız: {errors}");
            }

            return new RegisterCommandResponse("Kullanıcı başarıyla kaydedildi");
        }
    }
}
