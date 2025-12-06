using FluentValidation;

namespace SMTIA.Application.Features.Schedules.Update
{
    internal sealed class UpdateScheduleCommandValidator : AbstractValidator<UpdateScheduleCommand>
    {
        public UpdateScheduleCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Takvim ID gereklidir");

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("Kullanıcı ID gereklidir");

            RuleFor(x => x.ScheduleName)
                .NotEmpty().WithMessage("Takvim adı gereklidir")
                .MaximumLength(200).WithMessage("Takvim adı en fazla 200 karakter olabilir");

            RuleFor(x => x.StartDate)
                .NotEmpty().WithMessage("Başlangıç tarihi gereklidir");
        }
    }
}

