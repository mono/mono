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

		void CheckLocation (string script_loc, int line, int column, Dictionary<string, string> scripts, JToken location)
		{
			Assert.Equal (script_loc, scripts [location ["scriptId"].Value<string> ()]);
			Assert.Equal (line, location ["lineNumber"].Value<int> ());
			Assert.Equal (column, location ["columnNumber"].Value<int> ());
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
				Assert.True (bp1_res.IsErr);
				Assert.Equal ((int)MonoErrorCodes.BpNotFound, bp1_res.Error ["code"]?.Value<int> ());
			});
		}

		[Fact]
		public async Task CreateGoodBreakpointAndHit () {
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

				var eval_req = JObject.FromObject(new {
					expression = "window.setTimeout(function() { invoke_add(); }, 1);",
				});

				var eval_res = await cli.SendCommand ("Runtime.evaluate", eval_req, token);
				Assert.True (eval_res.IsOk);

				var pause_location = await insp.WaitFor(Inspector.PAUSE);

				Assert.Equal ("other", pause_location ["reason"]?.Value<string> ());
				Assert.Equal ("dotnet:0", pause_location ["hitBreakpoints"]?[0]?.Value<string> ());

				var top_frame = pause_location ["callFrames"][0];

				Assert.Equal ("IntAdd", top_frame ["functionName"].Value<string>());
				Assert.Equal ("dotnet://debugger-test.dll/debugger-test.cs", top_frame ["url"].Value<string> ());

				CheckLocation ("dotnet://debugger-test.dll/debugger-test.cs", 4, 41, scripts, top_frame["functionLocation"]);
				CheckLocation ("dotnet://debugger-test.dll/debugger-test.cs", 5, 2, scripts, top_frame["location"]);

				//not check the scope
				var scope = top_frame ["scopeChain"][0];
				Assert.Equal ("local", scope ["type"]);
				Assert.Equal ("IntAdd", scope ["name"]);

				Assert.Equal ("object", scope ["object"]["type"]);
				Assert.Equal ("dotnet:scope:0", scope ["object"]["objectId"]);
				CheckLocation ("dotnet://debugger-test.dll/debugger-test.cs", 4, 41, scripts, scope["startLocation"]);
				CheckLocation ("dotnet://debugger-test.dll/debugger-test.cs", 10, 1, scripts, scope["endLocation"]);
			});
		}
	}
}
