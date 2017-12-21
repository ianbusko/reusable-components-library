using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System.Reflection;

namespace ComponentLibrary
{
    public static class Extensions
    {
        public static void AddComponentLibraryViews(this IServiceCollection services)
        {
            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.FileProviders.Add(new EmbeddedFileProvider(typeof(ComponentLibrary.ViewComponents.NavComponent)
                    .GetTypeInfo().Assembly));
            });
        }
    }
}
