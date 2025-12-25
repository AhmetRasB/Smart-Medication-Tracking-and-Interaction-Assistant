using System.Reflection;
using System.Text;
using GenericRepository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Scrutor;
using SMTIA.Application.Abstractions;
using SMTIA.Domain.Abstractions;
using SMTIA.Domain.Entities;
using SMTIA.Infrastructure.Context;
using SMTIA.Infrastructure.Options;
using SMTIA.Infrastructure.Repositories;

namespace SMTIA.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(configuration.GetConnectionString("PosgreSql"));
            });

            services.AddScoped<GenericRepository.IUnitOfWork>(srv => srv.GetRequiredService<ApplicationDbContext>());
            services.AddScoped<Application.Abstractions.IUnitOfWork>(srv => srv.GetRequiredService<ApplicationDbContext>());
            
            // Register repositories for each entity
            services.AddScoped(typeof(Application.Abstractions.IRepository<>), typeof(EfRepository<>));

            // Register email service
            services.AddScoped<Application.Services.IEmailService, Services.EmailService>();

            // Register FDA service
            // OpenFDA disabled -> use dummy service for stable UX
            services.AddScoped<Application.Services.IFdaService, Services.DummyFdaService>();

            // Register Audit Log service
            services.AddScoped<Application.Services.IAuditLogService, Services.AuditLogService>();

            // Register Gemma AI service
            services.Configure<GemmaOptions>(configuration.GetSection("Gemma"));
            services.ConfigureOptions<GemmaOptionsSetup>();
            services.AddHttpClient<Application.Services.IGemmaInteractionAnalyzer, Services.GemmaInteractionAnalyzer>();

            services
                .AddIdentity<AppUser, IdentityRole<Guid>>(cfr =>
                {
                    cfr.Password.RequiredLength = 1;
                    cfr.Password.RequireNonAlphanumeric = false;
                    cfr.Password.RequireUppercase = false;
                    cfr.Password.RequireLowercase = false;
                    cfr.Password.RequireDigit = false;
                    cfr.SignIn.RequireConfirmedEmail = true;
                    cfr.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                    cfr.Lockout.MaxFailedAccessAttempts = 3;
                    cfr.Lockout.AllowedForNewUsers = true;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer();
            services.ConfigureOptions<JwtTokenOptionsSetup>();
            services.AddAuthorization();

            services.Scan(action =>
            {
                action
                .FromAssemblies(Assembly.GetExecutingAssembly())
                .AddClasses(publicOnly: false)
                .UsingRegistrationStrategy(RegistrationStrategy.Skip)
                .AsMatchingInterface()
                .AsImplementedInterfaces()
                .WithScopedLifetime();
            });


            services.AddHealthChecks()
            .AddCheck("health-check", () => HealthCheckResult.Healthy())
            .AddDbContextCheck<ApplicationDbContext>()
            ;

            return services;
        }
    }
}