﻿using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace health.web.common
{
    public static class LazyLoadingHelper
    {
        public static IServiceCollection AddLazyResolution(this IServiceCollection services)
        {
            return services.AddTransient(
                typeof(Lazy<>),
                typeof(LazilyResolved<>));
        }

        private class LazilyResolved<T> : Lazy<T>
        {
            public LazilyResolved(IServiceProvider serviceProvider)
                : base(serviceProvider.GetRequiredService<T>)
            {
            }
        }
    }
}
