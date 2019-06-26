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
				};

				var h = new WebHostBuilder()
					.UseSetting(nameof(WebHostBuilderIISExtensions.UseIISIntegration), false.ToString())
					.ConfigureAppConfiguration (config => config.AddCommandLine (args))
			        .UseKestrel()
			        .UseStartup<TestHarnessStartup>()
			        .UseUrls("http://localhost:9300")
			        .Build();
				host = h;
				Task.Run (() => { host.Run (); });

				//FIXME implement this using socket polling so it's faster
				Thread.Sleep (1000);
				Console.WriteLine ("WebServer Ready!");
			}
		}
	}
	class ChromeDebuggingSession {
		string page_name;
		TaskCompletionSource<bool> all_done = new TaskCompletionSource<bool> ();

		public ChromeDebuggingSession (string page_name)
		{
			this.page_name = page_name;
		}

		public void Start ()
		{
			Task.Run (() => { Launch(); });
		}

		static string GuessChromePath() {
			string chrome_path = null;
			if (File.Exists ("/Applications/Google Chrome Canary.app/Contents/MacOS/Google Chrome Canary"))
			chrome_path = "/Applications/Google Chrome Canary.app/Contents/MacOS/Google Chrome Canary";
			else if (File.Exists ("/Applications/Google Chrome.app/Contents/MacOS/Google Chrome"))
				chrome_path = "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome";
			if (chrome_path == null)
				throw new Exception ("Could not find an installed Chrome to use");
			return chrome_path;
		}

		void StageTwoLaunch (string wsURL)
		{
			System.Console.WriteLine("StageTwoLaunch");
			//http://localhost:9222/devtools/inspector.html?ws:localhost:9300/devtools/page/43F58580226BB03ADB1955C9F0CF6BC9

			//http://localhost:9222/devtools/inspector.html?ws=localhost:9300/devtools/page/C6E72B1B4E31893D99CF0448B7B56F11
			var injected_url = wsURL.Replace ("://localhost:9222", "=localhost:9300");
			var debugger_url = $"http://localhost:9222/devtools/inspector.html?{injected_url}";
			Console.WriteLine("launching debugger pointing at {0}", debugger_url);
			var psi = new ProcessStartInfo ();
			psi.Arguments = $"--new-window --no-first-run --user-data-dir=ho --window-size=850,1000 --window-position=600,0 {debugger_url}";
			psi.FileName = GuessChromePath ();
			psi.UseShellExecute = false;
			psi.RedirectStandardError = true;
			psi.RedirectStandardOutput = true;

			var proc = Process.Start (psi);
			try {
				proc.ErrorDataReceived += (sender, e) => {
					var str = e.Data;
					Console.WriteLine ($"dbg_cli: {str}");
				};
				proc.OutputDataReceived += (sender, e) => {
					Console.WriteLine ($"dbg_cli: {e.Data}");
				};
				proc.BeginErrorReadLine ();
				proc.BeginOutputReadLine ();
				var _dummy = all_done.Task.Result;
			} finally {
				proc.CancelErrorRead ();
				proc.CancelOutputRead ();
				proc.Kill ();
				proc.WaitForExit ();
				proc.Close ();
			}
		}

		void Launch()
		{
			//FIXME implement this using socket polling so it's faster
			Thread.Sleep (1000);
			Console.WriteLine ("proxy server is ready");
			var page_to_debug = $"http://localhost:9300/app/{page_name}";
			Console.WriteLine ("launching chrome to debug {0}", page_to_debug);

			var psi = new ProcessStartInfo ();
			psi.Arguments = $"--new-window --remote-debugging-port=9222 --no-first-run --user-data-dir={Path.GetTempPath ()} --window-size=600,1000 --window-position=0,0 {page_to_debug}";
			psi.FileName = GuessChromePath ();
			psi.UseShellExecute = false;
			psi.RedirectStandardError = true;
			psi.RedirectStandardOutput = true;

			var proc = Process.Start (psi);
			try {
				proc.ErrorDataReceived += (sender, e) => {
					var str = e.Data;
					Console.WriteLine ($"page_cli: {str}");
					//We wait for it as a signal that chrome finished launching
					if (!str.StartsWith ("DevTools listening on ", StringComparison.Ordinal))
						return;

					var client = new HttpClient ();
					var res = client.GetStringAsync ("http://localhost:9222/json/list").Result;
					Console.WriteLine ("res is {0}", res);
					var obj = JArray.Parse (res);
					var wsURL = obj? [0]? ["webSocketDebuggerUrl"]?.Value<string> ();
					Console.WriteLine (">>> {0}", wsURL);
					StageTwoLaunch (wsURL);
				};
				proc.OutputDataReceived += (sender, e) => {
					Console.WriteLine ($"page_cli: {e.Data}");
				};
				proc.BeginErrorReadLine ();
				proc.BeginOutputReadLine ();
				var _dummy = all_done.Task.Result;
			} finally {
				proc.CancelErrorRead ();
				proc.CancelOutputRead ();
				proc.Kill ();
				proc.WaitForExit ();
				proc.Close ();
			}
		}

	}
}
