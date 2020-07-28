using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace health.Middleware
{
    public class ConfigureMvcOptions : IConfigureOptions<MvcOptions>
    {
        private readonly ILogger<MvcOptions> _logger;
        private readonly ObjectPoolProvider _objectPoolProvider;
        public ConfigureMvcOptions(ILogger<MvcOptions> logger, ObjectPoolProvider objectPoolProvider)
        {
            _logger = logger;
            _objectPoolProvider = objectPoolProvider;
        }

        public void Configure(MvcOptions options)
        {
            options.Filters.Add<ZFExceptionFilter>();
            options.ModelValidatorProviders.Clear();
            options.ModelValidatorProviders.Add(new ZFModelValidatorProvider());
        }
    }
}
