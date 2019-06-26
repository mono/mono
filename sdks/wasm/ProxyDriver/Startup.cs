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
		public void ConfigureServices (IServiceCollection services) =>
			services.AddRouting ()
				.Configure<ProxyOptions> (Configuration);

		public Startup (IConfiguration configuration) =>
			Configuration = configuration;

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
		public static Dictionary<string,string> MapValues (Dictionary<string,string> response, HttpContext context, Uri debuggerHost)
		{
			var filtered = new Dictionary<string, string> ();
			var request = context.Request;

			foreach (var key in response.Keys) {
				switch (key) {
				case "devtoolsFrontendUrl":
					var front = response [key];
					filtered[key] = $"{debuggerHost.Scheme}://{debuggerHost.Authority}{front.Replace ($"ws={debuggerHost.Authority}", $"ws={request.Host}")}";
					break;
				case "webSocketDebuggerUrl":
					var page = new Uri (response [key]);
					filtered [key] = $"{page.Scheme}://{request.Host}{page.PathAndQuery}";
					break;
				default:
					filtered [key] = response [key];
					break;
				}
			}
			return filtered;
		}

		public static IApplicationBuilder UseDebugProxy (this IApplicationBuilder app, ProxyOptions options) =>
			UseDebugProxy (app, options, MapValues);
		
		public static IApplicationBuilder UseDebugProxy (
			this IApplicationBuilder app,
			ProxyOptions options,
			Func<Dictionary<string,string>, HttpContext, Uri, Dictionary<string,string>> mapFunc)
		{
			var devToolsHost = options.DevToolsUrl;
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
					case "/json/new":
						var version = await ProxyGetJsonAsync<Dictionary<string,string>> (endpoint);
						context.Response.ContentType = "application/json";
						await context.Response.WriteAsync (
							JsonConvert.SerializeObject (mapFunc (version, context, devToolsHost)));
						break;
					case "/json/list":
					case "/json":
						var tabs = await ProxyGetJsonAsync <Dictionary<string, string>[]> (endpoint);
						var alteredTabs = tabs.Select (t => mapFunc (t, context, devToolsHost)).ToArray();
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
