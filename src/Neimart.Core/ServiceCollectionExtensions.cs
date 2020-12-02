using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Neimart.Core;
using Neimart.Core.Infrastructure.Caching;
using Neimart.Core.Infrastructure.Database;
using Neimart.Core.Infrastructure.Imaging;
using Neimart.Core.Infrastructure.Sending;
using Neimart.Core.Infrastructure.Storing;
using Neimart.Core.Infrastructure.Paying;
using Neimart.Core.Infrastructure.Paying.PaySwitch;
using Neimart.Core.Infrastructure.Web;
using Neimart.Core.Settings;
using reCAPTCHA.AspNetCore;
using Neimart.Core.Infrastructure.Paying.Flutterwave;

namespace Neimart.Core
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddUnitOfWork<TContext>(this IServiceCollection services)
            where TContext : DbContext
        {
            services.AddScoped<IUnitOfWork, UnitOfWork<TContext>>();
            return services;
        }

        public static IServiceCollection AddCacheManager(this IServiceCollection services)
        {
            services.AddScoped<ICacheManager, CacheManager>();
            return services;
        }


        public static IServiceCollection AddLocalFileClient(this IServiceCollection services, Action<LocalFileClientOptions> configure = null)
        {
            if (configure != null)
                services.Configure(configure);

            services.AddTransient<IFileClient>(serviceProvider =>
            {
                var appSettings = serviceProvider.GetRequiredService<IOptions<AppSettings>>();
                var options = serviceProvider.GetRequiredService<IOptions<LocalFileClientOptions>>();
                var httpContextAccessor = serviceProvider.GetRequiredService<IActionContextAccessor>();

                options.Value.AllowedFileExtensions = appSettings.Value.AnyFileExtensions;
                options.Value.AllowedFileSize = appSettings.Value.AnyFileMaxSize;
                return new LocalFileClient(options, httpContextAccessor);
            });

            return services;
        }

        public static IServiceCollection AddSkiaSharpProcessor(this IServiceCollection services)
        {
            services.AddSingleton<IImageProcessor, SkiaSharpProcessor>();
            return services;
        }

        public static IServiceCollection AddSmtpEmailSender(this IServiceCollection services, Action<SmtpEmailSenderOptions> configure = null)
        {
            if (configure != null)
                services.Configure(configure);

            services.AddScoped<IEmailSender, SmtpEmailSender>();
            return services;
        }

        public static IServiceCollection AddSmsSender(this IServiceCollection services, Action<SmsSenderOptions> configure = null)
        {
            if (configure != null)
                services.Configure(configure);

            services.AddScoped<ISmsSender, SmsSender>(); //
            return services;
        }

        public static IServiceCollection AddRazorViewRenderer(this IServiceCollection services)
        {
            services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddTransient<IRazorViewRenderer, RazorViewRenderer>();
            return services;
        }

        public static void AddPaySwitch(this IServiceCollection services, Action<PaySwitchOptions> configure = null)
        {
            if (configure != null)
                services.Configure(configure);

            services.AddTransient<IPaymentProcessor, PaySwitchProcessor>();
            services.AddHttpClient(nameof(PaySwitchProcessor))
                    .ConfigurePrimaryHttpMessageHandler(messageHandler =>
                    {
                        var handler = new HttpClientHandler();
                        if (handler.SupportsAutomaticDecompression)
                        {
                            handler.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                        }
                        return handler;
                    });
        }

        public static void AddFlutterwave(this IServiceCollection services, Action<FlutterwaveOptions> configure = null)
        {
            if (configure != null)
                services.Configure(configure);

            services.AddTransient<IPaymentProcessor, FlutterwaveProcessor>();
            services.AddHttpClient(nameof(FlutterwaveProcessor))
                    .ConfigurePrimaryHttpMessageHandler(messageHandler =>
                    {
                        var handler = new HttpClientHandler();
                        if (handler.SupportsAutomaticDecompression)
                        {
                            handler.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                        }
                        return handler;
                    });
        }

        public static void AddRecaptcha(this IServiceCollection services, Action<RecaptchaSettings> configure = null)
        {
            if (configure != null)
                services.Configure(configure);

            services.AddTransient<RecaptchaService>();
        }
    }
}
