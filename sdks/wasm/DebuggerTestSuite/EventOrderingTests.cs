using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Xunit;
using WebAssembly.Net.Debugging;
using System.Threading;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace DebuggerTests
{
	public class EventOrderingTests : DebuggerTestBase, IAsyncLifetime
	{
		Inspector insp;
		InspectorClient client;
		CancellationTokenSource cts;
		ILoggerFactory loggerFactory;
		Dictionary<string, string> scripts;
		Uri clientConnectUri;

		public async Task InitializeAsync()
		{
			insp = new Inspector ();

			scripts = SubscribeToScripts (insp);
			await Ready ();

			cts = new CancellationTokenSource();
			cts.CancelAfter (60 * 1000);
			loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().AddFilter(null, LogLevel.Trace));
			client = new InspectorClient (loggerFactory.CreateLogger<Inspector>());
			clientConnectUri = new Uri ($"ws://{TestHarnessProxy.Endpoint.Authority}/launch-chrome-and-connect");
		}

		public async Task DisposeAsync()
		{
			await client?.Close (cts.Token);
			client?.Dispose ();
			loggerFactory?.Dispose ();
			cts?.Dispose ();
		}

		[Theory]
		[InlineData ("/other.js$", "/other.js", "big_array_js_test (10);", "big_array_js_test", 3, 2)]
		[InlineData ("/debugger-test.cs$", "dotnet://debugger-test.dll/debugger-test.cs", "invoke_add ();", "IntAdd", 8, 2,
					Skip = "Bug: Cannot set breakpoints before the startup commands")]
		public async Task SetBpBeforeAnyStartupCommands (string url_regex, string test_url, string eval_str, string fn_name, int line, int col)
		{
			await client.Connect (clientConnectUri, OnMessage, async token => {
				ctx = new DebugTestContext (client, insp, token, scripts);

				Assert.Empty (events);

				var bp1_res = await SetBreakpoint (url_regex, line, col, use_regex:true);

				// breakpoint not resolved yet
				Assert.Empty (bp1_res.Value ["locations"]?.Value<JArray> ());
				Assert.DoesNotContain ("Debugger.breakpointResolved", events);

				// Start the regular startup commands
				_ = client.SendCommand ("Profiler.enable", null, token);
				_ = client.SendCommand ("Runtime.enable", null, token);
				_ = client.SendCommand ("Debugger.enable", null, token);
				_ = client.SendCommand ("Runtime.runIfWaitingForDebugger", null, token);
				await insp.WaitFor ("ready");

				// Check that test script has been loaded
				Assert.Contains (test_url, dicFileToUrl.Keys);
				// breakpointResolved event, because it was requested before Debugger.enable
				Assert.Contains ("Debugger.breakpointResolved", events);

				await EvaluateAndCheck (
					"window.setTimeout(function() { " + eval_str + " }, 1);",
					test_url, line, col,
					fn_name
				);

				var script_ix = events.IndexOf ($"Debugger.scriptParsed {dicFileToUrl [test_url]}");
				var resolve_ix = events.IndexOf ("Debugger.breakpointResolved");

				Assert.True (script_ix >= 0, "No scriptParsed event found");
				Assert.True (resolve_ix >= 0, "No breakpointResolved event found");

				// check breakpoint resolved after script is loaded
				Assert.True (script_ix < resolve_ix);
			}, cts.Token);
		}

		[Theory]
		[InlineData ("/other.js$", "/other.js", "big_array_js_test (10);", "big_array_js_test", 3, 2)]
		[InlineData ("/debugger-test.cs$", "dotnet://debugger-test.dll/debugger-test.cs", "invoke_add ();", "IntAdd", 8, 2)]
		public async Task SetBpAfterRuntimeButBeforeDebuggerEnable (string url_regex, string test_url, string eval_str, string fn_name, int line, int col)
		{
			await client.Connect (clientConnectUri, OnMessage, async token => {
				ctx = new DebugTestContext (client, insp, token, scripts);

				_ = client.SendCommand ("Profiler.enable", null, token);
				await client.SendCommand ("Runtime.enable", null, token);

				Assert.DoesNotContain ("Mono.runtimeReady", events);
				// Send the bp request
				var bp1_res = await SetBreakpoint (url_regex, line, col, use_regex:true);

				// breakpoint not resolved yet
				Assert.Empty (bp1_res.Value ["locations"]?.Value<JArray> ());
				Assert.DoesNotContain ("Debugger.breakpointResolved", events);
				Assert.Empty (scripts);

				_ = client.SendCommand ("Debugger.enable", null, token);
				_ = client.SendCommand ("Runtime.runIfWaitingForDebugger", null, token);
				await insp.WaitFor ("ready");

				// Check that test script has been loaded
				Assert.Contains (test_url, dicFileToUrl.Keys);
				// breakpointResolved event, because it was requested before Debugger.enable
				Assert.Contains ("Debugger.breakpointResolved", events);

				await EvaluateAndCheck (
					"window.setTimeout(function() { " + eval_str + " }, 1);",
					test_url, line, col,
					fn_name
				);

				var script_ix = events.IndexOf ($"Debugger.scriptParsed {dicFileToUrl[test_url]}");
				var resolve_ix = events.IndexOf ("Debugger.breakpointResolved");

				Assert.True (script_ix >= 0, "No scriptParsed event found");
				Assert.True (resolve_ix >= 0, "No breakpointResolved event found");

				// check breakpoint resolved after script is loaded
				Assert.True (script_ix < resolve_ix);
			}, cts.Token);
		}

		[Theory]
		[InlineData ("/other.js$", "/other.js", "big_array_js_test (10);", "big_array_js_test", 3, 2)]
		[InlineData ("/debugger-test.cs$", "dotnet://debugger-test.dll/debugger-test.cs", "invoke_add ();", "IntAdd", 8, 2)]
		public async Task SetBpAfterInspectorReadyNoBPResolvedEvent (string url_regex, string test_url, string eval_str, string fn_name, int line, int col)
		{
			await client.Connect (clientConnectUri, OnMessage, async token => {
				ctx = new DebugTestContext (client, insp, token, scripts);

				_ = client.SendCommand ("Profiler.enable", null, token);
				_ = client.SendCommand ("Runtime.enable",  null, token);
				_ = client.SendCommand ("Debugger.enable", null, token);
				_ = client.SendCommand ("Runtime.runIfWaitingForDebugger", null, token);
				await insp.WaitFor ("ready");

				var bp1_res = await SetBreakpoint (url_regex, line, col, use_regex:true);
				var locs = bp1_res.Value ["locations"]?.Value<JArray> ();
				Assert.Single (locs);
				CheckLocation (test_url, line, col, scripts, locs [0]);

				// Check that test script has been loaded
				Assert.Contains (test_url, dicFileToUrl.Keys);
				// No breakpoint resolved event, because it was resolved with `SetBp`
				Assert.DoesNotContain ("Debugger.breakpointResolved", events);

				await EvaluateAndCheck (
					"window.setTimeout(function() { " + eval_str + " }, 1);",
					test_url, line, col,
					fn_name
				);
				Assert.DoesNotContain ("Debugger.breakpointResolved", events);
			}, cts.Token);
		}

		async Task OnMessage (string method, JObject args, CancellationToken token)
		{
			if (method != "Debugger.scriptParsed") {
				if (method == "Debugger.breakpointResolved")
					events.Add (method);
				else
					events.Add ($"{method} {args}");
			}
			await insp.OnMessage (method, args, token);
		}
	}
}
