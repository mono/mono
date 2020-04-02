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
		DebugTestContext ctx;
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
			var loc_str = $"{ scripts[location["scriptId"].Value<string>()] }"
							+ $"#{ location ["lineNumber"].Value<int> () }"
							+ $"#{ location ["columnNumber"].Value<int> () }";

			var expected_loc_str = $"{script_loc}#{line}#{column}";
			Assert.Equal (expected_loc_str, loc_str);
		}

		[Fact]
		public async Task CheckThatAllSourcesAreSent () {
			var insp = new Inspector ();
			//Collect events
			var scripts = SubscribeToScripts(insp);

			await Ready();
			//all sources are sent before runtime ready is sent, nothing to check
			await insp.Ready ();
			Assert.Contains ("dotnet://debugger-test.dll/debugger-test.cs", scripts.Values);
			Assert.Contains ("dotnet://debugger-test.dll/debugger-test2.cs", scripts.Values);
			Assert.Contains ("dotnet://Simple.Dependency.dll/dependency.cs", scripts.Values);
		}

		[Fact]
		public async Task CreateGoodBreakpoint () {
			var insp = new Inspector ();

			//Collect events
			var scripts = SubscribeToScripts(insp);

			await Ready ();
			await insp.Ready (async (cli, token) => {
				ctx = new DebugTestContext (cli, insp, token, scripts);

				var bp1_res = await SetBreakpoint ("dotnet://debugger-test.dll/debugger-test.cs", 5, 2);

				Assert.EndsWith ("debugger-test.cs", bp1_res.Value ["breakpointId"].ToString());
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

				Assert.True (bp1_res.IsOk);
				Assert.Empty (bp1_res.Value["locations"].Values<object>());
				//Assert.Equal ((int)MonoErrorCodes.BpNotFound, bp1_res.Error ["code"]?.Value<int> ());
			});
		}

		[Fact]
		public async Task CreateGoodBreakpointAndHit () {
			var insp = new Inspector ();

			//Collect events
			var scripts = SubscribeToScripts(insp);

			await Ready ();
			await insp.Ready (async (cli, token) => {
				ctx = new DebugTestContext (cli, insp, token, scripts);

				var bp = await SetBreakpoint ("dotnet://debugger-test.dll/debugger-test.cs", 5, 2);

				var eval_req = JObject.FromObject(new {
					expression = "window.setTimeout(function() { invoke_add(); }, 1);",
				});

				await EvaluateAndCheck (
					"window.setTimeout(function() { invoke_add(); }, 1);",
					"dotnet://debugger-test.dll/debugger-test.cs", 5, 2,
					"IntAdd",
					wait_for_event_fn: (pause_location) => {
						Assert.Equal ("other", pause_location ["reason"]?.Value<string> ());
						Assert.Equal (bp.Value["breakpointId"]?.ToString(), pause_location ["hitBreakpoints"]?[0]?.Value<string> ());

						var top_frame = pause_location ["callFrames"][0];
						Assert.Equal ("IntAdd", top_frame ["functionName"].Value<string>());
						Assert.Contains ("debugger-test.cs", top_frame ["url"].Value<string> ());

						CheckLocation ("dotnet://debugger-test.dll/debugger-test.cs", 3, 41, scripts, top_frame["functionLocation"]);

						//now check the scope
						var scope = top_frame ["scopeChain"][0];
						Assert.Equal ("local", scope ["type"]);
						Assert.Equal ("IntAdd", scope ["name"]);

						Assert.Equal ("object", scope ["object"]["type"]);
						Assert.Equal ("dotnet:scope:0", scope ["object"]["objectId"]);
						CheckLocation ("dotnet://debugger-test.dll/debugger-test.cs", 3, 41, scripts, scope["startLocation"]);
						CheckLocation ("dotnet://debugger-test.dll/debugger-test.cs", 9, 1, scripts, scope["endLocation"]);
						return Task.CompletedTask;
					}
				);

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
				ctx = new DebugTestContext (cli, insp, token, scripts);

				await SetBreakpoint ("/debugger-driver.html", 27, 2);

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

		void CheckString (JToken locals, string name, string value) {
			foreach (var l in locals) {
				if (name != l["name"]?.Value<string> ())
					continue;
				var val = l["value"];
				if (value == null) {
						Assert.Equal ("object", val ["type"]?.Value<string> ());
						Assert.Equal ("null", val["subtype"]?.Value<string> ());
				} else {
						Assert.Equal ("string", val ["type"]?.Value<string> ());
						Assert.Equal (value, val["value"]?.Value <string> ());
				}
				return;
			}
			Assert.True(false, $"Could not find variable '{name}'");
		}

		JToken CheckObject (JToken locals, string name, string class_name, string subtype=null, bool is_null=false) {
			var l = GetAndAssertObjectWithName (locals, name);
			var val = l["value"];
			Assert.Equal ("object", val ["type"]?.Value<string> ());
			Assert.True (val ["isValueType"] == null || !val ["isValueType"].Value<bool> ());
			Assert.Equal (class_name, val ["className"]?.Value<string> ());

			var has_null_subtype = val ["subtype"] != null && val ["subtype"]?.Value<string> () == "null";
			Assert.Equal (is_null, has_null_subtype);
			if (subtype != null)
				Assert.Equal (subtype, val ["subtype"]?.Value<string> ());

			return l;
		}

		async Task CheckDateTime (JToken locals, string name, DateTime expected)
			=> await CheckObjectOnLocals (locals, name,
				test_fn: (members) => {
					// not checking everything
#if false
					CheckNumber (members, "Year", expected.Year);
					CheckNumber (members, "Month", expected.Month);
					CheckNumber (members, "Day", expected.Day);
					CheckNumber (members, "Hour", expected.Hour);
					CheckNumber (members, "Minute", expected.Minute);
					CheckNumber (members, "Second", expected.Second);
#endif

					CheckString (members, "Year", "int");
					CheckString (members, "Month", "int");
					CheckString (members, "Day", "int");
					CheckString (members, "Hour", "int");
					CheckString (members, "Minute", "int");
					CheckString (members, "Second", "int");

					// FIXME: check some float properties too
				}
			);

		JToken CheckBool (JToken locals, string name, bool expected)
		{
			var l = GetAndAssertObjectWithName (locals, name);
			var val = l["value"];
			Assert.Equal ("boolean", val ["type"]?.Value<string> ());
			if (val ["value"] == null)
				Assert.True (false, "expected bool value not found for variable named {name}");
			Assert.Equal (expected, val ["value"]?.Value<bool> ());

			return l;
		}

		JToken CheckValueType (JToken locals, string name, string class_name) {
			var l = GetAndAssertObjectWithName (locals, name);
			var val = l["value"];
			Assert.Equal ("object", val ["type"]?.Value<string> ());
			Assert.True (val ["isValueType"] != null && val ["isValueType"].Value<bool> ());
			Assert.Equal (class_name, val ["className"]?.Value<string> ());
			return l;
		}

		JToken CheckEnum (JToken locals, string name, string class_name, string descr) {
			var l = GetAndAssertObjectWithName (locals, name);
			var val = l["value"];
			Assert.Equal ("object", val ["type"]?.Value<string> ());
			Assert.True (val ["isEnum"] != null && val ["isEnum"].Value<bool> ());
			Assert.Equal (class_name, val ["className"]?.Value<string> ());
			Assert.Equal (descr, val ["description"]?.Value<string> ());
			return l;
		}

		void CheckArray (JToken locals, string name, string class_name) {
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

		JToken GetAndAssertObjectWithName (JToken obj, string name)
		{
			var l = obj.FirstOrDefault (jt => jt ["name"]?.Value<string> () == name);
			if (l == null)
				Assert.True (false, $"Could not find variable '{name}'");
			return l;
		}

		[Fact]
		public async Task InspectLocalsAtBreakpointSite () =>
			await CheckInspectLocalsAtBreakpointSite (
				"dotnet://debugger-test.dll/debugger-test.cs", 5, 2, "IntAdd",
				"window.setTimeout(function() { invoke_add(); }, 1);",
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
				"dotnet://debugger-test.dll/debugger-test.cs", 41, 2, "DelegatesTest",
				"window.setTimeout(function() { invoke_delegates_test (); }, 1);",
				test_fn: (locals) => {
					CheckObject (locals, "fn_func", "System.Func<Math, bool>");
					CheckObject (locals, "fn_func_null", "System.Func<Math, bool>", is_null: true);
					CheckArray (locals, "fn_func_arr", "System.Func<Math, bool>[]");
					CheckFunction (locals, "fn_del", "Math.IsMathNull");
					CheckObject (locals, "fn_del_null", "Math.IsMathNull", is_null: true);
					CheckArray (locals, "fn_del_arr", "Math.IsMathNull[]");

					// Unused locals
					CheckObject (locals, "fn_func_unused", "System.Func<Math, bool>", is_null: true);
					CheckObject (locals, "fn_func_null_unused", "System.Func<Math, bool>", is_null: true);
					CheckObject (locals, "fn_func_arr_unused", "System.Func<Math, bool>[]", is_null: true);

					CheckObject (locals, "fn_del_unused", "Math.IsMathNull", is_null: true);
					CheckObject (locals, "fn_del_null_unused", "Math.IsMathNull", is_null: true);
					CheckObject (locals, "fn_del_arr_unused", "Math.IsMathNull[]", is_null: true);
				}
			);

		[Fact]
		public async Task InspectLocalsWithGenericTypesAtBreakpointSite () =>
			await CheckInspectLocalsAtBreakpointSite (
				"dotnet://debugger-test.dll/debugger-test.cs", 62, 2, "GenericTypesTest",
				"window.setTimeout(function() { invoke_generic_types_test (); }, 1);",
				test_fn: (locals) => {
					CheckObject (locals, "list", "System.Collections.Generic.Dictionary<Math[], Math.IsMathNull>");
					CheckObject (locals, "list_null", "System.Collections.Generic.Dictionary<Math[], Math.IsMathNull>", is_null: true);

					CheckArray (locals, "list_arr", "System.Collections.Generic.Dictionary<Math[], Math.IsMathNull>[]");
					CheckObject (locals, "list_arr_null", "System.Collections.Generic.Dictionary<Math[], Math.IsMathNull>[]", is_null: true);

					// Unused locals
					CheckObject (locals, "list_unused", "System.Collections.Generic.Dictionary<Math[], Math.IsMathNull>", is_null: true);
					CheckObject (locals, "list_null_unused", "System.Collections.Generic.Dictionary<Math[], Math.IsMathNull>", is_null: true);

					CheckObject (locals, "list_arr_unused", "System.Collections.Generic.Dictionary<Math[], Math.IsMathNull>[]", is_null: true);
					CheckObject (locals, "list_arr_null_unused", "System.Collections.Generic.Dictionary<Math[], Math.IsMathNull>[]", is_null: true);
				}
			);

		async Task CheckInspectLocalsAtBreakpointSite (string url_key, int line, int column, string function_name, string eval_expression, Action<JToken> test_fn) {
			var insp = new Inspector ();
			//Collect events
			var scripts = SubscribeToScripts(insp);

			await Ready ();
			await insp.Ready (async (cli, token) => {
				ctx = new DebugTestContext (cli, insp, token, scripts);

				var bp = await SetBreakpoint (url_key, line, column);

				await EvaluateAndCheck (
					eval_expression, url_key, line, column,
					function_name,
					wait_for_event_fn: (pause_location) => {
						//make sure we're on the right bp

						Assert.Equal (bp.Value ["breakpointId"]?.ToString (), pause_location ["hitBreakpoints"]?[0]?.Value<string> ());

						var top_frame = pause_location ["callFrames"][0];

						var scope = top_frame ["scopeChain"][0];
						Assert.Equal ("dotnet:scope:0", scope ["object"]["objectId"]);
						return Task.CompletedTask;
					},
					locals_fn: (locals) => {
						if (test_fn != null)
							test_fn (locals);
					}
				);
			});
		}

		[Fact]
		public async Task RuntimeGetPropertiesWithInvalidScopeIdTest () {
			var insp = new Inspector ();
			//Collect events
			var scripts = SubscribeToScripts(insp);

			await Ready ();
			await insp.Ready (async (cli, token) => {
				ctx = new DebugTestContext (cli, insp, token, scripts);

				var bp = await SetBreakpoint ("dotnet://debugger-test.dll/debugger-test.cs", 41, 2);

				await EvaluateAndCheck (
					"window.setTimeout(function() { invoke_delegates_test (); }, 1);",
					"dotnet://debugger-test.dll/debugger-test.cs", 41, 2,
					"DelegatesTest",
					wait_for_event_fn: async (pause_location) => {
						//make sure we're on the right bp
						Assert.Equal (bp.Value ["breakpointId"]?.ToString (), pause_location ["hitBreakpoints"]?[0]?.Value<string> ());

						var top_frame = pause_location ["callFrames"][0];

						var scope = top_frame ["scopeChain"][0];
						Assert.Equal ("dotnet:scope:0", scope ["object"]["objectId"]);

						// Try to get an invalid scope!
						var get_prop_req = JObject.FromObject(new {
							objectId = "dotnet:scope:23490871",
						});

						var frame_props = await cli.SendCommand ("Runtime.getProperties", get_prop_req, token);
						Assert.True (frame_props.IsErr);
					}
				);
			});
		}

		[Fact]
		public async Task TrivalStepping () {
			var insp = new Inspector ();
			//Collect events
			var scripts = SubscribeToScripts(insp);

			await Ready ();
			await insp.Ready (async (cli, token) => {
				ctx = new DebugTestContext (cli, insp, token, scripts);

				var bp = await SetBreakpoint ("dotnet://debugger-test.dll/debugger-test.cs", 5, 2);

				await EvaluateAndCheck (
					"window.setTimeout(function() { invoke_add(); }, 1);",
					"dotnet://debugger-test.dll/debugger-test.cs", 5, 2,
					"IntAdd",
					wait_for_event_fn: (pause_location) => {
						//make sure we're on the right bp
						Assert.Equal (bp.Value ["breakpointId"]?.ToString (), pause_location ["hitBreakpoints"]?[0]?.Value<string> ());

						var top_frame = pause_location ["callFrames"][0];
						CheckLocation ("dotnet://debugger-test.dll/debugger-test.cs", 3, 41, scripts, top_frame["functionLocation"]);
						return Task.CompletedTask;
					}
				);

				await StepAndCheck (StepKind.Over, "dotnet://debugger-test.dll/debugger-test.cs", 6, 2, "IntAdd",
						wait_for_event_fn: (pause_location) => {
							var top_frame = pause_location ["callFrames"][0];
							CheckLocation ("dotnet://debugger-test.dll/debugger-test.cs", 3, 41, scripts, top_frame["functionLocation"]);
							return Task.CompletedTask;
						}
				);
			});
		}

		[Fact]
		public async Task InspectLocalsDuringStepping () {
			var insp = new Inspector ();
			//Collect events
			var scripts = SubscribeToScripts(insp);

			await Ready();
			await insp.Ready (async (cli, token) => {
				ctx = new DebugTestContext (cli, insp, token, scripts);

				var debugger_test_loc = "dotnet://debugger-test.dll/debugger-test.cs";
				await SetBreakpoint (debugger_test_loc, 4, 2);

				await EvaluateAndCheck (
					"window.setTimeout(function() { invoke_add(); }, 1);",
					debugger_test_loc, 4, 2, "IntAdd",
					locals_fn: (locals) => {
						CheckNumber (locals, "a", 10);
						CheckNumber (locals, "b", 20);
						CheckNumber (locals, "c", 0);
						CheckNumber (locals, "d", 0);
						CheckNumber (locals, "e", 0);
					}
				);

				await StepAndCheck (StepKind.Over, debugger_test_loc, 5, 2, "IntAdd",
					locals_fn: (locals) => {
						CheckNumber (locals, "a", 10);
						CheckNumber (locals, "b", 20);
						CheckNumber (locals, "c", 30);
						CheckNumber (locals, "d", 0);
						CheckNumber (locals, "e", 0);
					}
				);

				//step and get locals
				await StepAndCheck (StepKind.Over, debugger_test_loc, 6, 2, "IntAdd",
					locals_fn: (locals) => {
						CheckNumber (locals, "a", 10);
						CheckNumber (locals, "b", 20);
						CheckNumber (locals, "c", 30);
						CheckNumber (locals, "d", 50);
						CheckNumber (locals, "e", 0);
					}
				);
			});
		}

		[Fact]
		public async Task InspectLocalsInPreviousFramesDuringSteppingIn2 () {
			var insp = new Inspector ();
			//Collect events
			var scripts = SubscribeToScripts(insp);

			await Ready();
			await insp.Ready (async (cli, token) => {
				ctx = new DebugTestContext (cli, insp, token, scripts);

				var dep_cs_loc = "dotnet://Simple.Dependency.dll/dependency.cs";
				await SetBreakpoint (dep_cs_loc, 24, 2);

				var debugger_test_loc = "dotnet://debugger-test.dll/debugger-test.cs";

				// Will stop in Complex.DoEvenMoreStuff
				var pause_location = await EvaluateAndCheck (
					"window.setTimeout(function() { invoke_use_complex (); }, 1);",
					dep_cs_loc, 24, 2, "DoEvenMoreStuff",
					locals_fn: (locals) => {
						Assert.Single (locals);
						CheckObject (locals, "this", "Simple.Complex");
					}
				);

				await CheckObjectOnFrameLocals (pause_location["callFrames"][0], "this",
					test_fn: (props) => {
						Assert.Equal (3, props.Count());
						CheckNumber (props, "A", 10);
						CheckString (props, "B", "xx");
						CheckObject (props, "c", "object");
					}
				);

				// Check UseComplex frame
				await CheckLocalsOnFrame (pause_location ["callFrames"][3], debugger_test_loc, 17, 2, "UseComplex",
					test_fn: (locals_m1) => {
						Assert.Equal (7, locals_m1.Count());

						CheckNumber (locals_m1, "a", 10);
						CheckNumber (locals_m1, "b", 20);
						CheckObject (locals_m1, "complex", "Simple.Complex");
						CheckNumber (locals_m1, "c", 30);
						CheckNumber (locals_m1, "d", 50);
						CheckNumber (locals_m1, "e", 60);
						CheckNumber (locals_m1, "f", 0);
					}
				);

				await CheckObjectOnFrameLocals (pause_location["callFrames"][3], "complex",
					test_fn: (props) => {
						Assert.Equal (3, props.Count());
						CheckNumber (props, "A", 10);
						CheckString (props, "B", "xx");
						CheckObject (props, "c", "object");
					}
				);

				pause_location = await StepAndCheck (StepKind.Over, dep_cs_loc, 16, 2, "DoStuff", times: 2);
				// Check UseComplex frame again
				await CheckLocalsOnFrame (pause_location ["callFrames"][1], debugger_test_loc, 17, 2, "UseComplex",
					test_fn: (locals_m1) => {
						Assert.Equal (7, locals_m1.Count());

						CheckNumber (locals_m1, "a", 10);
						CheckNumber (locals_m1, "b", 20);
						CheckObject (locals_m1, "complex", "Simple.Complex");
						CheckNumber (locals_m1, "c", 30);
						CheckNumber (locals_m1, "d", 50);
						CheckNumber (locals_m1, "e", 60);
						CheckNumber (locals_m1, "f", 0);
					}
				);

				await CheckObjectOnFrameLocals (pause_location["callFrames"][1], "complex",
					test_fn: (props) => {
						Assert.Equal (3, props.Count());
						CheckNumber (props, "A", 10);
						CheckString (props, "B", "xx");
						CheckObject (props, "c", "object");
					}
				);
			});
		}

		[Fact]
		public async Task InspectLocalsInPreviousFramesDuringSteppingIn () {
			var insp = new Inspector ();
			//Collect events
			var scripts = SubscribeToScripts(insp);

			await Ready();
			await insp.Ready (async (cli, token) => {
				ctx = new DebugTestContext (cli, insp, token, scripts);

				var debugger_test_loc = "dotnet://debugger-test.dll/debugger-test.cs";
				await SetBreakpoint (debugger_test_loc, 100, 3);

				// Will stop in InnerMethod
				var wait_res = await EvaluateAndCheck (
					"window.setTimeout(function() { invoke_outer_method(); }, 1);",
					debugger_test_loc, 100, 3, "InnerMethod",
					locals_fn: (locals) => {
						Assert.Equal (4, locals.Count());
						CheckNumber (locals, "i", 5);
						CheckNumber (locals, "j", 24);
						CheckString (locals, "foo_str", "foo");
						CheckObject (locals, "this", "Math.NestedInMath");
					}
				);

				var this_props = await CheckObjectOnFrameLocals (wait_res["callFrames"][0], "this",
					test_fn: (props) => {
						Assert.Equal (2, props.Count());
						CheckObject (props, "m", "Math");
						CheckValueType (props, "SimpleStructProperty", "Math.SimpleStruct");
					}
				);

				var ss_props = await CheckObjectOnLocals (this_props, "SimpleStructProperty",
					test_fn: (props) => {
						Assert.Equal (2, props.Count());
						CheckValueType (props, "dt", "System.DateTime");
						CheckValueType (props, "gs", "Math.GenericStruct<System.DateTime>");
					}
				);

				await CheckDateTime (ss_props, "dt", new DateTime (2020, 1, 2, 3, 4, 5));

				// Check OuterMethod frame
				await CheckLocalsOnFrame (wait_res ["callFrames"][1], debugger_test_loc, 76, 2, "OuterMethod",
					test_fn: (locals_m1) => {
						Assert.Equal (5, locals_m1.Count());
						// FIXME: Failing test CheckNumber (locals_m1, "i", 5);
						// FIXME: Failing test CheckString (locals_m1, "text", "Hello");
						CheckNumber (locals_m1, "new_i", 0);
						CheckNumber (locals_m1, "k", 0);
						CheckObject (locals_m1, "nim", "Math.NestedInMath");
					}
				);

				// step back into OuterMethod
				await StepAndCheck (StepKind.Over, debugger_test_loc, 80, 2, "OuterMethod", times: 9,
					locals_fn: (locals) => {
						Assert.Equal (5, locals.Count());

						// FIXME: Failing test CheckNumber (locals_m1, "i", 5);
						CheckString (locals, "text", "Hello");
						// FIXME: Failing test CheckNumber (locals, "new_i", 24);
						CheckNumber (locals, "k", 19);
						CheckObject (locals, "nim", "Math.NestedInMath");
					}
				);

				//await StepAndCheck (StepKind.Over, "dotnet://debugger-test.dll/debugger-test.cs", 81, 2, "OuterMethod", times: 2);

				// step into InnerMethod2
				await StepAndCheck (StepKind.Into, "dotnet://debugger-test.dll/debugger-test.cs", 85, 1, "InnerMethod2",
					locals_fn: (locals) => {
						Assert.Equal (3, locals.Count());

						CheckString (locals, "s", "test string");
						//out var: CheckNumber (locals, "k", 0);
						CheckNumber (locals, "i", 24);
					}
				);

				await StepAndCheck (StepKind.Over, "dotnet://debugger-test.dll/debugger-test.cs", 89, 1, "InnerMethod2", times: 4,
					locals_fn: (locals) => {
						Assert.Equal (3, locals.Count());

						CheckString (locals, "s", "test string");
						// FIXME: Failing test CheckNumber (locals, "k", 34);
						CheckNumber (locals, "i", 24);
					}
				);

				await StepAndCheck (StepKind.Over, "dotnet://debugger-test.dll/debugger-test.cs", 81, 2, "OuterMethod", times: 2,
					locals_fn: (locals) => {
						Assert.Equal (5, locals.Count());

						CheckString (locals, "text", "Hello");
						// FIXME: failing test CheckNumber (locals, "i", 5);
						CheckNumber (locals, "new_i", 22);
						CheckNumber (locals, "k", 34);
						CheckObject (locals, "nim", "Math.NestedInMath");
					}
				);
			});
		}

		[Fact]
		public async Task InspectLocalsDuringSteppingIn () {
			var insp = new Inspector ();
			//Collect events
			var scripts = SubscribeToScripts(insp);

			await Ready();
			await insp.Ready (async (cli, token) => {
				ctx = new DebugTestContext (cli, insp, token, scripts);

				await SetBreakpoint ("dotnet://debugger-test.dll/debugger-test.cs", 75, 2);

				await EvaluateAndCheck ("window.setTimeout(function() { invoke_outer_method(); }, 1);",
					"dotnet://debugger-test.dll/debugger-test.cs", 75, 2, "OuterMethod",
					locals_fn: (locals) => {
						Assert.Equal (5, locals.Count());

						CheckObject (locals, "nim", "Math.NestedInMath");
						CheckNumber (locals, "i", 0);
						CheckNumber (locals, "k", 0);
						CheckNumber (locals, "new_i", 0);
						CheckString (locals, "text", null);
					}
				);

				await StepAndCheck (StepKind.Over, "dotnet://debugger-test.dll/debugger-test.cs", 76, 2, "OuterMethod",
					locals_fn: (locals) => {
						Assert.Equal (5, locals.Count());

						CheckObject (locals, "nim", "Math.NestedInMath");
						// FIXME: Failing test CheckNumber (locals, "i", 5);
						CheckNumber (locals, "k", 0);
						CheckNumber (locals, "new_i", 0);
						CheckString (locals, "text", "Hello");
					}
				);

				// Step into InnerMethod
				await StepAndCheck (StepKind.Into, "dotnet://debugger-test.dll/debugger-test.cs", 94, 2, "InnerMethod");
				await StepAndCheck (StepKind.Over, "dotnet://debugger-test.dll/debugger-test.cs", 98, 3, "InnerMethod", times: 5,
					locals_fn: (locals) => {
						Assert.Equal (4, locals.Count());

						CheckNumber (locals, "i", 5);
						CheckNumber (locals, "j", 15);
						CheckString (locals, "foo_str", "foo");
						CheckObject (locals, "this", "Math.NestedInMath");
					}
				);

				// Step back to OuterMethod
				await StepAndCheck (StepKind.Over, "dotnet://debugger-test.dll/debugger-test.cs", 77, 2, "OuterMethod", times: 6,
					locals_fn: (locals) => {
						Assert.Equal (5, locals.Count());

						CheckObject (locals, "nim", "Math.NestedInMath");
						// FIXME: Failing test CheckNumber (locals, "i", 5);
						CheckNumber (locals, "k", 0);
						CheckNumber (locals, "new_i", 24);
						CheckString (locals, "text", "Hello");
					}
				);
			});
		}

		[Fact]
		public async Task InspectLocalsInAsyncMethods () {
			var insp = new Inspector ();
			//Collect events
			var scripts = SubscribeToScripts(insp);

			await Ready();
			await insp.Ready (async (cli, token) => {
				ctx = new DebugTestContext (cli, insp, token, scripts);
				var debugger_test_loc = "dotnet://debugger-test.dll/debugger-test.cs";

				await SetBreakpoint (debugger_test_loc, 108, 3);
				await SetBreakpoint (debugger_test_loc, 123, 3);

				// Will stop in Asyncmethod0
				var wait_res = await EvaluateAndCheck (
					"window.setTimeout(function() { invoke_async_method_with_await(); }, 1);",
					debugger_test_loc, 108, 3, "MoveNext", //FIXME:
					locals_fn: (locals) => {
						Assert.Equal (4, locals.Count());
						CheckString (locals, "s", "string from js");
						CheckNumber (locals, "i", 42);
						CheckString (locals, "local0", "value0");
						CheckObject (locals, "this", "Math.NestedInMath");
					}
				);

				// TODO: previous frames have async machinery details, so no point checking that right now

				var pause_loc = await SendCommandAndCheck (null, "Debugger.resume", debugger_test_loc, 123, 3, /*FIXME: "AsyncMethodNoReturn"*/ "MoveNext",
					locals_fn: (locals) => {
						Assert.Equal (4, locals.Count());
						CheckString (locals, "str", "AsyncMethodNoReturn's local");
						CheckObject (locals, "this", "Math.NestedInMath");
						//FIXME: check fields
						CheckValueType (locals, "ss", "Math.SimpleStruct");
						CheckArray (locals, "ss_arr", "Math.SimpleStruct[]");
						// TODO: struct fields
					}
				);

				await CheckObjectOnFrameLocals (pause_loc ["callFrames"][0], "this",
					test_fn: (props) => {
						Assert.Equal (2, props.Count ());
						CheckObject (props, "m", "Math");
						CheckValueType (props, "SimpleStructProperty", "Math.SimpleStruct");
					}
				);

				// TODO: Check `this` properties
			});
		}

		[Fact]
		public async Task InspectLocalsWithStructs () {
			var insp = new Inspector ();
			//Collect events
			var scripts = SubscribeToScripts(insp);

			await Ready();
			await insp.Ready (async (cli, token) => {
				ctx = new DebugTestContext (cli, insp, token, scripts);
				var debugger_test_loc = "dotnet://debugger-test.dll/debugger-valuetypes-test.cs";

				await SetBreakpoint (debugger_test_loc, 16, 2);

				var pause_location = await EvaluateAndCheck (
					"window.setTimeout(function() { invoke_method_with_structs(); }, 1);",
					debugger_test_loc, 16, 2, "MethodWithLocalStructs",
					locals_fn: (locals) => {
						Assert.Equal (3, locals.Count ());

						CheckValueType (locals, "ss_local", "DebuggerTests.ValueTypesTest.SimpleStruct");
						CheckValueType (locals, "gs_local", "DebuggerTests.ValueTypesTest.GenericStruct<DebuggerTests.ValueTypesTest>");
						CheckObject (locals, "vt_local", "DebuggerTests.ValueTypesTest");
					}
				);

				// Check ss_local's properties
				var ss_local_props = await CheckObjectOnFrameLocals (pause_location ["callFrames"][0], "ss_local",
					test_fn: (props) => CheckProps (props, "ss_local", new {
							str_member = TString ("set in MethodWithLocalStructs#SimpleStruct#str_member"),
							dt = TValueType ("System.DateTime"),
							gs = TValueType ("DebuggerTests.ValueTypesTest.GenericStruct<System.DateTime>"),
							Kind = TEnum ("System.DateTimeKind", "Utc")
						})
				);

				{
					// Check ss_local.dt
					await CheckDateTime (ss_local_props, "dt", new DateTime (2021, 2, 3, 4, 6, 7));

					// Check ss_local.gs
					await CheckObjectOnLocals (ss_local_props, "gs",
						test_fn: (props) => {
							CheckString (props, "StringField", "set in MethodWithLocalStructs#SimpleStruct#gs#StringField");
							CheckObject (props, "List", "System.Collections.Generic.List<System.DateTime>");
						}
					);
				}

				// Check gs_local's properties
				await CheckObjectOnFrameLocals (pause_location ["callFrames"][0], "gs_local",
					test_fn: (props) => CheckProps (props, "gs_local", new {
						StringField = TString ("gs_local#GenericStruct<ValueTypesTest>#StringField"),
						List        = TObject ("System.Collections.Generic.List<DebuggerTests.ValueTypesTest>", is_null: true),
						Options     = TEnum   ("DebuggerTests.Options", "None")
					})
				);

				// Check vt_local's properties
				var vt_local_props = await CheckObjectOnFrameLocals (pause_location ["callFrames"][0], "vt_local",
					test_fn: async (props) => {
						Assert.Equal (5, props.Count());

						CheckString (props, "StringField", "string#0");
						CheckValueType (props, "SimpleStructField", "DebuggerTests.ValueTypesTest.SimpleStruct");
						CheckValueType (props, "SimpleStructProperty", "DebuggerTests.ValueTypesTest.SimpleStruct");
						await CheckDateTime (props, "DT", new DateTime (2020, 1, 2, 3, 4, 5));
						CheckEnum (props, "RGB", "DebuggerTests.RGB", "Blue");
					}
				);

				{
					await CheckObjectOnLocals (vt_local_props, "SimpleStructProperty",
						test_fn: (props) => CheckProps (props, "SimpleStructProperty", new {
							str_member = TString ("SimpleStructProperty#string#0#SimpleStruct#str_member"),
							dt = TValueType ("System.DateTime"),
							gs = TValueType ("DebuggerTests.ValueTypesTest.GenericStruct<System.DateTime>"),
							Kind = TEnum ("System.DateTimeKind", "Utc")
						})
					);

					await CheckObjectOnLocals (vt_local_props, "SimpleStructField",
						test_fn: (props) => CheckProps (props, "SimpleStructField", new {
							str_member = TString ("SimpleStructField#string#0#SimpleStruct#str_member"),
							dt = TValueType ("System.DateTime"),
							gs = TValueType ("DebuggerTests.ValueTypesTest.GenericStruct<System.DateTime>"),
							Kind = TEnum ("System.DateTimeKind", "Local")
						})
					);
				}

				// FIXME: check ss_local.gs.List's members
			});
		}

		[Fact]
		public async Task InspectValueTypeMethodArgs () {
			var insp = new Inspector ();
			//Collect events
			var scripts = SubscribeToScripts(insp);

			await Ready();
			await insp.Ready (async (cli, token) => {
				ctx = new DebugTestContext (cli, insp, token, scripts);
				var debugger_test_loc = "dotnet://debugger-test.dll/debugger-valuetypes-test.cs";

				await SetBreakpoint (debugger_test_loc, 27, 3);


				var pause_location = await EvaluateAndCheck (
					"window.setTimeout(function() { invoke_static_method ('[debugger-test] DebuggerTests.ValueTypesTest:TestStructsAsMethodArgs'); }, 1);",
					debugger_test_loc, 27, 3, "MethodWithStructArgs",
					locals_fn: (locals) => {
						Assert.Equal (3, locals.Count ());

						CheckString (locals, "label", "TestStructsAsMethodArgs#label");
						CheckValueType (locals, "ss_arg", "DebuggerTests.ValueTypesTest.SimpleStruct");
						CheckNumber (locals, "x", 3);
					}
				);

				var ss_local_as_ss_arg = new {
					str_member = TString    ("ss_local#SimpleStruct#string#0#SimpleStruct#str_member"),
					dt         = TValueType ("System.DateTime"),
					gs         = TValueType ("DebuggerTests.ValueTypesTest.GenericStruct<System.DateTime>"),
					Kind       = TEnum      ("System.DateTimeKind", "Local")
				};
				var ss_local_gs = new {
					StringField = TString ("ss_local#SimpleStruct#string#0#SimpleStruct#gs#StringField"),
					List        = TObject ("System.Collections.Generic.List<System.DateTime>"),
					Options     = TEnum   ("DebuggerTests.Options", "Option1")
				};

				// Check ss_arg's properties
				var ss_arg_props = await CheckObjectOnFrameLocals (pause_location ["callFrames"][0], "ss_arg",
					test_fn: (props) => {
						Assert.Equal (4, props.Count());
						CheckProps (props, "ss_arg", ss_local_as_ss_arg);
					}
				);

				{
					// Check ss_local.dt
					await CheckDateTime (ss_arg_props, "dt", new DateTime (2025, 6, 7, 8, 10, 11));

					// Check ss_local.gs
					await CheckObjectOnLocals (ss_arg_props, "gs",
						test_fn: (props) => {
							CheckProps (props, "gs", ss_local_gs);
						}
					);
				}

				pause_location = await StepAndCheck (StepKind.Over, debugger_test_loc, 31, 3, "MethodWithStructArgs", times: 4,
					locals_fn: (locals) => {
						Assert.Equal (3, locals.Count());

						CheckString (locals, "label", "TestStructsAsMethodArgs#label");
						CheckValueType (locals, "ss_arg", "DebuggerTests.ValueTypesTest.SimpleStruct");
						CheckNumber (locals, "x", 3);

					}
				);

				var ss_arg_updated = new {
					str_member = TString    ("ValueTypesTest#MethodWithStructArgs#updated#ss_arg#str_member"),
					dt         = TValueType ("System.DateTime"),
					gs         = TValueType ("DebuggerTests.ValueTypesTest.GenericStruct<System.DateTime>"),
					Kind       = TEnum      ("System.DateTimeKind", "Utc")
				};

				ss_arg_props = await CheckObjectOnFrameLocals (pause_location ["callFrames"][0], "ss_arg",
					test_fn: (props) => {
						CheckProps (props, "ss_arg", ss_arg_updated);
					}
				);

				{
					// Check ss_local.gs
					await CheckObjectOnLocals (ss_arg_props, "gs",
						test_fn: (props) => {
							CheckProps (props, "gs", new {
									StringField = TString ("ValueTypesTest#MethodWithStructArgs#updated#gs#StringField#3"),
									List        = TObject ("System.Collections.Generic.List<System.DateTime>"),
									Options     = TEnum   ("DebuggerTests.Options", "Option1")
							});
						}
					);
				}

				// Check locals on previous frame, same as earlier in this test
				ss_arg_props = await CheckObjectOnFrameLocals (pause_location ["callFrames"][1], "ss_local",
					test_fn: (props) => {
						CheckProps (props, "ss_local_frame_1", ss_local_as_ss_arg);
					}
				);
				{
					// Check ss_local.dt
					await CheckDateTime (ss_arg_props, "dt", new DateTime (2025, 6, 7, 8, 10, 11));

					// Check ss_local.gs
					await CheckObjectOnLocals (ss_arg_props, "gs",
						test_fn: (props) => {
							CheckString (props, "StringField", "ss_local#SimpleStruct#string#0#SimpleStruct#gs#StringField");
							CheckObject (props, "List", "System.Collections.Generic.List<System.DateTime>");
						}
					);
				}

				// ----------- Step back to the caller ---------

				pause_location = await StepAndCheck (StepKind.Over, debugger_test_loc, 22, 3, "TestStructsAsMethodArgs", times: 2,
					locals_fn: (locals) => CheckProps (locals, "locals#0", new {
							ss_local =  TValueType ("DebuggerTests.ValueTypesTest.SimpleStruct"),
							ss_ret   =  TValueType ("DebuggerTests.ValueTypesTest.SimpleStruct")
							})
				);

				ss_arg_props = await CheckObjectOnFrameLocals (pause_location ["callFrames"] [0], "ss_local",
					test_fn: (props) => CheckProps (props, "ss_local", ss_local_as_ss_arg)
				);

				{
					// Check ss_local.gs
					await CheckObjectOnLocals (ss_arg_props, "gs",
						test_fn: (props) => CheckProps (props, "ss_local_gs", ss_local_gs)
					);
				}

				// FIXME: check ss_local.gs.List's members
			});
		}

		async Task<JObject> StepAndCheck (StepKind kind, string script_loc, int line, int column, string function_name,
							Func<JObject, Task> wait_for_event_fn = null, Action<JToken> locals_fn = null, int times=1)
		{
			for (int i = 0; i < times - 1; i ++) {
				await SendCommandAndCheck (null, $"Debugger.step{kind.ToString ()}", null, -1, -1, null);
			}

			// Check for method/line etc only at the last step
			return await SendCommandAndCheck (
						null, $"Debugger.step{kind.ToString ()}", script_loc, line, column, function_name,
						wait_for_event_fn: wait_for_event_fn,
						locals_fn: locals_fn);
		}

		async Task<JObject> EvaluateAndCheck (string expression, string script_loc, int line, int column, string function_name,
								Func<JObject, Task> wait_for_event_fn = null, Action<JToken> locals_fn = null)
			=> await SendCommandAndCheck (
						JObject.FromObject (new { expression = expression }),
						"Runtime.evaluate", script_loc, line, column, function_name,
						wait_for_event_fn: wait_for_event_fn,
						locals_fn: locals_fn);

		async Task<JObject> SendCommandAndCheck (JObject args, string method, string script_loc, int line, int column, string function_name,
								Func<JObject, Task> wait_for_event_fn = null, Action<JToken> locals_fn = null, string waitForEvent = Inspector.PAUSE)
		{
			var res = await ctx.cli.SendCommand (method, args, ctx.token);
			if (!res.IsOk) {
				Console.WriteLine ($"Failed to run command {method} with args: {args?.ToString ()}\nresult: {res.Error.ToString ()}");
				Assert.True (false, $"SendCommand for {method} failed with {res.Error.ToString ()}");
			}

			var wait_res = await ctx.insp.WaitFor(waitForEvent);

			if (function_name != null)
				Assert.Equal (function_name, wait_res ["callFrames"]?[0]?["functionName"]?.Value<string> ());

			if (script_loc != null)
				CheckLocation (script_loc, line, column, ctx.scripts, wait_res ["callFrames"][0]["location"]);

			if (wait_for_event_fn != null)
				await wait_for_event_fn (wait_res);

			if (locals_fn != null)
				await CheckLocalsOnFrame (wait_res ["callFrames"][0], locals_fn);

			return wait_res;
		}

		void CheckProps (JToken actual, string label, object exp_o, int num_fields=-1)
		{
			var exp = exp_o as JObject;
			if (exp == null)
				exp = JObject.FromObject (exp_o);

			num_fields = num_fields < 0 ? exp.Values<JToken> ().Count () : num_fields;
			Assert.True (num_fields == actual.Count (), $"[{label}] Number of fields don't match, Expected: {num_fields}, Actual: {actual.Count ()}");

			foreach (var kvp in exp) {
				var exp_name = kvp.Key;
				var exp_val = kvp.Value;

				var actual_obj = actual.FirstOrDefault (jt => jt ["name"]?.Value<string> () == exp_name);
				Assert.True (actual_obj != null, $"[{label}] Could not find property named '{exp_name}");

				var actual_val = actual_obj ["value"];
				Assert.True (actual_obj != null, $"[{label}] not value found for property named '{exp_name}'");

				foreach (var jp in exp_val.Values<JProperty> ()) {
					Console.WriteLine ($"jp: {jp}");
					var actual_field_val = actual_val.Values<JProperty> ().FirstOrDefault (a_jp => a_jp.Name == jp.Name);
					Assert.True (actual_field_val != null, $"[{label}] Could not find value field named {jp.Name}, for property named {exp_name}");

					Assert.True (jp.Value.Value<string> () == actual_field_val.Value.Value<string> (),
							$"[{label}] Value for field named {exp_name}'s json property named {jp.Name} didn't match.\n" +
							$"Expected: {jp.Value.Value<string> ()}\n" +
							$"Actual:   {actual_field_val.Value.Value<string> ()}");
				}
			}
		}

		async Task CheckLocalsOnFrame (JToken frame, string script_loc, int line, int column, string function_name, Action<JToken> test_fn = null)
		{
			CheckLocation (script_loc, line, column, ctx.scripts, frame ["location"]);
			Assert.Equal (function_name, frame ["functionName"].Value<string> ());

			await CheckLocalsOnFrame (frame, test_fn);
		}

		async Task CheckLocalsOnFrame (JToken frame, Action<JToken> test_fn)
		{
			var locals = await GetProperties (frame ["callFrameId"].Value<string> ());
			try {
				test_fn (locals);
			} catch {
				Console.WriteLine ($"CheckLocalsOnFrame failed for locals: {locals}");
				throw;
			}
		}

		async Task<JToken> CheckObjectOnFrameLocals (JToken frame, string name, Action<JToken> test_fn)
		{
			var locals = await GetProperties (frame ["callFrameId"].Value<string> ());
			return await CheckObjectOnLocals (locals, name, test_fn);
		}

		async Task<JToken> CheckObjectOnLocals (JToken locals, string name, Action<JToken> test_fn)
		{
			var obj = locals.Where (jt => jt ["name"]?.Value<string> () == name)
					.FirstOrDefault ();
			if (obj == null) {
				Console.WriteLine ($"CheckObjectOnLocals failed with locals: {locals}");
				Assert.True (false, $"Could not find a var with name {name} and type object");
			}

			var props = await GetProperties (obj ["value"]["objectId"].Value<string> ());
			if (test_fn != null) {
				try {
					test_fn (props);
				} catch (Exception) {
					Console.WriteLine ($"Failed for properties: {props}");
					throw;
				}
			}

			return props;
		}

		async Task<JToken> GetProperties (string id)
		{
			var get_prop_req = JObject.FromObject (new {
				objectId = id
			});

			var frame_props = await ctx.cli.SendCommand ("Runtime.getProperties", get_prop_req, ctx.token);
			if (!frame_props.IsOk)
				Assert.True (false, $"Runtime.getProperties failed for {get_prop_req.ToString ()}, with Result: {frame_props}");

			var locals = frame_props.Value ["result"];
			return locals;
		}

		async Task<Result> SetBreakpoint (string url_key, int line, int column, bool expect_ok=true)
		{
			var bp1_req = JObject.FromObject(new {
				lineNumber = line,
				columnNumber = column,
				url = dicFileToUrl[url_key],
			});

			var bp1_res = await ctx.cli.SendCommand ("Debugger.setBreakpointByUrl", bp1_req, ctx.token);
			Assert.True (expect_ok ? bp1_res.IsOk : bp1_res.IsErr);

			return bp1_res;
		}

		static JObject TString (string value) =>
			JObject.FromObject (new { type = "string", value = @value, description = @value });

		static JObject TValueType (string className, object members = null) =>
			JObject.FromObject (new { type = "object", isValueType = true, className = className, description = className });

		static JObject TEnum (string className, string descr, object members = null) =>
			JObject.FromObject (new { type = "object", isEnum = true, className = className, description = descr });

		static JObject TObject (string className, bool is_null = false) =>
			is_null
				? JObject.FromObject (new { type = "object", className = className, description = className, subtype = is_null ? "null" : null })
				: JObject.FromObject (new { type = "object", className = className, description = className });

		//TODO add tests covering basic stepping behavior as step in/out/over
	}

	class DebugTestContext
	{
		public InspectorClient cli;
		public Inspector insp;
		public CancellationToken token;
		public Dictionary<string, string> scripts;

		public DebugTestContext (InspectorClient cli, Inspector insp, CancellationToken token, Dictionary<string, string> scripts)
		{
				this.cli = cli;
				this.insp = insp;
				this.token = token;
				this.scripts = scripts;
		}
	}

	enum StepKind
	{
		Into,
		Over,
		Out
	}
}
