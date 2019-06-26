using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using System.Net.WebSockets;
using System.Net.Http;
using System.Threading;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using System.Net;

namespace WsProxy {
	internal class Startup {
		// This method gets called by the runtime. Use this method to add services to the container.
		// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
		public void ConfigureServices (IServiceCollection services)
			=> services.AddRouting ()
				.Configure<ProxyOptions> (Configuration);

		public Startup (IConfiguration configuration)
			=> Configuration = configuration;

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure (IApplicationBuilder app, IOptionsMonitor<ProxyOptions> optionsAccessor, IHostingEnvironment env)
		{
			var options  = 	optionsAccessor.CurrentValue;
			app.UseDeveloperExceptionPage ()
				.UseWebSockets ()
				.UseDebugProxy (options);
		}
	}

	static class DebugExtensions {
		public static DevToolsTab DefaultTabMapper (DevToolsTab tab, HttpContext context, Uri debuggerHost)
		{
			var request = context.Request;
			var frontendUrl = $"{debuggerHost.Scheme}://{debuggerHost.Authority}{tab.devtoolsFrontendUrl.Replace ($"ws={debuggerHost.Authority}", $"ws={request.Host}")}";
			var debuggerPage = new Uri (tab.webSocketDebuggerUrl);
			var proxyDebuggerUrl = $"{debuggerPage.Scheme}://{request.Host}{debuggerPage.PathAndQuery}";
			
			return new DevToolsTab {
				description = tab.description,
				devtoolsFrontendUrl = frontendUrl,
				id = tab.id,
				title = tab.title,
				type = tab.type,
				url = tab.url,
				webSocketDebuggerUrl = proxyDebuggerUrl,
				faviconUrl = tab.faviconUrl
			};
		}

		public static IApplicationBuilder UseDebugProxy (this IApplicationBuilder app, ProxyOptions options)
			=> UseDebugProxy (app, options, DefaultTabMapper);
		
		public static IApplicationBuilder UseDebugProxy (this IApplicationBuilder app, ProxyOptions options, Func<DevToolsTab, HttpContext, Uri, DevToolsTab> rewriteFunc)
		{
			var devToolsHost = new Uri (options.DevToolsUrl);
			app.Use (async (context, next) => {
				var request = context.Request;

				var requestPath = request.Path;
				var endpoint = $"{devToolsHost.Scheme}://{devToolsHost.Authority}{request.Path}{request.QueryString}";
				
				switch (requestPath.Value.ToLower (System.Globalization.CultureInfo.InvariantCulture)) {
					case "/":
						using (var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds (5) }) {
							var response = await httpClient.GetStringAsync ($"{devToolsHost}");
							context.Response.ContentType = "text/html";
							context.Response.ContentLength = response.Length;
							await context.Response.WriteAsync (response);
						}
						break;
					case "/json/version":
						context.Response.ContentType = "application/json";
						await context.Response.WriteAsync (JsonConvert.SerializeObject (
							new Dictionary<string, string> {
								{ "Browser", "Chrome/71.0.3578.98" },
								{ "Protocol-Version", "1.3" },
								{ "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/71.0.3578.98 Safari/537.36" },
								{ "V8-Version", "7.1.302.31" },
								{ "WebKit-Version", "537.36 (@15234034d19b85dcd9a03b164ae89d04145d8368)" },
							}));
						break;
					case "/json/new":
						var tab = await ProxyGetJsonAsync <DevToolsTab> (endpoint);
						var redirect = rewriteFunc?.Invoke (tab, context, devToolsHost);
						context.Response.ContentType = "application/json";
						await context.Response.WriteAsync (JsonConvert.SerializeObject (redirect));
						break;
					case "/json/list":
					case "/json":
						var tabs = await ProxyGetJsonAsync <DevToolsTab[]>(endpoint);
						var alteredTabs = tabs.Select (t => rewriteFunc?.Invoke (t, context, devToolsHost)).ToArray();
						context.Response.ContentType = "application/json";
						await context.Response.WriteAsync (JsonConvert.SerializeObject (alteredTabs));
						break;
					default:
						await next();
						break;
				}
			})
			.UseRouter (router => {
					router.MapGet ("devtools/page/{pageId}", async context => {
						if (!context.WebSockets.IsWebSocketRequest) {
							context.Response.StatusCode = 400;
							return;
						}

						var endpoint = new Uri ($"ws://{devToolsHost.Authority}{context.Request.Path.ToString()}");
						try {
							var proxy = new MonoProxy ();
							var ideSocket = await context.WebSockets.AcceptWebSocketAsync ();

							await proxy.Run (endpoint, ideSocket);
						} catch (Exception e) {
							Console.WriteLine ("got exception {0}", e);
						}
					});
				});
			return app;
		}

		public class DevToolsTab {
			public string description { get; set; }
			public string id { get; set; }
			public string type { get; set; }
			public string url { get; set; }
			public string title { get; set; }
			public string devtoolsFrontendUrl { get; set; }
			public string webSocketDebuggerUrl { get; set; }
			public string faviconUrl { get; set; }
		}

		private static async Task<T> ProxyGetJsonAsync<T> (string url)
		{
			using (var httpClient = new HttpClient ()) {
				var response = await httpClient.GetAsync (url);
				var jsonResponse = await response.Content.ReadAsStringAsync ();
				return JsonConvert.DeserializeObject<T> (jsonResponse);
			}
		}
	}
}
