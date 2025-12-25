using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace SMTIA.Infrastructure.Options
{
    internal sealed class GemmaOptionsSetup : IConfigureOptions<GemmaOptions>
    {
        private readonly IConfiguration _configuration;

        public GemmaOptionsSetup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void Configure(GemmaOptions options)
        {
            _configuration.GetSection("Gemma").Bind(options);
        }
    }
}

