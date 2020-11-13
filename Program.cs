using System.IO;
using System.Net;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;

using Serilog;
using Serilog.Events;
using Microsoft.Extensions.Hosting;

using Schlauchboot.Hosting.SoundCloud.Manager.Methods;

namespace Schlauchboot.Hosting.SoundCloud
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var metaManager = new Meta();
            var tempFileStore = $"{metaManager.GetAssemblyPath()}\\TempFileStore";
            if (!Directory.Exists(tempFileStore)) { Directory.CreateDirectory(tempFileStore); }
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .UseSerilog((context, configuration) => configuration
                    .Enrich.FromLogContext()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                    .MinimumLevel.Debug()
                    .WriteTo.Console())
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseKestrel(kestrelOptions =>
                    {
                        kestrelOptions.Listen(IPAddress.Any, 443, listenOptions =>
                        {
                            listenOptions.UseHttps(Startup._configuration.GetSection("Meta").GetChildren()
                                    .Where(x => x.Key == "CertificatePath").FirstOrDefault().Value,
                                Startup._configuration.GetSection("Meta").GetChildren()
                                    .Where(x => x.Key == "CertificateKey").FirstOrDefault().Value);
                        });
                    });
                    webBuilder.UseStartup<Startup>();
                });
    }
}
