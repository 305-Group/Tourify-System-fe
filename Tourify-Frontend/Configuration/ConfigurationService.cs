using System.Globalization;
using HotelWeb.Resources;
using Microsoft.AspNetCore.Localization;

namespace HotelWeb.Configuration
{
    public static class ConfigurationService
    {
        public static void RegisterGlobalizationAndLocalization(this IServiceCollection services) 
        {
            var defaultCultures = new[]
            {
                new CultureInfo("vi-VN"),
                new CultureInfo("en-US"),
            };

            services.Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture = new RequestCulture("vi-VN");
                options.SupportedCultures = defaultCultures;
                options.SupportedUICultures = defaultCultures;
            });           
            services.AddMvc()
                .AddDataAnnotationsLocalization(options => {
                    options.DataAnnotationLocalizerProvider = (type, factory) =>
                        factory.Create(typeof(Resource));
                });
        }
    }
}
