using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace Neimart.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.ConfigureAppConfiguration(ConfigureAppConfiguration);
                    webBuilder.ConfigureLogging(ConfigureLogging);
                }).UseSerilog();

        private static void ConfigureAppConfiguration(WebHostBuilderContext hostingContext, IConfigurationBuilder configBuilder)
        {
            var hostingEnvironment = hostingContext.HostingEnvironment;
            var loggerConfiguration = new LoggerConfiguration()
                       .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                       .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                       .MinimumLevel.Override("Microsoft.AspNetCore.Identity", LogEventLevel.Error)
                       .Enrich.FromLogContext()
                       .Enrich.WithProperty("ApplicationName", typeof(Program).Assembly.GetName().Name)
                       .Enrich.WithProperty("Environment", hostingContext.HostingEnvironment)
                       .WriteTo.RollingFile("logs\\log-{Date}.txt");

            if (hostingEnvironment.IsDevelopment())
                loggerConfiguration.WriteTo.ColoredConsole();

            Log.Logger = loggerConfiguration.CreateLogger();
        }

        private static void ConfigureLogging(WebHostBuilderContext hostingContext, ILoggingBuilder loggingBuilder)
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.AddSerilog();
        }
    }
}
