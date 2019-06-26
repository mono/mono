using System;
﻿using System.Net.Http;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Newtonsoft.Json.Linq;

namespace WsProxy
{
	public class ProxyOptions {
		public string DevToolsUrl { get; set; } = "http://localhost:9222";
	}

	public class TestHarnessOptions : ProxyOptions {
		public string ChromePath { get; set; }
		public string AppPath { get; set; }
		public string PagePath { get; set; }
	}

	public class Program
	{
		public static void Main(string[] args)
        {
			var host = new WebHostBuilder()
				.UseSetting (nameof(WebHostBuilderIISExtensions.UseIISIntegration), false.ToString())
				.UseKestrel ()
				.UseContentRoot (Directory.GetCurrentDirectory())
				.UseStartup<Startup> ()
				.UseDebugProxy ()
				.ConfigureAppConfiguration ((hostingContext, config) =>
				{
					config.AddCommandLine(args);
					//config.AddCommandLine(args, _switchMappings);
				})
				.Build ();

			host.Run ();
		}
	}

	public static class MonoProxyExtensions {
		public static IWebHostBuilder UseDebugProxy (this IWebHostBuilder host)
			=> host.UseUrls ("http://localhost:9300");
	}

	public class TestHarnessProxy {
		static IWebHost host;

		public static void Start (string chrome_path, string app_path, string page_path) {
			lock (typeof (TestHarnessProxy)) {
				if (host != null)
					return;

				//FIXME wtf ConfigureAppConfiguration
				string[] args = new [] {
					$"/ChromePath={chrome_path}",
					$"/AppPath={app_path}",
					$"/PagePath={page_path}",
					$"/DevToolsUrl=http://localhost:9333"
				};

				var h = new WebHostBuilder()
					.UseSetting(nameof(WebHostBuilderIISExtensions.UseIISIntegration), false.ToString())
					.ConfigureAppConfiguration (config => config.AddCommandLine (args))
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
