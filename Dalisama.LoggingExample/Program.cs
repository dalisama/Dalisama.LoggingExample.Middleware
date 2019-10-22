using Dalisama.LoggingExample;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Formatting.Json;
using System;
using System.IO;

namespace Dalisama.LoggingExample
{
    public class Program
    {
        public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appSettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        public static void Main(string[] args)
        {
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                Log.Logger = new LoggerConfiguration()
                                .ReadFrom.Configuration(Configuration)
                                .WriteTo.File(new JsonFormatter(), @"logs.json", shared: true).WriteTo.Console(new JsonFormatter())
                                .CreateLogger();
            }
            else
            {
                Log.Logger = new LoggerConfiguration()
                                .ReadFrom.Configuration(Configuration)
                                .WriteTo.File(new JsonFormatter(), @"logs.json", shared: true)
                                .CreateLogger();
            }


            try
            {
                Log.Information("Starting web host");
                CreateWebHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseSerilog();
    }
}
