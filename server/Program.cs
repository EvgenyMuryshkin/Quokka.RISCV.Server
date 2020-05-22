using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace svr
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine($"PID: {Process.GetCurrentProcess().Id}");
            Console.WriteLine($"Path: {Environment.GetEnvironmentVariable("PATH")}");

            var port = args.FirstOrDefault() ?? "15000";

            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseStartup<Startup>()
                        .UseUrls($"http://0.0.0.0:{port}/");
                })
                .Build()
                .Run();
        }
    }
}
