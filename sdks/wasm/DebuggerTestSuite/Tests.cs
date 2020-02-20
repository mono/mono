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
using WebAssembly.Net.Debugging;

namespace DebuggerTests
{

	public class SourceList : DebuggerTestBase {
		Dictionary<string, string> dicScriptsIdToUrl;
		Dictionary<string, string> dicFileToUrl;
		Dictionary<string, string> SubscribeToScripts (Inspector insp) {
			dicScriptsIdToUrl = new Dictionary<string, string> ();
			dicFileToUrl = new Dictionary<string, string>();
			insp.On("Debugger.scriptParsed", async (args, c) => {
				var script_id = args? ["scriptId"]?.Value<string> ();
				var url = args["url"]?.Value<string> ();
				if (script_id.StartsWith("dotnet://"))
				{
					var dbgUrl = args["dotNetUrl"]?.Value<string>();
					var arrStr = dbgUrl.Split("/");
					dbgUrl = arrStr[0] + "/" + arrStr[1] + "/" + arrStr[2] + "/" + arrStr[arrStr.Length - 1];
					dicScriptsIdToUrl[script_id] = dbgUrl;
					dicFileToUrl[dbgUrl] = args["url"]?.Value<string>();
				} else if (!String.IsNullOrEmpty (url)) {
					dicFileToUrl[new Uri (url).AbsolutePath] = url;
				}
				await Task.FromResult (0);
			});
			return dicScriptsIdToUrl;
		}

		void CheckLocation (string script_loc, int line, int column, Dictionary<string, string> scripts, JToken location)
		{
			Assert.Equal (script_loc, scripts[location["scriptId"].Value<string>()]);
			Assert.Equal (line, location ["lineNumber"].Value<int> ());
			Assert.Equal (column, location ["columnNumber"].Value<int> ());
		}

		[Fact]
		public async Task CheckThatAllSourcesAreSent () {
			var insp = new Inspector ();
			//Collect events
			var scripts = SubscribeToScripts(insp);

			await Ready();
			//all sources are sent before runtime ready is sent, nothing to check
			await insp.Ready ();
			Assert.True (scripts.ContainsValue ("dotnet://debugger-test.dll/debugger-test.cs"));
			Assert.True (scripts.ContainsValue ("dotnet://debugger-test.dll/debugger-test2.cs"));
			Assert.True (scripts.ContainsValue ("dotnet://Simple.Dependency.dll/dependency.cs"));
		}

		[Fact]
		public async Task CreateGoodBreakpoint () {
			var insp = new Inspector ();

			//Collect events
			var scripts = SubscribeToScripts(insp);

			await Ready ();
			await insp.Ready (async (cli, token) => {
				var bp1_req = JObject.FromObject(new {
					lineNumber = 5,
					columnNumber = 2,
					url = dicFileToUrl["dotnet://debugger-test.dll/debugger-test.cs"],
				});

				var bp1_res = await cli.SendCommand ("Debugger.setBreakpointByUrl", bp1_req, token);
				Assert.True (bp1_res.IsOk);
				Assert.Equal ("dotnet:0", bp1_res.Value ["breakpointId"]);
				Assert.Equal (1, bp1_res.Value ["locations"]?.Value<JArray> ()?.Count);
			
				var loc = bp1_res.Value ["locations"]?.Value<JArray> ()[0];

				Assert.NotNull (loc ["scriptId"]);
				Assert.Equal("dotnet://debugger-test.dll/debugger-test.cs", scripts [loc["scriptId"]?.Value<string> ()]);
				Assert.Equal (5, loc ["lineNumber"]);
				Assert.Equal (2, loc ["columnNumber"]);
			});
		}

		[Fact]
		public async Task CreateBadBreakpoint () {
			var insp = new Inspector ();

			//Collect events
			var scripts = SubscribeToScripts(insp);

			await Ready ();
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

			await Ready ();
			await insp.Ready (async (cli, token) => {
				var bp1_req = JObject.FromObject(new {
					lineNumber = 5,
					columnNumber = 2,
					url = dicFileToUrl["dotnet://debugger-test.dll/debugger-test.cs"],
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
				Assert.True (top_frame ["url"].Value<string> ().Contains("debugger-test.cs"));

				CheckLocation ("dotnet://debugger-test.dll/debugger-test.cs", 3, 41, scripts, top_frame["functionLocation"]);
				CheckLocation ("dotnet://debugger-test.dll/debugger-test.cs", 5, 2, scripts, top_frame["location"]);

				//now check the scope
				var scope = top_frame ["scopeChain"][0];
				Assert.Equal ("local", scope ["type"]);
				Assert.Equal ("IntAdd", scope ["name"]);

				Assert.Equal ("object", scope ["object"]["type"]);
				Assert.Equal ("dotnet:scope:0", scope ["object"]["objectId"]);
				CheckLocation ("dotnet://debugger-test.dll/debugger-test.cs", 3, 41, scripts, scope["startLocation"]);
				CheckLocation ("dotnet://debugger-test.dll/debugger-test.cs", 9, 1, scripts, scope["endLocation"]);
			});
		}

		[Fact]
		public async Task ExceptionThrownInJS () {
			var insp = new Inspector ();

			//Collect events
			var scripts = SubscribeToScripts(insp);

			await Ready ();
			await insp.Ready (async (cli, token) => {
				var eval_req = JObject.FromObject(new {
					expression = "invoke_bad_js_test();"
				});

				var eval_res = await cli.SendCommand ("Runtime.evaluate", eval_req, token);
				Assert.True (eval_res.IsErr);
				Assert.Equal ("Uncaught", eval_res.Error ["exceptionDetails"]? ["text"]? .Value<string> ());
			});
		}

		[Fact]
		public async Task ExceptionThrownInJSOutOfBand () {
			var insp = new Inspector ();

			//Collect events
			var scripts = SubscribeToScripts(insp);

			await Ready ();
			await insp.Ready (async (cli, token) => {
				var bp1_req = JObject.FromObject(new {
					lineNumber = 27,
					columnNumber = 2,
					url = dicFileToUrl["/debugger-driver.html"],
				});

				var bp1_res = await cli.SendCommand ("Debugger.setBreakpointByUrl", bp1_req, token);
				Assert.True (bp1_res.IsOk);

				var eval_req = JObject.FromObject(new {
					expression = "window.setTimeout(function() { invoke_bad_js_test(); }, 1);",
				});

				var eval_res = await cli.SendCommand ("Runtime.evaluate", eval_req, token);
				// Response here will be the id for the timer from JS!
				Assert.True (eval_res.IsOk);

				var ex = await Assert.ThrowsAsync<ArgumentException> (async () => await insp.WaitFor("Runtime.exceptionThrown"));
				var ex_json = JObject.Parse (ex.Message);
				Assert.Equal (dicFileToUrl["/debugger-driver.html"], ex_json ["exceptionDetails"]? ["url"]? .Value<string> ());
			});

		}

		void CheckNumber (JToken locals, string name, int value) {
			foreach (var l in locals) {
				if (name != l["name"]?.Value<string> ())
					continue;
				var val = l["value"];
				Assert.Equal ("number", val ["type"]?.Value<string> ());
				Assert.Equal (value, val["value"]?.Value <int> ());
				return;
			}
			Assert.True(false, $"Could not find variable '{name}'");
		}

		void CheckObject (JToken locals, string name, string class_name, string subtype=null) {
			Console.WriteLine ($"** Locals: {locals.ToString ()}");
			Console.WriteLine ($"\tChecking {name}");
			foreach (var l in locals) {
				if (name != l["name"]?.Value<string> ())
					continue;

				var val = l["value"];
				Assert.Equal ("object", val ["type"]?.Value<string> ());
				Assert.Equal (class_name, val ["className"]?.Value<string> ());
				Assert.Equal (subtype, val ["subtype"]?.Value<string> ());
				return;
			}
			Assert.True(false, $"Could not find variable '{name}'");
		}

		void CheckArray (JToken locals, string name, string class_name) {
			Console.WriteLine ($"** Locals: {locals.ToString ()}");
			Console.WriteLine ($"\tChecking {name}");
			foreach (var l in locals) {
				if (name != l["name"]?.Value<string> ())
					continue;

				var val = l["value"];
				Assert.Equal ("object", val ["type"]?.Value<string> ());
				Assert.Equal ("array", val ["subtype"]?.Value<string> ());
				Assert.Equal (class_name, val ["className"]?.Value<string> ());

				//FIXME: elements?
				return;
			}
			Assert.True(false, $"Could not find variable '{name}'");
		}

		void CheckFunction (JToken locals, string name, string description, string subtype=null) {
			Console.WriteLine ($"** Locals: {locals.ToString ()}");
			foreach (var l in locals) {
				if (name != l["name"]?.Value<string> ())
					continue;

				var val = l["value"];
				Assert.Equal ("function", val ["type"]?.Value<string> ());
				Assert.Equal (description, val ["description"]?.Value<string> ());
				Assert.Equal (subtype, val ["subtype"]?.Value<string> ());
				return;
			}
			Assert.True(false, $"Could not find variable '{name}'");
		}

		[Fact]
		public async Task InspectLocalsAtBreakpointSite () =>
			await CheckInspectLocalsAtBreakpointSite (
				bp_req_fn: () => JObject.FromObject(new {
					lineNumber = 5,
					columnNumber = 2,
					url = dicFileToUrl["dotnet://debugger-test.dll/debugger-test.cs"],
				}),
				eval_req_fn: () => JObject.FromObject(new {
					expression = "window.setTimeout(function() { invoke_add(); }, 1);",
				}),
				test_fn: (locals) => {
					CheckNumber (locals, "a", 10);
					CheckNumber (locals, "b", 20);
					CheckNumber (locals, "c", 30);
					CheckNumber (locals, "d", 0);
					CheckNumber (locals, "e", 0);
				}
			);

		[Fact]
		public async Task InspectLocalsWithDelegatesAtBreakpointSite () =>
			await CheckInspectLocalsAtBreakpointSite (
				bp_req_fn: () => JObject.FromObject(new {
					lineNumber = 41,
					columnNumber = 2,
					url = dicFileToUrl["dotnet://debugger-test.dll/debugger-test.cs"],
				}),
				eval_req_fn: () => JObject.FromObject(new {
					expression = "window.setTimeout(function() { invoke_delegates_test (); }, 1);",
				}),
				test_fn: (locals) => {
					CheckObject (locals, "fn_func", "System.Func<Math, bool>");
					CheckObject (locals, "fn_func_null", "System.Func<Math, bool>", subtype: "null");
					CheckArray (locals, "fn_func_arr", "System.Func<Math, bool>[]");
					CheckFunction (locals, "fn_del", "Math.IsMathNull");
					CheckObject (locals, "fn_del_null", "Math.IsMathNull", subtype: "null");
					CheckArray (locals, "fn_del_arr", "Math.IsMathNull[]");

					// Unused locals
					CheckObject (locals, "fn_func_unused", "System.Func<Math, bool>", subtype: "null");
					CheckObject (locals, "fn_func_null_unused", "System.Func<Math, bool>", subtype: "null");
					CheckObject (locals, "fn_func_arr_unused", "System.Func<Math, bool>[]", subtype: "null");

					CheckObject (locals, "fn_del_unused", "Math.IsMathNull", subtype: "null");
					CheckObject (locals, "fn_del_null_unused", "Math.IsMathNull", subtype: "null");
					CheckObject (locals, "fn_del_arr_unused", "Math.IsMathNull[]", subtype: "null");
				}
			);

		[Fact]
		public async Task InspectLocalsWithGenericTypesAtBreakpointSite () =>
			await CheckInspectLocalsAtBreakpointSite (
				bp_req_fn: () => JObject.FromObject(new {
					lineNumber = 62,
					columnNumber = 2,
					url = dicFileToUrl["dotnet://debugger-test.dll/debugger-test.cs"],
				}),
				eval_req_fn: () => JObject.FromObject(new {
					expression = "window.setTimeout(function() { invoke_generic_types_test (); }, 1);",
				}),
				test_fn: (locals) => {
					CheckObject (locals, "list", "System.Collections.Generic.Dictionary<Math[], Math.IsMathNull>");
					CheckObject (locals, "list_null", "System.Collections.Generic.Dictionary<Math[], Math.IsMathNull>", subtype: "null");

					CheckArray (locals, "list_arr", "System.Collections.Generic.Dictionary<Math[], Math.IsMathNull>[]");
					CheckObject (locals, "list_arr_null", "System.Collections.Generic.Dictionary<Math[], Math.IsMathNull>[]", subtype: "null");

					// Unused locals
					CheckObject (locals, "list_unused", "System.Collections.Generic.Dictionary<Math[], Math.IsMathNull>", subtype: "null");
					CheckObject (locals, "list_null_unused", "System.Collections.Generic.Dictionary<Math[], Math.IsMathNull>", subtype: "null");

					CheckObject (locals, "list_arr_unused", "System.Collections.Generic.Dictionary<Math[], Math.IsMathNull>[]", subtype: "null");
					CheckObject (locals, "list_arr_null_unused", "System.Collections.Generic.Dictionary<Math[], Math.IsMathNull>[]", subtype: "null");
				}
			);

		async Task CheckInspectLocalsAtBreakpointSite (Func<JObject> bp_req_fn, Func<JObject> eval_req_fn, Action<JToken> test_fn) {
			var insp = new Inspector ();
			//Collect events
			var scripts = SubscribeToScripts(insp);

			await Ready ();
			await insp.Ready (async (cli, token) => {
				var bp_req = bp_req_fn ();
				System.Console.WriteLine("InspectLocalsAtBreakpointSite-1");
				var bp_res = await cli.SendCommand ("Debugger.setBreakpointByUrl", bp_req, token);
				System.Console.WriteLine("InspectLocalsAtBreakpointSite-2");
				Assert.True (bp_res.IsOk);
				System.Console.WriteLine("InspectLocalsAtBreakpointSite-3");
				var eval_req = eval_req_fn ();
				System.Console.WriteLine("InspectLocalsAtBreakpointSite-4");
				var eval_res = await cli.SendCommand ("Runtime.evaluate", eval_req, token);
				System.Console.WriteLine("InspectLocalsAtBreakpointSite-5");
				Assert.True (eval_res.IsOk);
				System.Console.WriteLine("InspectLocalsAtBreakpointSite-6");
				var pause_location = await insp.WaitFor(Inspector.PAUSE);
				System.Console.WriteLine("InspectLocalsAtBreakpointSite-7");
				//make sure we're on the right bp
				Assert.Equal ("dotnet:0", pause_location ["hitBreakpoints"]?[0]?.Value<string> ());

				var top_frame = pause_location ["callFrames"][0];

				var scope = top_frame ["scopeChain"][0];
				Assert.Equal ("dotnet:scope:0", scope ["object"]["objectId"]);

				//ok, what's on that scope?
				var get_prop_req = JObject.FromObject(new {
					objectId = "dotnet:scope:0",
				});

				var frame_props = await cli.SendCommand ("Runtime.getProperties", get_prop_req, token);
				if (frame_props.IsErr)
					Console.WriteLine ($"frame_props: {frame_props.Error.ToString ()}");
				Assert.True (frame_props.IsOk);

				var locals = frame_props.Value ["result"];
				if (test_fn != null)
					test_fn (locals);
			});
		}

		[Fact]
		public async Task RuntimeGetPropertiesWithInvalidScopeIdTest () {
			var insp = new Inspector ();
			//Collect events
			var scripts = SubscribeToScripts(insp);

			await Ready ();
			await insp.Ready (async (cli, token) => {
				var bp_req = JObject.FromObject(new {
					lineNumber = 41,
					columnNumber = 2,
					url = dicFileToUrl["dotnet://debugger-test.dll/debugger-test.cs"],
				});

				var bp_res = await cli.SendCommand ("Debugger.setBreakpointByUrl", bp_req, token);
				Assert.True (bp_res.IsOk);
				var eval_req = JObject.FromObject(new {
					expression = "window.setTimeout(function() { invoke_delegates_test (); }, 1);",
				});
				var eval_res = await cli.SendCommand ("Runtime.evaluate", eval_req, token);
				Assert.True (eval_res.IsOk);
				var pause_location = await insp.WaitFor(Inspector.PAUSE);

				//make sure we're on the right bp
				Assert.Equal ("dotnet:0", pause_location ["hitBreakpoints"]?[0]?.Value<string> ());

				var top_frame = pause_location ["callFrames"][0];

				var scope = top_frame ["scopeChain"][0];
				Assert.Equal ("dotnet:scope:0", scope ["object"]["objectId"]);

				// Try to get an invalid scope!
				var get_prop_req = JObject.FromObject(new {
					objectId = "dotnet:scope:23490871",
				});

				var frame_props = await cli.SendCommand ("Runtime.getProperties", get_prop_req, token);
				Assert.True (frame_props.IsErr);
			});
		}

		[Fact]
		public async Task TrivalStepping () {
			var insp = new Inspector ();
			//Collect events
			var scripts = SubscribeToScripts(insp);

			await Ready ();
			await insp.Ready (async (cli, token) => {
				var bp1_req = JObject.FromObject(new {
					lineNumber = 5,
					columnNumber = 2,
					url = dicFileToUrl["dotnet://debugger-test.dll/debugger-test.cs"],
				});

				var bp1_res = await cli.SendCommand ("Debugger.setBreakpointByUrl", bp1_req, token);
				Assert.True (bp1_res.IsOk);

				var eval_req = JObject.FromObject(new {
					expression = "window.setTimeout(function() { invoke_add(); }, 1);",
				});

				var eval_res = await cli.SendCommand ("Runtime.evaluate", eval_req, token);
				Assert.True (eval_res.IsOk);

				var pause_location = await insp.WaitFor(Inspector.PAUSE);
				//make sure we're on the right bp
				Assert.Equal ("dotnet:0", pause_location ["hitBreakpoints"]?[0]?.Value<string> ());
				var top_frame = pause_location ["callFrames"][0];
				System.Console.WriteLine("TrivalStepping1");
				CheckLocation ("dotnet://debugger-test.dll/debugger-test.cs", 3, 41, scripts, top_frame["functionLocation"]);
				System.Console.WriteLine("TrivalStepping2");
				CheckLocation ("dotnet://debugger-test.dll/debugger-test.cs", 5, 2, scripts, top_frame["location"]);
				System.Console.WriteLine("TrivalStepping3");
				var step_res = await cli.SendCommand ("Debugger.stepOver", null, token);
				Assert.True (step_res.IsOk);
				System.Console.WriteLine("TrivalStepping4");
				var pause_location2 = await insp.WaitFor(Inspector.PAUSE);
				System.Console.WriteLine("TrivalStepping5");
				var top_frame2 = pause_location2 ["callFrames"][0];
				System.Console.WriteLine("TrivalStepping6");
				CheckLocation ("dotnet://debugger-test.dll/debugger-test.cs", 3, 41, scripts, top_frame2["functionLocation"]);
				System.Console.WriteLine("TrivalStepping7");
				CheckLocation ("dotnet://debugger-test.dll/debugger-test.cs", 6, 2, scripts, top_frame2["location"]); //it moved one line!
				System.Console.WriteLine("TrivalStepping8");
			});
		}

		[Fact]
		public async Task InspectLocalsDuringStepping () {
			var insp = new Inspector ();
			//Collect events
			var scripts = SubscribeToScripts(insp);

			await Ready();
			await insp.Ready (async (cli, token) => {
				var bp1_req = JObject.FromObject(new {
					lineNumber = 4,
					columnNumber = 2,
					url = dicFileToUrl["dotnet://debugger-test.dll/debugger-test.cs"],
				});

				var bp1_res = await cli.SendCommand ("Debugger.setBreakpointByUrl", bp1_req, token);
				Assert.True (bp1_res.IsOk);

				var eval_req = JObject.FromObject(new {
					expression = "window.setTimeout(function() { invoke_add(); }, 1);",
				});

				var eval_res = await cli.SendCommand ("Runtime.evaluate", eval_req, token);
				Assert.True (eval_res.IsOk);

				var pause_location = await insp.WaitFor(Inspector.PAUSE);

				//ok, what's on that scope?
				var get_prop_req = JObject.FromObject(new {
					objectId = "dotnet:scope:0",
				});

				var frame_props = await cli.SendCommand ("Runtime.getProperties", get_prop_req, token);
				Assert.True (frame_props.IsOk);
				var locals = frame_props.Value ["result"];
				CheckNumber (locals, "a", 10);
				CheckNumber (locals, "b", 20);
				CheckNumber (locals, "c", 0);
				CheckNumber (locals, "d", 0);
				CheckNumber (locals, "e", 0);

				//step and get locals
				var step_res = await cli.SendCommand ("Debugger.stepOver", null, token);
				Assert.True (step_res.IsOk);
				pause_location = await insp.WaitFor(Inspector.PAUSE);
				frame_props = await cli.SendCommand ("Runtime.getProperties", get_prop_req, token);
				Assert.True (frame_props.IsOk);

				locals = frame_props.Value ["result"];
				CheckNumber (locals, "a", 10);
				CheckNumber (locals, "b", 20);
				CheckNumber (locals, "c", 30);
				CheckNumber (locals, "d", 0);
				CheckNumber (locals, "e", 0);

				//step and get locals
				step_res = await cli.SendCommand ("Debugger.stepOver", null, token);
				Assert.True (step_res.IsOk);
				pause_location = await insp.WaitFor(Inspector.PAUSE);
				frame_props = await cli.SendCommand ("Runtime.getProperties", get_prop_req, token);
				Assert.True (frame_props.IsOk);

				locals = frame_props.Value ["result"];
				CheckNumber (locals, "a", 10);
				CheckNumber (locals, "b", 20);
				CheckNumber (locals, "c", 30);
				CheckNumber (locals, "d", 50);
				CheckNumber (locals, "e", 0);
			});
		}

		//TODO add tests covering basic stepping behavior as step in/out/over
	}
}
