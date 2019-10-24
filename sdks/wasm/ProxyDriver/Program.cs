using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace WsProxy
{
	public class ProxyOptions {
		public Uri DevToolsUrl { get; set; } = new Uri ("http://localhost:9222");
	}

	public class TestHarnessOptions : ProxyOptions {
		public string ChromePath { get; set; }
		public string AppPath { get; set; }
		public string PagePath { get; set; }
		public string NodeApp { get; set; }
	}

	public class Program {
		public static void Main(string[] args)
		{
			var host = new WebHostBuilder()
				.UseSetting ("UseIISIntegration", false.ToString ())
				.UseKestrel ()
				.UseContentRoot (Directory.GetCurrentDirectory())
				.UseStartup<Startup> ()
				.ConfigureAppConfiguration ((hostingContext, config) =>
				{
					config.AddCommandLine(args);
				})
				.UseDebugProxy ()
				.Build ();

			host.Run ();
		}
	}

	public static class MonoProxyExtensions {
		public static IWebHostBuilder UseDebugProxy (this IWebHostBuilder host) =>
			host.UseUrls ("http://localhost:9300");
	}

	public class TestHarnessProxy {
		static IWebHost host;

		public static void Start (string chromePath, string appPath, string pagePath)
		{
			lock (typeof (TestHarnessProxy)) {
				if (host != null)
					return;

				var h = new WebHostBuilder()
					.UseSetting ("UseIISIntegration", false.ToString ())
					.ConfigureServices (services => {
						services.Configure<TestHarnessOptions> (options => {
							options.ChromePath = chromePath;
							options.AppPath = appPath;
							options.PagePath = pagePath;
							options.DevToolsUrl = new Uri ("http://localhost:9333");
						});
					})
					.UseKestrel ()
					.UseStartup<TestHarnessStartup> ()
					.UseDebugProxy ()
					.Build();

				host = h;
				Task.Run (() => { host.Run (); });

				//FIXME implement this using socket polling so it's faster
				Thread.Sleep (1000);
				Console.WriteLine ("WebServer Ready!");
			}
		}
	}
}
