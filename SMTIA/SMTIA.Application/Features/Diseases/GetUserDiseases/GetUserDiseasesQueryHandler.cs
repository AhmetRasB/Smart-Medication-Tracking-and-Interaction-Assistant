using MediatR;
using SMTIA.Application.Abstractions;
using SMTIA.Domain.Entities;
using TS.Result;

namespace SMTIA.Application.Features.Diseases.GetUserDiseases
{
    internal sealed class GetUserDiseasesQueryHandler(
        IRepository<UserDisease> diseaseRepository) : IRequestHandler<GetUserDiseasesQuery, Result<List<GetUserDiseasesQueryResponse>>>
    {
        public async Task<Result<List<GetUserDiseasesQueryResponse>>> Handle(GetUserDiseasesQuery request, CancellationToken cancellationToken)
        {
            var allDiseases = await diseaseRepository.ListAllAsync(cancellationToken);
            var userDiseases = allDiseases
                .Where(d => d.UserId == request.UserId)
                .Select(d => new GetUserDiseasesQueryResponse(
                    d.Id,
                    d.DiseaseName,
                    d.Description,
                    d.DiagnosisDate,
                    d.IsActive,
                    d.CreatedAt
                ))
                .ToList();

            return userDiseases;
        }
    }
}

