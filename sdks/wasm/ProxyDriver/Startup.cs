using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json.Linq;
using Mono.WebAssembly;


namespace WsProxy
{
	public class Startup {
		private readonly IConfiguration configuration;
		public Startup (IConfiguration configuration)
		{
			this.configuration = configuration;
		}

		// This method gets called by the runtime. Use this method to add services to the container.
		// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
		public void ConfigureServices (IServiceCollection services)
		{
			services.AddRouting ();
		}

		Uri GetBrowserUri (string path)
		{
			return new Uri ("ws://localhost:9222" + path);
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure (IApplicationBuilder app, IHostingEnvironment env)
		{
			app.UseWebSockets ();

			var debug_path = configuration ["Debug"];
			if (debug_path != null) {
				var serve_path = Path.GetDirectoryName (debug_path);
				Console.WriteLine ($"Configuring to server from {serve_path} under /app");
				app.UseStaticFiles (new StaticFileOptions {
					FileProvider = new PhysicalFileProvider (serve_path),
					ServeUnknownFileTypes = true, //Cuz .wasm is not a known file type :cry:
					RequestPath = "/app"
				});
			}

			app.UseRouter (router => {
				router.MapGet ("devtools/page/{pageId}", async context => {
					if (!context.WebSockets.IsWebSocketRequest) {
						context.Response.StatusCode = 400;
						return;
					}

					try {
						var proxy = new MonoProxy ();
						var browserUri = GetBrowserUri (context.Request.Path.ToString ());
						var ideSocket = await context.WebSockets.AcceptWebSocketAsync ();

						await proxy.Run (browserUri, ideSocket);
					} catch (Exception e) {
						Console.WriteLine ("got exception {0}", e);
					}
				});
			});
		}
	}
}
