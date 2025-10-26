using SMTIA.Application.Features.Auth.Login;
using SMTIA.Domain.Entities;

namespace SMTIA.Application.Services
{
    public interface IJwtProvider
    {
        Task<LoginCommandResponse> CreateToken(AppUser user);
    }
}
