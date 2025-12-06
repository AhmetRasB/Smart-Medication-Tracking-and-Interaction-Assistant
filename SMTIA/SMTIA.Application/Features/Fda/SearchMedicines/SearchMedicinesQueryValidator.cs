using FluentValidation;

namespace SMTIA.Application.Features.Fda.SearchMedicines
{
    internal sealed class SearchMedicinesQueryValidator : AbstractValidator<SearchMedicinesQuery>
    {
        public SearchMedicinesQueryValidator()
        {
            RuleFor(x => x.SearchTerm)
                .NotEmpty().WithMessage("Arama terimi zorunludur")
                .MinimumLength(2).WithMessage("Arama terimi en az 2 karakter olmalıdır")
                .MaximumLength(100).WithMessage("Arama terimi en fazla 100 karakter olmalıdır");

            RuleFor(x => x.Limit)
                .InclusiveBetween(1, 50).WithMessage("Limit 1 ile 50 arasında olmalıdır");
        }
    }
}

