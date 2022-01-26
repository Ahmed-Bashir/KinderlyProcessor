using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using KinderlyProcessor.Interfaces;
using KinderlyProcessor.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Topshelf;

namespace KinderlyProcessor
{
    public class Program
    {
        //private readonly IKinderlyApiService _kinderlyApiService;
        //private readonly IBluelightApiService _bluelightApiService;

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


        public Program()
        {

        }

        //public Program(IKinderlyApiService kinderlyApiService, IBluelightApiService bluelightApiService)
        //{
        //    _kinderlyApiService = kinderlyApiService;
        //    _bluelightApiService = bluelightApiService;
        //}



        //static async Task Main(string[] args)
        //{

        //    var services = ConfigureServices();
        //    var serviceProvider = services.BuildServiceProvider();

        //    var config = LoadConfiguration();

        //    var path = AppDomain.CurrentDomain.BaseDirectory + "KinderlyLog.txt";

        //    Log.Logger = new LoggerConfiguration()
        //        .Enrich.FromLogContext()
        //        .WriteTo.Console()
        //        .WriteTo.File(path)
        //        .CreateLogger();

        //    Log.Information("Application starting");

        //    //calls the Run method in App, which is replacing Main
        //    await serviceProvider.GetService<Program>().Run();

        //}

        private static IServiceCollection ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();

            var config = LoadConfiguration();
            services.AddSingleton(config);

            // Required to run the application.
            services.AddTransient<IBluelightApiService, BluelightApiService>();
            services.AddTransient<IKinderlyApiService, KinderlyApiService>();
            services.AddTransient<Application>();
            services.AddTransient<IEmailSercive, EmailService>();
            services.AddHttpClient();
            services.AddOptions();

            var appsettings = config.GetRequiredSection("Environment").Value.ToLower();

            appsettings = appsettings.Equals("live") ? "appsettingsLive" : "appsettingsDev";



            services.AddHttpClient("BluelightApi", client =>
            {
                client.BaseAddress = new Uri(config.GetValue<string>($"{appsettings}:BluelightApiUrl"));
                

                var authToken = Encoding.ASCII.GetBytes($"{config.GetValue<string>($"{appsettings}:BluelightUserName")}:{config.GetValue<string>($"{appsettings}:BluelightPassword")}");


                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authToken));
                var token = client.GetStringAsync(config.GetValue<string>($"{appsettings}:BluelightTokenUrl")).Result;



                token = token.Substring(1, token.Length - 2);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);


            });

            services.AddHttpClient("KinderlyApi", client =>
            {
                client.BaseAddress = new Uri(config.GetValue<string>($"{appsettings}:KinderlyApiUrl"));
                var authToken = Encoding.ASCII.GetBytes($"{config.GetValue<string>($"{appsettings}:KinderlyUserName")}:{config.GetValue<string>($"{appsettings}:KinderlyPassword")}");

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authToken));
            });

            services.AddHttpClient("DigitalContractApi", client =>
            {
                if (appsettings.Equals("appsettingsLive"))
                {

                    client.BaseAddress = new Uri(config.GetValue<string>($"{appsettings}:DigitalContractApiUrl"));
                    var authToken = Encoding.ASCII.GetBytes($"{config.GetValue<string>($"{appsettings}:DigitalUserName")}:{config.GetValue<string>($"{appsettings}:DigitalPassword")}");

                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authToken));

                }
                else
                {

                    client.BaseAddress = new Uri(config.GetValue<string>($"{appsettings}:DigitalContractApiUrl"));

                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", config.GetValue<string>($"{appsettings}:DigitalContractToken"));

                }
            });

           

            services.Configure<SmtpSetting>(config.GetSection(
                                     "smtpsettings"));

            services.AddLogging(configure => configure.AddSerilog());

            services.Configure<Dictionary<string, string>>(config.GetSection("Products"));


            return services;
        }

        public static IConfiguration LoadConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);



            return builder.Build();
        }


        //    public async Task Run()
        //    {

        //        //if (await _bluelightApiService.GetMembershipsAsync() != null)
        //        //{
        //        await _kinderlyApiService.SendApprovedPaceyMembersAsync();
        //        //}
        //        // await _bluelightApiService.GetContracts();

        //        //  await _kinderlyApiService.ProcessDigitalContractsAsync();


        //        //await emailService.SendUnApprovedMembers();

        //    }
    }
}