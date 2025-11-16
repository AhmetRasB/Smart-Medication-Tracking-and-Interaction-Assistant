using MediatR;
using SMTIA.Application.Abstractions;
using SMTIA.Domain.Entities;
using TS.Result;

namespace SMTIA.Application.Features.Allergies.GetUserAllergies
{
    internal sealed class GetUserAllergiesQueryHandler(
        IRepository<UserAllergy> allergyRepository) : IRequestHandler<GetUserAllergiesQuery, Result<List<GetUserAllergiesQueryResponse>>>
    {
        public async Task<Result<List<GetUserAllergiesQueryResponse>>> Handle(GetUserAllergiesQuery request, CancellationToken cancellationToken)
        {
            var allAllergies = await allergyRepository.ListAllAsync(cancellationToken);
            var userAllergies = allAllergies
                .Where(a => a.UserId == request.UserId)
                .Select(a => new GetUserAllergiesQueryResponse(
                    a.Id,
                    a.AllergyName,
                    a.Description,
                    a.Severity,
                    a.CreatedAt
                ))
                .ToList();

            return userAllergies;
        }
    }
}

