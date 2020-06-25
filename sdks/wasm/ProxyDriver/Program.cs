using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace WebAssembly.Net.Debugging
{
	public class ProxyOptions {
		public Uri DevToolsUrl { get; set; } = new Uri ("http://localhost:9222");
	}

	public class TestHarnessOptions : ProxyOptions {
		public string ChromePath { get; set; }
		public string AppPath { get; set; }
		public string PagePath { get; set; }
		public string NodeApp { get; set; }
		public string ChromeRevision { get; set; }
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
				.UseUrls ("http://localhost:9300")
				.Build ();

			host.Run ();
		}
	}

	public class TestHarnessProxy {
		static IWebHost host;
		static Task hostTask;
		static CancellationTokenSource cts = new CancellationTokenSource ();
		static object proxyLock = new object ();

		static Uri _webserverUri = null;
		public static Uri Endpoint {
			get {
				if (_webserverUri == null)
					throw new ArgumentException ("Can't use WebServer Uri before it is set, since it is bound dynamically.");

				return _webserverUri;
			}
			set { _webserverUri = value; }
		}

		public static Task Start (string chromePath, string appPath, string pagePath, string chromeRevision = null)
		{
			lock (proxyLock) {
				if (hostTask != null)
					return hostTask;

				hostTask = Start ();
			}

			Console.WriteLine ("WebServer Ready!");
			return hostTask;

			async Task Start ()
			{
				if (chromePath == null || chromeRevision != null) {
					try {
						chromePath = await PuppeteerHelper.ProvisionChrome (chromeRevision).ConfigureAwait (false);
					} catch (Exception ex) {
						Environment.FailFast ($"Failed to auto-provision Chrome: {ex}");
					}
				}

				host = WebHost.CreateDefaultBuilder ()
					.UseSetting ("UseIISIntegration", false.ToString ())
					.ConfigureAppConfiguration ((hostingContext, config) => {
						config.AddEnvironmentVariables (prefix: "WASM_TESTS_");
					})
					.ConfigureServices ((ctx, services) => {
						services.Configure<TestHarnessOptions> (ctx.Configuration);
						services.Configure<TestHarnessOptions> (options => {
							options.ChromePath = options.ChromePath ?? chromePath;
							options.AppPath = appPath;
							options.PagePath = pagePath;
							options.DevToolsUrl = new Uri ("http://localhost:0");
							options.ChromeRevision = chromeRevision;
						});
					})
					.UseStartup<TestHarnessStartup> ()
					.UseUrls ("http://127.0.0.1:0")
					.Build ();

				await host.StartAsync (cts.Token).ConfigureAwait (false);
			}
		}
	}
}
