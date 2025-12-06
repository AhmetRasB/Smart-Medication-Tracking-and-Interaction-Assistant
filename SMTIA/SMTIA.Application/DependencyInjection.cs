using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SMTIA.Application.Behaviors;
using SMTIA.Application.Services;

namespace SMTIA.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(DependencyInjection).Assembly);

            services.AddMediatR(conf =>
            {
                conf.RegisterServicesFromAssemblies(typeof(DependencyInjection).Assembly);
                conf.AddOpenBehavior(typeof(ValidationBehavior<,>));
            });

            services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

            // Register schedule timing service
            services.AddScoped<IScheduleTimingService, ScheduleTimingService>();

            return services;
        }
    }
}
