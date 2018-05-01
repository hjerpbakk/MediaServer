using System;
using CachePopulator.Services;
using Microsoft.AspNetCore.Builder;

namespace CachePopulator.Extensions {
    public static class ApplicationBuilderExtensions {
        public static void WarmupCache(this IApplicationBuilder applicationBuilder) {
            Console.WriteLine("Starting warmup");
            var fireAndForgetService = (FireAndForgetService)applicationBuilder.ApplicationServices.GetService(typeof(FireAndForgetService));
            fireAndForgetService.TouchEndpoints().GetAwaiter().GetResult();
        }
    }
}
