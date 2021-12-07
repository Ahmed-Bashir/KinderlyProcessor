using KinderlyProcessor.Core;
using KinderlyProcessor.Core.Interfaces;
using KinderlyProcessor.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;
using System.Text;
using Topshelf;

namespace KinderlyProcessor
{
    public class Program
    {
        private static void Main()
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            var services = ConfigureServices();
            var serviceProvider = services.BuildServiceProvider();
            var path = AppDomain.CurrentDomain.BaseDirectory + "KinderlyLog.txt";

            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File(path)
                .CreateLogger();

            Log.Information("Application starting");

            var topshelfExitCode = HostFactory.Run(configure =>
            {
                configure.Service<Application>(x =>
                {
                    x.ConstructUsing(name => serviceProvider.GetService<Application>());
                    x.WhenStarted(application => application.Start());
                    x.WhenStopped(application => application.Stop());
                });

                configure.RunAsLocalSystem();
                configure.SetDisplayName("KinderlyProcessor");
                configure.SetServiceName("KinderlyProcessor");
                configure.SetDescription("Provides Pacey members with Kinderdly");
            });

            var exitCode = (int)Convert.ChangeType(topshelfExitCode, topshelfExitCode.GetTypeCode());
            Environment.ExitCode = exitCode;
        }

        private static IServiceCollection ConfigureServices()
        {
            var config = LoadConfiguration();

            var services = new ServiceCollection();
            services.AddSingleton(config);
            services.AddLogging(configure => configure.AddSerilog());

            services.AddHttpClient("BluelightApi", client =>
            {
                client.BaseAddress = new Uri(config.GetRequiredSection("BluelightApiUrl").Value);
                var authToken = Encoding.ASCII.GetBytes($"{config.GetRequiredSection("BluelightUserName")}:{config.GetRequiredSection("BluelightPassword")}");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authToken));
                var token = client.GetStringAsync(config.GetRequiredSection("BluelightTokenUrl").Value).GetAwaiter().GetResult();
                token = token.Substring(1, token.Length - 2);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            });

            services.AddHttpClient("KinderlyApi", client =>
            {
                client.BaseAddress = new Uri(config.GetRequiredSection("KinderlyApiUrl").Value);
                var authToken = Encoding.ASCII.GetBytes($"{config.GetRequiredSection("KinderlyUserName")}:{config.GetRequiredSection("KinderlyPassword")}");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authToken));
            });

            services.AddHttpClient("DigitalContractApi", client =>
            {
                var environment = config.GetRequiredSection("BluelightEnvironment").Value;
                client.BaseAddress = new Uri(config.GetRequiredSection("DigitalContractApiUrl").Value);

                if (environment.Equals("Live", StringComparison.InvariantCultureIgnoreCase))
                {
                    // Production.
                    var authToken = Encoding.ASCII.GetBytes($"{config.GetRequiredSection("DigitalUserName")}:{config.GetRequiredSection("DigitalPassword")}");
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authToken));
                }
                else
                {
                    // Dev.
                    client.BaseAddress = new Uri(config.GetRequiredSection("DigitalContractApiUrl").Value);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", config.GetRequiredSection("DigitalContractToken").Value);
                }
            });

            services.Configure<Dictionary<string, string>>(config.GetSection("Products"));
            services.Configure<SmtpSetting>(config.GetSection("SmtpSettings"));
            services.AddTransient<IBluelightApiService, BluelightApiService>();
            services.AddTransient<IKinderlyApiService, KinderlyApiService>();
            services.AddTransient<Application>();
            services.AddTransient<IEmailSercive, EmailService>();
            services.AddHttpClient();
            services.AddOptions();
            return services;
        }

        public static IConfiguration LoadConfiguration()
            => new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", false, true).Build();
    }
}
