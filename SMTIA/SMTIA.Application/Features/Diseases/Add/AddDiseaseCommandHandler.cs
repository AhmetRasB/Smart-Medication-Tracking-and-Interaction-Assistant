using MediatR;
using SMTIA.Application.Abstractions;
using SMTIA.Domain.Entities;
using TS.Result;

namespace SMTIA.Application.Features.Diseases.Add
{
    internal sealed class AddDiseaseCommandHandler(
        IRepository<UserDisease> diseaseRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<AddDiseaseCommand, Result<AddDiseaseCommandResponse>>
    {
        public async Task<Result<AddDiseaseCommandResponse>> Handle(AddDiseaseCommand request, CancellationToken cancellationToken)
        {
            var disease = new UserDisease
            {
                UserId = request.UserId,
                DiseaseName = request.DiseaseName,
                Description = request.Description,
                DiagnosisDate = request.DiagnosisDate,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await diseaseRepository.AddAsync(disease, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return new AddDiseaseCommandResponse(disease.Id, "Hastalık başarıyla eklendi");
        }
    }
}

