using FluentValidation;

namespace SMTIA.Application.Features.Interactions.Analyze
{
    internal sealed class AnalyzeInteractionsCommandValidator : AbstractValidator<AnalyzeInteractionsCommand>
    {
        public AnalyzeInteractionsCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("Kullanıcı ID gereklidir");

            RuleFor(x => x)
                .Must(x => x.NewMedicineId.HasValue || !string.IsNullOrWhiteSpace(x.NewMedicineName))
                .WithMessage("Yeni ilaç ID'si veya adı belirtilmelidir");

            When(x => !string.IsNullOrWhiteSpace(x.NewMedicineName), () =>
            {
                RuleFor(x => x.NewMedicineName)
                    .MaximumLength(200).WithMessage("İlaç adı en fazla 200 karakter olabilir");
            });
        }
    }
}

