using FluentValidation;

namespace SMTIA.Application.Features.Medicines.Search
{
    internal sealed class SearchLocalMedicinesQueryValidator : AbstractValidator<SearchLocalMedicinesQuery>
    {
        public SearchLocalMedicinesQueryValidator()
        {
            RuleFor(x => x.Query)
                .NotEmpty().WithMessage("Arama kelimesi gereklidir")
                .MinimumLength(2).WithMessage("Arama kelimesi en az 2 karakter olmalıdır")
                .MaximumLength(200).WithMessage("Arama kelimesi en fazla 200 karakter olabilir");

            RuleFor(x => x.Limit)
                .InclusiveBetween(1, 50).WithMessage("Limit 1 ile 50 arasında olmalıdır");
        }
    }
}


