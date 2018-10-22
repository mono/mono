using System;
using System.Linq;
using System.Threading.Tasks;

using System.Net.WebSockets;
using System.Threading;
using System.IO;
using System.Text;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;
using Xunit;
using Mono.WebAssembly;

namespace DebuggerTests
{
	public class SourceList : DebuggerTestBase {
		Dictionary<string, string> SubscribeToScripts (Inspector insp) {
			Dictionary<string, string> scripts = new Dictionary<string, string> ();
			insp.On("Debugger.scriptParsed", async (args, c) => {
				var script_id = args? ["scriptId"]?.Value<string> ();
				var url = args["url"]?.Value<string> ();
				if (script_id.StartsWith("dotnet://"))
					scripts [script_id] = url;
				await Task.FromResult (0);
			});
			return scripts;
		}

		[Fact]
		public async Task CheckThatAllSourcesAreSent () {
			var insp = new Inspector ();

			//Collect events
			var scripts = SubscribeToScripts(insp);

			//all sources are sent before runtime ready is sent, nothing to check
			await insp.Ready ();
			Assert.Equal (3, scripts.Count);
			Assert.True (scripts.ContainsValue ("dotnet://debugger-test.dll/debugger-test.cs"));
			Assert.True (scripts.ContainsValue ("dotnet://debugger-test.dll/debugger-test2.cs"));
			Assert.True (scripts.ContainsValue ("dotnet://Simple.Dependency.dll/dependency.cs"));
		}

		[Fact]
		public async Task CreateGoodBreakpoint () {
			var insp = new Inspector ();

			//Collect events
			var scripts = SubscribeToScripts(insp);

			await insp.Ready (async (cli, token) => {
				var bp1_req = JObject.FromObject(new {
					lineNumber = 5,
					columnNumber = 2,
					url = "dotnet://debugger-test.dll/debugger-test.cs",
				});

				var bp1_res = await cli.SendCommand ("Debugger.setBreakpointByUrl", bp1_req, token);

				Assert.True (bp1_res.IsOk);
				Assert.Equal ("dotnet:0", bp1_res.Value ["breakpointId"]);
				Assert.Equal (1, bp1_res.Value ["locations"]?.Value<JArray> ()?.Count);

				var loc = bp1_res.Value ["locations"]?.Value<JArray> ()[0];

				Assert.NotNull (loc ["scriptId"]);
				Assert.Equal ("dotnet://debugger-test.dll/debugger-test.cs", scripts [loc["scriptId"]?.Value<string> ()]);
				Assert.Equal (5, loc ["lineNumber"]);
				Assert.Equal (2, loc ["columnNumber"]);
			});
		}

		[Fact]
		public async Task CreateBadBreakpoint () {
			var insp = new Inspector ();

			//Collect events
			var scripts = SubscribeToScripts(insp);

			await insp.Ready (async (cli, token) => {
				var bp1_req = JObject.FromObject(new {
					lineNumber = 5,
					columnNumber = 2,
					url = "dotnet://debugger-test.dll/this-file-doesnt-exist.cs",
				});

				var bp1_res = await cli.SendCommand ("Debugger.setBreakpointByUrl", bp1_req, token);

				Assert.False (bp1_res.IsOk);
				Assert.Equal ((int)MonoErrorCodes.BpNotFound, bp1_res.Error ["code"]?.Value<int> ());
			});
		}
	}
}