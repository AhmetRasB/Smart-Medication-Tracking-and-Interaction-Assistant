using MediatR;
using SMTIA.Application.Abstractions;
using SMTIA.Domain.Entities;
using TS.Result;

namespace SMTIA.Application.Features.Allergies.Add
{
    internal sealed class AddAllergyCommandHandler(
        IRepository<UserAllergy> allergyRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<AddAllergyCommand, Result<AddAllergyCommandResponse>>
    {
        public async Task<Result<AddAllergyCommandResponse>> Handle(AddAllergyCommand request, CancellationToken cancellationToken)
        {
            var allergy = new UserAllergy
            {
                UserId = request.UserId,
                AllergyName = request.AllergyName,
                Description = request.Description,
                Severity = request.Severity,
                CreatedAt = DateTime.UtcNow
            };

            await allergyRepository.AddAsync(allergy, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return new AddAllergyCommandResponse(allergy.Id, "Alerji başarıyla eklendi");
        }
    }
}

