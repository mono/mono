using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace WsProxy
{
    public class Program
    {
        public static void Main(string[] args)
        {
		    var host = new WebHostBuilder()
		        .UseSetting(nameof(WebHostBuilderIISExtensions.UseIISIntegration), false.ToString())
		        .UseKestrel()
		        .UseContentRoot(Directory.GetCurrentDirectory())
		        .UseStartup<Startup>()
		        .UseUrls("http://localhost:9300")
		        .Build();

		    host.Run();
        }
    }
}
