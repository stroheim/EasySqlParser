using System;
using System.IO;
using EasySqlParser.Configurations;
using EasySqlParser.SourceGeneratorSandbox.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace EasySqlParser.SourceGeneratorSandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            Log.Logger = CreateLogger();
            ConfigContainer.AddDefault(
                DbConnectionKind.SqlServer,
                () => new SqlParameter());
            ConfigContainer.EnableCache = true;
            CreateHostBuilder(args).Build().Run();
            Console.ReadKey();


            //var aaa = new HostBuilder()
            //    .ConfigureAppConfiguration((hostContext, configApp) =>
            //                               {

            //                               })
            //    .ConfigureServices(services =>
            //                       {

            //                       })
            //    .ConfigureLogging((context, b) =>
            //                      {

            //                      }).Build();
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureAppConfiguration((hostContext, configApp) =>
                                           {

                                           })
                .ConfigureServices(services =>
                                   {
                                       services.AddDbContext<EfContext>(options =>
                                                                        {
                                                                            options.UseSqlServer("");
                                                                        });
                                   })
                .ConfigureLogging((context, b) =>
                                  {

                                  });

        static ILogger CreateLogger() =>
            new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

        static IConfigurationBuilder CreateBuilder() =>
            new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory());
    }
}
