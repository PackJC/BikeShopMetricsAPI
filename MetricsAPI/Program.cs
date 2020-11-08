using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MetricsAPI.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MetricsAPI
{
    public class Program
    {
        
        public static List<UserAccessRights> ActiveUsers { get; set; }
        public static void Main(string[] args)
        {
            ActiveUsers = new List<UserAccessRights>();
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
