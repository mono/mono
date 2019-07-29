using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.StaticFiles;
using Newtonsoft.Json.Linq;
using WsProxy;


namespace WsProxy {
	public class TestHarnessStartup {
		public TestHarnessStartup (IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; set; }

		// This method gets called by the runtime. Use this method to add services to the container.
		// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
		public void ConfigureServices (IServiceCollection services)
		{
			services.AddRouting ()
				.Configure<TestHarnessOptions> (Configuration);
		}

		async Task SendNodeVersion (HttpContext context)
		{
			Console.WriteLine ("hello chrome! json/version");
			var resp_obj = new JObject ();
			resp_obj ["Browser"] = "node.js/v9.11.1";
			resp_obj ["Protocol-Version"] = "1.1";

			var response = resp_obj.ToString ();
			await context.Response.WriteAsync (response, new CancellationTokenSource ().Token);
		}

		async Task SendNodeList (HttpContext context)
		{
			Console.WriteLine ("hello chrome! json/list");
			try {
				var response = new JArray (JObject.FromObject (new {
					description = "node.js instance",
					devtoolsFrontendUrl = "chrome-devtools://devtools/bundled/inspector.html?experiments=true&v8only=true&ws=localhost:9300/91d87807-8a81-4f49-878c-a5604103b0a4",
					faviconUrl = "https://nodejs.org/static/favicon.ico",
					id = "91d87807-8a81-4f49-878c-a5604103b0a4",
					title = "foo.js",
					type = "node",
					webSocketDebuggerUrl = "ws://localhost:9300/91d87807-8a81-4f49-878c-a5604103b0a4"
				})).ToString ();

				Console.WriteLine ($"sending: {response}");
				await context.Response.WriteAsync (response, new CancellationTokenSource ().Token);
			} catch (Exception e) { Console.WriteLine (e); }
		}

		public async Task LaunchAndServe (ProcessStartInfo psi, HttpContext context, Func<string, string> extract_conn_url)
		{
			if (!context.WebSockets.IsWebSocketRequest) {
				context.Response.StatusCode = 400;
				return;
			}

			var tcs = new TaskCompletionSource<string> ();

			var proc = Process.Start (psi);
			try {
				proc.ErrorDataReceived += (sender, e) => {
					Console.WriteLine ($"stderr: {e.Data}");
					var res = extract_conn_url (e.Data);
					if (res != null)
						tcs.TrySetResult (res);
				};
				proc.OutputDataReceived += (sender, e) => {
					Console.WriteLine ($"stdout: {e.Data}");
				};
				proc.BeginErrorReadLine ();
				proc.BeginOutputReadLine ();

				if (await Task.WhenAny (tcs.Task, Task.Delay (2000)) != tcs.Task) {
					Console.WriteLine ("Didnt get the con string after 2s.");
					throw new Exception ("node.js timedout");
				}
				var con_str = await tcs.Task;
				Console.WriteLine ($"lauching proxy for {con_str}");

				var proxy = new MonoProxy ();
				var browserUri = new Uri (con_str);
				var ideSocket = await context.WebSockets.AcceptWebSocketAsync ();

				await proxy.Run (browserUri, ideSocket);
				Console.WriteLine("Proxy done");
			} catch (Exception e) {
				Console.WriteLine ("got exception {0}", e.GetType ().FullName);
			} finally {
				proc.CancelErrorRead ();
				proc.CancelOutputRead ();
				proc.Kill ();
				proc.WaitForExit ();
				proc.Close ();
			}
		}
		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure (IApplicationBuilder app, IOptionsMonitor<TestHarnessOptions> optionsAccessor, IHostingEnvironment env)
		{
			app.UseWebSockets ();
			app.UseStaticFiles ();

			TestHarnessOptions options = optionsAccessor.CurrentValue;
			Console.WriteLine ($"Chrome from: '{options.ChromePath}'");
			Console.WriteLine ($"Files from: '{options.AppPath}'");
			Console.WriteLine ($"Using page : '{options.PagePath}'");

			var provider = new FileExtensionContentTypeProvider();
			provider.Mappings [".wasm"] = "application/wasm";

			app.UseStaticFiles (new StaticFileOptions {
				FileProvider = new PhysicalFileProvider (options.AppPath),
				ServeUnknownFileTypes = true, //Cuz .wasm is not a known file type :cry:
				RequestPath = "",
				ContentTypeProvider = provider
			});

			var devToolsUrl = options.DevToolsUrl;
			var psi = new ProcessStartInfo ();
			psi.Arguments = $"--headless --disable-gpu --remote-debugging-port={devToolsUrl.Port} http://localhost:9300/{options.PagePath}";
			psi.UseShellExecute = false;
			psi.FileName = options.ChromePath;
			psi.RedirectStandardError = true;
			psi.RedirectStandardOutput = true;

			app.UseRouter (router => {
				router.MapGet ("launch-chrome-and-connect", async context => {
					Console.WriteLine ("New test request");
					await LaunchAndServe (psi, context, str => {
						//We wait for it as a signal that chrome finished launching
						if (!str.StartsWith ("DevTools listening on ", StringComparison.Ordinal))
							return null;

						var client = new HttpClient ();
						var res = client.GetStringAsync (new Uri (devToolsUrl, "/json/list")).Result;
						Console.WriteLine ("res is {0}", res);
						if (res == null)
							return null;

						var obj = JArray.Parse (res);
						if (obj == null || obj.Count < 1)
							return null;

						var wsURl = obj[0]? ["webSocketDebuggerUrl"]?.Value<string> ();
						Console.WriteLine (">>> {0}", wsURl);

						return wsURl;
					});
				});
			});

			if (options.NodeApp != null) {
				Console.WriteLine($"Doing the nodejs: {options.NodeApp}");
				var nodeFullPath = Path.GetFullPath (options.NodeApp);
				Console.WriteLine (nodeFullPath);

				psi.Arguments = $"--inspect-brk=localhost:0 {nodeFullPath}";
				psi.FileName = "node";

				app.UseRouter (router => {
					//Inspector API for using chrome devtools directly
					router.MapGet ("json", SendNodeList);
					router.MapGet ("json/list", SendNodeList);
					router.MapGet ("json/version", SendNodeVersion);
					router.MapGet ("launch-done-and-connect", async context => {
						await LaunchAndServe (psi, context, str => {
							if (str.StartsWith ("Debugger listening on", StringComparison.Ordinal))
								return str.Substring (str.IndexOf ("ws://", StringComparison.Ordinal));
							return null;
						});
					});
				});
			}
		}
	}
}
