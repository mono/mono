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

		void CheckContentValue (JToken token, string value) {
			var val = token["value"].Value<string> ();
			Assert.Equal (value, val);
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
				
				await CheckLocalsOnFrame (wait_res ["callFrames"][2],
					test_fn: (locals) => {
						Assert.Equal (4, locals.Count());
						CheckString (locals, "ls", "string from jstest");
						CheckNumber (locals, "li", 52);
				});

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
				var ss_local_props = await CompareObjectPropertiesOnFrameLocals (pause_location ["callFrames"][0], "ss_local",
						new {
							str_member = TString ("set in MethodWithLocalStructs#SimpleStruct#str_member"),
							dt = TValueType ("System.DateTime"),
							gs = TValueType ("DebuggerTests.ValueTypesTest.GenericStruct<System.DateTime>"),
							Kind = TEnum ("System.DateTimeKind", "Utc")
						});

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
				await CompareObjectPropertiesOnFrameLocals (pause_location ["callFrames"][0], "gs_local",
					new {
						StringField = TString ("gs_local#GenericStruct<ValueTypesTest>#StringField"),
						List        = TObject ("System.Collections.Generic.List<DebuggerTests.ValueTypesTest>", is_null: true),
						Options     = TEnum   ("DebuggerTests.Options", "None")
					});

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
					await CompareObjectPropertiesFor (vt_local_props, "SimpleStructProperty", 
						new {
							str_member = TString ("SimpleStructProperty#string#0#SimpleStruct#str_member"),
							dt = TValueType ("System.DateTime"),
							gs = TValueType ("DebuggerTests.ValueTypesTest.GenericStruct<System.DateTime>"),
							Kind = TEnum ("System.DateTimeKind", "Utc")
						},
						label: "vt_local_props.SimpleStructProperty");

					await CompareObjectPropertiesFor (vt_local_props, "SimpleStructField",
						new {
							str_member = TString ("SimpleStructField#string#0#SimpleStruct#str_member"),
							dt = TValueType ("System.DateTime"),
							gs = TValueType ("DebuggerTests.ValueTypesTest.GenericStruct<System.DateTime>"),
							Kind = TEnum ("System.DateTimeKind", "Local")
						},
						label: "vt_local_props.SimpleStructField");
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
				var ss_arg_props = await CompareObjectPropertiesOnFrameLocals (pause_location ["callFrames"][0],
								"ss_arg", ss_local_as_ss_arg);

				{
					// Check ss_local.dt
					await CheckDateTime (ss_arg_props, "dt", new DateTime (2025, 6, 7, 8, 10, 11));

					// Check ss_local.gs
					await CompareObjectPropertiesFor (ss_arg_props, "gs", ss_local_gs);
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

				ss_arg_props = await CompareObjectPropertiesOnFrameLocals (pause_location ["callFrames"][0],
							"ss_arg", ss_arg_updated);

				{
					// Check ss_local.gs
					await CompareObjectPropertiesFor (ss_arg_props, "gs", new {
									StringField = TString ("ValueTypesTest#MethodWithStructArgs#updated#gs#StringField#3"),
									List        = TObject ("System.Collections.Generic.List<System.DateTime>"),
									Options     = TEnum   ("DebuggerTests.Options", "Option1")
							});
				}

				// Check locals on previous frame, same as earlier in this test
				ss_arg_props = await CompareObjectPropertiesOnFrameLocals (pause_location ["callFrames"][1],
						"ss_local", ss_local_as_ss_arg);

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

				pause_location = await StepAndCheck (StepKind.Over, debugger_test_loc, 22, 3, "TestStructsAsMethodArgs",
							times: 2, locals_fn: (l) => { /* non-null to make sure that locals get fetched */} );
				await CheckLocalsOnFrame (pause_location ["callFrames"][0], new {
							ss_local =  TValueType ("DebuggerTests.ValueTypesTest.SimpleStruct"),
							ss_ret   =  TValueType ("DebuggerTests.ValueTypesTest.SimpleStruct")
							}, "locals#0");

				ss_arg_props = await CompareObjectPropertiesOnFrameLocals (pause_location ["callFrames"] [0],
							"ss_local", ss_local_as_ss_arg);

				{
					// Check ss_local.gs
					await CompareObjectPropertiesFor (ss_arg_props, "gs", ss_local_gs, label: "ss_local_gs");
				}

				// FIXME: check ss_local.gs.List's members
			});
		}

		[Fact]
		public async Task InspectLocalsWithStructsStaticAsync () {
			var insp = new Inspector ();
			//Collect events
			var scripts = SubscribeToScripts(insp);

			await Ready();
			await insp.Ready (async (cli, token) => {
				ctx = new DebugTestContext (cli, insp, token, scripts);
				var debugger_test_loc = "dotnet://debugger-test.dll/debugger-valuetypes-test.cs";

				await SetBreakpoint (debugger_test_loc, 47, 3);

				var pause_location = await EvaluateAndCheck (
					"window.setTimeout(function() { invoke_static_method_async ("
						+ "'[debugger-test] DebuggerTests.ValueTypesTest:MethodWithLocalStructsStaticAsync'"
					+ "); }, 1);",
					debugger_test_loc, 47, 3, "MoveNext"); //BUG: method name

				await CheckLocalsOnFrame (pause_location ["callFrames"][0],
						new {
							ss_local = TObject ("DebuggerTests.ValueTypesTest.SimpleStruct"),
							gs_local = TValueType ("DebuggerTests.ValueTypesTest.GenericStruct<int>"),
							result   = TBool (true)
						},
						"locals#0");

				// Check ss_local's properties
				var ss_local_props = await CompareObjectPropertiesOnFrameLocals (pause_location ["callFrames"][0], "ss_local",
					new {
						str_member = TString ("set in MethodWithLocalStructsStaticAsync#SimpleStruct#str_member"),
						dt         = TValueType ("System.DateTime"),
						gs         = TValueType ("DebuggerTests.ValueTypesTest.GenericStruct<System.DateTime>"),
						Kind       = TEnum ("System.DateTimeKind", "Utc")
					});

				{
					// Check ss_local.dt
					await CheckDateTime (ss_local_props, "dt", new DateTime (2021, 2, 3, 4, 6, 7));

					// Check ss_local.gs
					await CompareObjectPropertiesFor (ss_local_props, "gs",
						new {
							StringField = TString ("set in MethodWithLocalStructsStaticAsync#SimpleStruct#gs#StringField"),
							List        = TObject ("System.Collections.Generic.List<System.DateTime>"),
							Options     = TEnum   ("DebuggerTests.Options", "Option1")
						}
					);
				}

				// Check gs_local's properties
				await CompareObjectPropertiesOnFrameLocals (pause_location ["callFrames"][0], "gs_local",
					new {
						StringField = TString ("gs_local#GenericStruct<ValueTypesTest>#StringField"),
						List        = TObject ("System.Collections.Generic.List<int>"),
						Options     = TEnum   ("DebuggerTests.Options", "Option2")
					});

				// FIXME: check ss_local.gs.List's members
			});
		}

		[Theory]
		[InlineData (16, 2, "PrimitiveTypeLocals", false, 0)]
		[InlineData (93, 2, "YetAnotherMethod", true, 2)]
		public async Task InspectPrimitiveTypeArrayLocals (int line, int col, string method_name, bool test_prev_frame, int frame_idx)
			=> await TestSimpleArrayLocals (
				line, col,
				entry_method_name: "[debugger-test] DebuggerTests.ArrayTestsClass:PrimitiveTypeLocals",
				method_name: method_name,
				etype_name: "int",
				local_var_name_prefix: "int",
				array: new [] { TNumber (4), TNumber (70), TNumber (1) },
				array_elements: null,
				test_prev_frame: test_prev_frame,
				frame_idx: frame_idx);

		static Func<int, int, string, string, object> TSimpleClass = (X, Y, Id, Color) => new {
			X =     TNumber (X),
			Y =     TNumber (Y),
			Id =    TString (Id),
			Color = TEnum ("DebuggerTests.RGB", Color),
			//PointWithCustomGetter = TValueType ("DebuggerTests.Point")
			// only automatic properties are supported currently!
			PointWithCustomGetter = TString ("DebuggerTests.Point")
		};

		static Func<int, int, string, string, object> TPoint = (X, Y, Id, Color) => new {
			X =     TNumber (X),
			Y =     TNumber (Y),
			Id =    TString (Id),
			Color = TEnum ("DebuggerTests.RGB", Color),
		};

		[Theory]
		[InlineData (32, 2, "ValueTypeLocals", false, 0)]
		[InlineData (93, 2, "YetAnotherMethod", true, 2)]
		public async Task InspectValueTypeArrayLocals (int line, int col, string method_name, bool test_prev_frame, int frame_idx)
			=> await TestSimpleArrayLocals (
				line, col,
				entry_method_name: "[debugger-test] DebuggerTests.ArrayTestsClass:ValueTypeLocals",
				method_name: method_name,
				etype_name: "DebuggerTests.Point",
				local_var_name_prefix: "point",
				array: new [] {
					TValueType ("DebuggerTests.Point"),
					TValueType ("DebuggerTests.Point"),
				},
				array_elements: new [] {
					TPoint (5, -2, "point_arr#Id#0", "Green"),
					TPoint (123, 0, "point_arr#Id#1", "Blue")
				},
				test_prev_frame: test_prev_frame,
				frame_idx: frame_idx);

		[Theory]
		[InlineData (49, 2, "ObjectTypeLocals", false, 0)]
		[InlineData (93, 2, "YetAnotherMethod", true, 2)]
		public async Task InspectObjectArrayLocals (int line, int col, string method_name, bool test_prev_frame, int frame_idx)
			=> await TestSimpleArrayLocals (
				line, col,
				entry_method_name: "[debugger-test] DebuggerTests.ArrayTestsClass:ObjectTypeLocals",
				method_name: method_name,
				etype_name: "DebuggerTests.SimpleClass",
				local_var_name_prefix: "class",
				array: new [] {
					TObject ("DebuggerTests.SimpleClass"),
					TObject ("DebuggerTests.SimpleClass", is_null: true),
					TObject ("DebuggerTests.SimpleClass")
				},
				array_elements: new [] {
					TSimpleClass (5, -2, "class_arr#Id#0", "Green"),
					null, // Element is null
					TSimpleClass (123, 0, "class_arr#Id#2", "Blue") },
				test_prev_frame: test_prev_frame,
				frame_idx: frame_idx);

		[Theory]
		[InlineData (66, 2, "GenericTypeLocals", false, 0)]
		[InlineData (93, 2, "YetAnotherMethod", true, 2)]
		public async Task InspectGenericTypeArrayLocals (int line, int col, string method_name, bool test_prev_frame, int frame_idx)
			=> await TestSimpleArrayLocals (
				line, col,
				entry_method_name: "[debugger-test] DebuggerTests.ArrayTestsClass:GenericTypeLocals",
				method_name: method_name,
				etype_name: "DebuggerTests.GenericClass<int>",
				local_var_name_prefix: "gclass",
				array: new [] {
					TObject ("DebuggerTests.GenericClass<int>", is_null: true),
					TObject ("DebuggerTests.GenericClass<int>"),
					TObject ("DebuggerTests.GenericClass<int>")
				},
				array_elements: new [] {
					null, // Element is null
					new {
						Id = TString ("gclass_arr#1#Id"),
						Color = TEnum ("DebuggerTests.RGB", "Red"),
						Value = TNumber (5)
					},
					new {
						Id = TString ("gclass_arr#2#Id"),
						Color = TEnum ("DebuggerTests.RGB", "Blue"),
						Value = TNumber (-12)
					}
				},
				test_prev_frame: test_prev_frame,
				frame_idx: frame_idx);

		[Theory]
		[InlineData (82, 2, "GenericValueTypeLocals", false, 0)]
		[InlineData (93, 2, "YetAnotherMethod", true, 2)]
		public async Task InspectGenericValueTypeArrayLocals (int line, int col, string method_name, bool test_prev_frame, int frame_idx)
			=> await TestSimpleArrayLocals (
				line, col,
				entry_method_name: "[debugger-test] DebuggerTests.ArrayTestsClass:GenericValueTypeLocals",
				method_name: method_name,
				etype_name: "DebuggerTests.SimpleGenericStruct<DebuggerTests.Point>",
				local_var_name_prefix: "gvclass",
				array: new [] {
					TValueType ("DebuggerTests.SimpleGenericStruct<DebuggerTests.Point>"),
					TValueType ("DebuggerTests.SimpleGenericStruct<DebuggerTests.Point>")
				},
				array_elements: new [] {
					new {
						Id = TString ("gvclass_arr#1#Id"),
						Color = TEnum ("DebuggerTests.RGB", "Red"),
						Value = TPoint (100, 200, "gvclass_arr#1#Value#Id", "Red")
					},
					new {
						Id = TString ("gvclass_arr#2#Id"),
						Color = TEnum ("DebuggerTests.RGB", "Blue"),
						Value = TPoint (10, 20, "gvclass_arr#2#Value#Id", "Green")
					}
				},
				test_prev_frame: test_prev_frame,
				frame_idx: frame_idx);

		[Theory]
		[InlineData (191, 2, "GenericValueTypeLocals2", false, 0)]
		[InlineData (93, 2, "YetAnotherMethod", true, 2)]
		public async Task InspectGenericValueTypeArrayLocals2 (int line, int col, string method_name, bool test_prev_frame, int frame_idx)
			=> await TestSimpleArrayLocals (
				line, col,
				entry_method_name: "[debugger-test] DebuggerTests.ArrayTestsClass:GenericValueTypeLocals2",
				method_name: method_name,
				etype_name: "DebuggerTests.SimpleGenericStruct<DebuggerTests.Point[]>",
				local_var_name_prefix: "gvclass",
				array: new [] {
					TValueType ("DebuggerTests.SimpleGenericStruct<DebuggerTests.Point[]>"),
					TValueType ("DebuggerTests.SimpleGenericStruct<DebuggerTests.Point[]>")
				},
				array_elements: new [] {
					new {
						Id = TString ("gvclass_arr#0#Id"),
						Color = TEnum ("DebuggerTests.RGB", "Red"),
						Value = new [] {
							TPoint (100, 200, "gvclass_arr#0#0#Value#Id", "Red"),
							TPoint (100, 200, "gvclass_arr#0#1#Value#Id", "Green")
						}
					},
					new {
						Id = TString ("gvclass_arr#1#Id"),
						Color = TEnum ("DebuggerTests.RGB", "Blue"),
						Value = new [] {
							TPoint (100, 200, "gvclass_arr#1#0#Value#Id", "Green"),
							TPoint (100, 200, "gvclass_arr#1#1#Value#Id", "Blue")
						}
					}
				},
				test_prev_frame: test_prev_frame,
				frame_idx: frame_idx);

		async Task TestSimpleArrayLocals (int line, int col, string entry_method_name, string method_name, string etype_name,
							string local_var_name_prefix, object[] array, object[] array_elements,
							bool test_prev_frame=false, int frame_idx=0)
		{
			var insp = new Inspector ();
			//Collect events
			var scripts = SubscribeToScripts(insp);

			await Ready();
			await insp.Ready (async (cli, token) => {
				ctx = new DebugTestContext (cli, insp, token, scripts);
				var debugger_test_loc = "dotnet://debugger-test.dll/debugger-array-test.cs";

				await SetBreakpoint (debugger_test_loc, line, col);

				var eval_expr = "window.setTimeout(function() { invoke_static_method ("
							+ $"'{entry_method_name}', { (test_prev_frame ? "true" : "false") }"
						+ "); }, 1);";

				var pause_location = await EvaluateAndCheck (eval_expr, debugger_test_loc, line, col, method_name);
				await CheckLocalsOnFrame (pause_location ["callFrames"][frame_idx],
					test_fn: (locals) => {
						Assert.Equal (4, locals.Count ());
						CheckArray (locals, $"{local_var_name_prefix}_arr", $"{etype_name}[]");
						CheckArray (locals, $"{local_var_name_prefix}_arr_empty", $"{etype_name}[]");
						CheckObject (locals, $"{local_var_name_prefix}_arr_null", $"{etype_name}[]", is_null: true);
						CheckBool (locals, "call_other", test_prev_frame);
				});

				var prefix_arr = await CompareObjectPropertiesOnFrameLocals (pause_location ["callFrames"][frame_idx],
							$"{local_var_name_prefix}_arr", array);

				if (array_elements?.Length > 0) {
					for (int i = 0; i < array_elements.Length; i ++) {
						var i_str = $"[{i}]";
						var label = $"{local_var_name_prefix}_arr[{i}]";
						if (array_elements [i] == null) {
							var act_i = prefix_arr.FirstOrDefault (jt => jt ["name"]?.Value<string> () == i_str);
							Assert.True (act_i != null, $"[{label}] Couldn't find array element {i_str}");

							await CheckValue (act_i ["value"], TObject (etype_name, is_null: true), label);
						} else {
							await CompareObjectPropertiesFor (prefix_arr, i_str, array_elements [i], label: label);
						}
					}
				}

				await CompareObjectPropertiesOnFrameLocals (pause_location ["callFrames"][frame_idx],
						$"{local_var_name_prefix}_arr_empty", new object[0]);
			});
		}

		[Fact]
		public async Task InspectObjectArrayMembers ()
		{
			var insp = new Inspector ();
			//Collect events
			var scripts = SubscribeToScripts(insp);
			int line = 205;
			int col = 3;
			string entry_method_name = "[debugger-test] DebuggerTests.ArrayTestsClass:ObjectArrayMembers";
			string method_name = "PlaceholderMethod";
			int frame_idx = 1;

			await Ready();
			await insp.Ready (async (cli, token) => {
				ctx = new DebugTestContext (cli, insp, token, scripts);
				var debugger_test_loc = "dotnet://debugger-test.dll/debugger-array-test.cs";

				await SetBreakpoint (debugger_test_loc, line, col);

				var eval_expr = "window.setTimeout(function() { invoke_static_method ("
							+ $"'{entry_method_name}'"
						+ "); }, 1);";

				var pause_location = await EvaluateAndCheck (eval_expr, debugger_test_loc, line, col, method_name);
				await CheckLocalsOnFrame (pause_location ["callFrames"][frame_idx],
					test_fn: (locals) => {
						Assert.Single (locals);
						CheckObject (locals, "c", "DebuggerTests.Container");
				});

				var c_props = await CompareObjectPropertiesOnFrameLocals (pause_location ["callFrames"][frame_idx],
					"c", new {
						id = TString ("c#id"),
						ClassArrayProperty = TArray ("DebuggerTests.SimpleClass[]"),
						ClassArrayField = TArray ("DebuggerTests.SimpleClass[]"),
						PointsProperty = TArray ("DebuggerTests.Point[]"),
						PointsField = TArray ("DebuggerTests.Point[]")
					}
				);

				await CompareObjectPropertiesFor (c_props, "ClassArrayProperty",
					new [] {
						TSimpleClass (5, -2, "ClassArrayProperty#Id#0", "Green"),
						TSimpleClass (30, 1293, "ClassArrayProperty#Id#1", "Green"),
						TObject ("DebuggerTests.SimpleClass", is_null: true)
					},
					label: "InspectLocalsWithStructsStaticAsync");

				await CompareObjectPropertiesFor (c_props, "ClassArrayField",
					new [] {
						TObject ("DebuggerTests.SimpleClass", is_null: true),
						TSimpleClass (5, -2, "ClassArrayField#Id#1", "Blue"),
						TSimpleClass (30, 1293, "ClassArrayField#Id#2", "Green")
					},
					label: "c#ClassArrayField");

				await CompareObjectPropertiesFor (c_props, "PointsProperty",
					new [] {
						TPoint (5, -2, "PointsProperty#Id#0", "Green"),
						TPoint (123, 0, "PointsProperty#Id#1", "Blue"),
					},
					label: "c#PointsProperty");

				await CompareObjectPropertiesFor (c_props, "PointsField",
					new [] {
						TPoint (5, -2, "PointsField#Id#0", "Green"),
						TPoint (123, 0, "PointsField#Id#1", "Blue"),
					},
					label: "c#PointsField");
			});
		}

		[Fact]
		public async Task InspectValueTypeArrayLocalsStaticAsync ()
		{
			var insp = new Inspector ();
			//Collect events
			var scripts = SubscribeToScripts(insp);
			int line = 143;
			int col = 3;
			string entry_method_name = "[debugger-test] DebuggerTests.ArrayTestsClass:ValueTypeLocalsAsync";
			string method_name = "MoveNext"; // BUG: this should be ValueTypeLocalsAsync
			int frame_idx = 0;

			await Ready();
			await insp.Ready (async (cli, token) => {
				ctx = new DebugTestContext (cli, insp, token, scripts);
				var debugger_test_loc = "dotnet://debugger-test.dll/debugger-array-test.cs";

				await SetBreakpoint (debugger_test_loc, line, col);

				var eval_expr = "window.setTimeout(function() { invoke_static_method_async ("
							+ $"'{entry_method_name}', false" // *false* here keeps us only in the static method
						+ "); }, 1);";

				var pause_location = await EvaluateAndCheck (eval_expr, debugger_test_loc, line, col, method_name);
				var frame_locals = await CheckLocalsOnFrame (pause_location ["callFrames"][frame_idx],
					new {
						call_other = TBool (false),
						gvclass_arr = TArray ("DebuggerTests.SimpleGenericStruct<DebuggerTests.Point>[]"),
						gvclass_arr_empty = TArray ("DebuggerTests.SimpleGenericStruct<DebuggerTests.Point>[]"),
						gvclass_arr_null = TObject ("DebuggerTests.SimpleGenericStruct<DebuggerTests.Point>[]", is_null: true),
						gvclass = TValueType ("DebuggerTests.SimpleGenericStruct<DebuggerTests.Point>"),
						// BUG: this shouldn't be null!
						points = TObject ("DebuggerTests.Point[]", is_null: true)
					},
					"ValueTypeLocalsAsync#locals"
				);

				var local_var_name_prefix = "gvclass";
				await CompareObjectPropertiesFor (frame_locals, local_var_name_prefix, new {
						Id = TString (null),
						Color = TEnum ("DebuggerTests.RGB", "Red"),
						Value = TPoint (0, 0, null, "Red")
					});

				await CompareObjectPropertiesFor (frame_locals, $"{local_var_name_prefix}_arr",
					new [] {
						new {
							Id = TString ("gvclass_arr#1#Id"),
							Color = TEnum ("DebuggerTests.RGB", "Red"),
							Value = TPoint (100, 200, "gvclass_arr#1#Value#Id", "Red")
						},
						new {
							Id = TString ("gvclass_arr#2#Id"),
							Color = TEnum ("DebuggerTests.RGB", "Blue"),
							Value = TPoint (10, 20, "gvclass_arr#2#Value#Id", "Green")
						}
					}
				);
				await CompareObjectPropertiesFor (frame_locals, $"{local_var_name_prefix}_arr_empty",
						new object[0]);
			});
		}

		// TODO: Check previous frame too
		[Fact]
		public async Task InspectValueTypeArrayLocalsInstanceAsync ()
		{
			var insp = new Inspector ();
			//Collect events
			var scripts = SubscribeToScripts(insp);
			int line = 155;
			int col = 3;
			string entry_method_name = "[debugger-test] DebuggerTests.ArrayTestsClass:ValueTypeLocalsAsync";
			int frame_idx = 0;

			await Ready();
			await insp.Ready (async (cli, token) => {
				ctx = new DebugTestContext (cli, insp, token, scripts);
				var debugger_test_loc = "dotnet://debugger-test.dll/debugger-array-test.cs";

				await SetBreakpoint (debugger_test_loc, line, col);

				var eval_expr = "window.setTimeout(function() { invoke_static_method_async ("
							+ $"'{entry_method_name}', true"
						+ "); }, 1);";

				// BUG: Should be InspectValueTypeArrayLocalsInstanceAsync
				var pause_location = await EvaluateAndCheck (eval_expr, debugger_test_loc, line, col, "MoveNext");

				var frame_locals = await CheckLocalsOnFrame (pause_location ["callFrames"][frame_idx],
					new {
						t1 = TObject ("DebuggerTests.SimpleGenericStruct<DebuggerTests.Point>"),
						@this = TObject ("DebuggerTests.ArrayTestsClass"),
						point_arr = TArray ("DebuggerTests.Point[]"),
						point = TValueType ("DebuggerTests.Point")
					},
					"InspectValueTypeArrayLocalsInstanceAsync#locals"
				);

				await CompareObjectPropertiesFor (frame_locals, "t1",
					new {
						Id = TString ("gvclass_arr#1#Id"),
						Color = TEnum ("DebuggerTests.RGB", "Red"),
						Value = TPoint (100, 200, "gvclass_arr#1#Value#Id", "Red")
					});

				await CompareObjectPropertiesFor (frame_locals, "point_arr",
					new [] {
						TPoint (5, -2, "point_arr#Id#0", "Red"),
						TPoint (123, 0, "point_arr#Id#1", "Blue"),
					}
				);

				await CompareObjectPropertiesFor (frame_locals, "point",
						TPoint (45, 51, "point#Id", "Green"));
			});
		}

		[Fact]
		public async Task InspectValueTypeArrayLocalsInAsyncStaticStructMethod ()
		{
			var insp = new Inspector ();
			//Collect events
			var scripts = SubscribeToScripts(insp);
			int line = 222;
			int col = 3;
			string entry_method_name = "[debugger-test] DebuggerTests.ArrayTestsClass:EntryPointForStructMethod";
			int frame_idx = 0;

			await Ready();
			await insp.Ready (async (cli, token) => {
				ctx = new DebugTestContext (cli, insp, token, scripts);
				var debugger_test_loc = "dotnet://debugger-test.dll/debugger-array-test.cs";

				await SetBreakpoint (debugger_test_loc, line, col);
				//await SetBreakpoint (debugger_test_loc, 143, 3);

				var eval_expr = "window.setTimeout(function() { invoke_static_method_async ("
							+ $"'{entry_method_name}', false"
						+ "); }, 1);";

				// BUG: Should be InspectValueTypeArrayLocalsInstanceAsync
				var pause_location = await EvaluateAndCheck (eval_expr, debugger_test_loc, line, col, "MoveNext");

				var frame_locals = await CheckLocalsOnFrame (pause_location ["callFrames"][frame_idx],
					new {
						call_other = TBool (false),
						local_i  = TNumber (5),
						sc = TSimpleClass (10, 45, "sc#Id", "Blue")
					},
					"InspectValueTypeArrayLocalsInAsyncStaticStructMethod#locals"
				);
			});
		}

		[Fact]
		public async Task InspectValueTypeArrayLocalsInAsyncInstanceStructMethod ()
		{
			var insp = new Inspector ();
			//Collect events
			var scripts = SubscribeToScripts(insp);
			int line = 229;
			int col = 3;
			string entry_method_name = "[debugger-test] DebuggerTests.ArrayTestsClass:EntryPointForStructMethod";
			int frame_idx = 0;

			await Ready();
			await insp.Ready (async (cli, token) => {
				ctx = new DebugTestContext (cli, insp, token, scripts);
				var debugger_test_loc = "dotnet://debugger-test.dll/debugger-array-test.cs";

				await SetBreakpoint (debugger_test_loc, line, col);

				var eval_expr = "window.setTimeout(function() { invoke_static_method_async ("
							+ $"'{entry_method_name}', true"
						+ "); }, 1);";

				// BUG: Should be InspectValueTypeArrayLocalsInstanceAsync
				var pause_location = await EvaluateAndCheck (eval_expr, debugger_test_loc, line, col, "MoveNext");

				var frame_locals = await CheckLocalsOnFrame (pause_location ["callFrames"][frame_idx],
					new {
						sc_arg = TObject ("DebuggerTests.SimpleClass"),
						@this = TValueType ("DebuggerTests.Point"),
						local_gs = TValueType ("DebuggerTests.SimpleGenericStruct<int>")
					},
					"locals#0");

				await CompareObjectPropertiesFor (frame_locals, "local_gs",
					new {
						Id = TString ("local_gs#Id"),
						Color = TEnum ("DebuggerTests.RGB", "Green"),
						Value = TNumber (4)
					},
					label: "local_gs#0");

				await CompareObjectPropertiesFor (frame_locals, "sc_arg",
						TSimpleClass (10, 45, "sc_arg#Id", "Blue"),
						label: "sc_arg#0");

				await CompareObjectPropertiesFor (frame_locals, "this",
						TPoint (90, -4, "point#Id", "Green"),
						label: "this#0");
			});
		}

		[Fact]
		public async Task EvaluateThisProperties ()
		{
			var insp = new Inspector ();
			//Collect events
			var scripts = SubscribeToScripts(insp);
			int line = 18;
			int col = 16;
			string entry_method_name = "[debugger-test] DebuggerTests.EvaluateTestsClass:EvaluateLocals";

			await Ready();
			await insp.Ready (async (cli, token) => {
				ctx = new DebugTestContext (cli, insp, token, scripts);
				var debugger_test_loc = "dotnet://debugger-test.dll/debugger-evaluate-test.cs";

				await SetBreakpoint (debugger_test_loc, line, col);

				var eval_expr = "window.setTimeout(function() { invoke_static_method_async ("
							+ $"'{entry_method_name}'"
						+ "); }, 1);";

				var pause_location = await EvaluateAndCheck (eval_expr, 
										debugger_test_loc, 
										line, 
										col, 
										"run",
										wait_for_event_fn: async (pause_location) => {
											var locals = await GetProperties (pause_location ["callFrames"][0] ["callFrameId"].Value<string> ());
											var evaluate = await EvaluateOnCallFrame (pause_location ["callFrames"][0] ["callFrameId"].Value<string> (), "a");
											CheckContentValue (evaluate, "1");
											evaluate = await EvaluateOnCallFrame (pause_location ["callFrames"][0] ["callFrameId"].Value<string> (), "b");
											CheckContentValue (evaluate, "2");
											evaluate = await EvaluateOnCallFrame (pause_location ["callFrames"][0] ["callFrameId"].Value<string> (), "c");
											CheckContentValue (evaluate, "3");
										});
			});
		}

		[Fact]
		public async Task EvaluateParameters ()
		{
			var insp = new Inspector ();
			//Collect events
			var scripts = SubscribeToScripts(insp);
			int line = 18;
			int col = 16;
			string entry_method_name = "[debugger-test] DebuggerTests.EvaluateTestsClass:EvaluateLocals";

			await Ready();
			await insp.Ready (async (cli, token) => {
				ctx = new DebugTestContext (cli, insp, token, scripts);
				var debugger_test_loc = "dotnet://debugger-test.dll/debugger-evaluate-test.cs";

				await SetBreakpoint (debugger_test_loc, line, col);

				var eval_expr = "window.setTimeout(function() { invoke_static_method_async ("
							+ $"'{entry_method_name}'"
						+ "); }, 1);";

				var pause_location = await EvaluateAndCheck (eval_expr, 
										debugger_test_loc, 
										line, 
										col, 
										"run",
										wait_for_event_fn: async (pause_location) => {
											var locals = await GetProperties (pause_location ["callFrames"][0] ["callFrameId"].Value<string> ());
											var evaluate = await EvaluateOnCallFrame (pause_location ["callFrames"][0] ["callFrameId"].Value<string> (), "g");
											CheckContentValue (evaluate, "100");
											evaluate = await EvaluateOnCallFrame (pause_location ["callFrames"][0] ["callFrameId"].Value<string> (), "h");
											CheckContentValue (evaluate, "200");
											evaluate = await EvaluateOnCallFrame (pause_location ["callFrames"][0] ["callFrameId"].Value<string> (), "valString");
											CheckContentValue (evaluate, "test");
										});
			});
		}

		[Fact]
		public async Task EvaluateLocals ()
		{
			var insp = new Inspector ();
			//Collect events
			var scripts = SubscribeToScripts(insp);
			int line = 18;
			int col = 16;
			string entry_method_name = "[debugger-test] DebuggerTests.EvaluateTestsClass:EvaluateLocals";

			await Ready();
			await insp.Ready (async (cli, token) => {
				ctx = new DebugTestContext (cli, insp, token, scripts);
				var debugger_test_loc = "dotnet://debugger-test.dll/debugger-evaluate-test.cs";

				await SetBreakpoint (debugger_test_loc, line, col);

				var eval_expr = "window.setTimeout(function() { invoke_static_method_async ("
							+ $"'{entry_method_name}'"
						+ "); }, 1);";

				var pause_location = await EvaluateAndCheck (eval_expr, 
										debugger_test_loc, 
										line, 
										col, 
										"run",
										wait_for_event_fn: async (pause_location) => {
											var locals = await GetProperties (pause_location ["callFrames"][0] ["callFrameId"].Value<string> ());
											var evaluate = await EvaluateOnCallFrame (pause_location ["callFrames"][0] ["callFrameId"].Value<string> (), "d");
											CheckContentValue (evaluate, "101");
											evaluate = await EvaluateOnCallFrame (pause_location ["callFrames"][0] ["callFrameId"].Value<string> (), "e");
											CheckContentValue (evaluate, "102");
											evaluate = await EvaluateOnCallFrame (pause_location ["callFrames"][0] ["callFrameId"].Value<string> (), "f");
											CheckContentValue (evaluate, "103");
										});
			});
		}


		[Fact]
		public async Task EvaluateExpressions ()
		{
			var insp = new Inspector ();
			//Collect events
			var scripts = SubscribeToScripts(insp);
			int line = 18;
			int col = 16;
			string entry_method_name = "[debugger-test] DebuggerTests.EvaluateTestsClass:EvaluateLocals";

			await Ready();
			await insp.Ready (async (cli, token) => {
				ctx = new DebugTestContext (cli, insp, token, scripts);
				var debugger_test_loc = "dotnet://debugger-test.dll/debugger-evaluate-test.cs";

				await SetBreakpoint (debugger_test_loc, line, col);

				var eval_expr = "window.setTimeout(function() { invoke_static_method_async ("
							+ $"'{entry_method_name}'"
						+ "); }, 1);";

				var pause_location = await EvaluateAndCheck (eval_expr, 
										debugger_test_loc, 
										line, 
										col, 
										"run",
										wait_for_event_fn: async (pause_location) => {
											var locals = await GetProperties (pause_location ["callFrames"][0] ["callFrameId"].Value<string> ());
											var evaluate = await EvaluateOnCallFrame (pause_location ["callFrames"][0] ["callFrameId"].Value<string> (), "d + e");
											CheckContentValue (evaluate, "203");
											evaluate = await EvaluateOnCallFrame (pause_location ["callFrames"][0] ["callFrameId"].Value<string> (), "e + 10");
											CheckContentValue (evaluate, "112");
											evaluate = await EvaluateOnCallFrame (pause_location ["callFrames"][0] ["callFrameId"].Value<string> (), "a + a");
											CheckContentValue (evaluate, "2");
											evaluate = await EvaluateOnCallFrame (pause_location ["callFrames"][0] ["callFrameId"].Value<string> (), "this.a + this.b");
											CheckContentValue (evaluate, "3");
											evaluate = await EvaluateOnCallFrame (pause_location ["callFrames"][0] ["callFrameId"].Value<string> (), "\"test\" + \"test\"");
											CheckContentValue (evaluate, "testtest");
											evaluate = await EvaluateOnCallFrame (pause_location ["callFrames"][0] ["callFrameId"].Value<string> (), "5 + 5");
											CheckContentValue (evaluate, "10");
										});
			});
		}

		[Fact]
		public async Task EvaluateThisExpressions ()
		{
			var insp = new Inspector ();
			//Collect events
			var scripts = SubscribeToScripts(insp);
			int line = 18;
			int col = 16;
			string entry_method_name = "[debugger-test] DebuggerTests.EvaluateTestsClass:EvaluateLocals";

			await Ready();
			await insp.Ready (async (cli, token) => {
				ctx = new DebugTestContext (cli, insp, token, scripts);
				var debugger_test_loc = "dotnet://debugger-test.dll/debugger-evaluate-test.cs";

				await SetBreakpoint (debugger_test_loc, line, col);

				var eval_expr = "window.setTimeout(function() { invoke_static_method_async ("
							+ $"'{entry_method_name}'"
						+ "); }, 1);";

				var pause_location = await EvaluateAndCheck (eval_expr, 
										debugger_test_loc, 
										line, 
										col, 
										"run",
										wait_for_event_fn: async (pause_location) => {
											var locals = await GetProperties (pause_location ["callFrames"][0] ["callFrameId"].Value<string> ());
											var evaluate = await EvaluateOnCallFrame (pause_location ["callFrames"][0] ["callFrameId"].Value<string> (), "this.a");
											CheckContentValue (evaluate, "1");
											evaluate = await EvaluateOnCallFrame (pause_location ["callFrames"][0] ["callFrameId"].Value<string> (), "this.b");
											CheckContentValue (evaluate, "2");
											evaluate = await EvaluateOnCallFrame (pause_location ["callFrames"][0] ["callFrameId"].Value<string> (), "this.c");
											CheckContentValue (evaluate, "3");
										});
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

		async Task CheckProps (JToken actual, object exp_o, string label, int num_fields=-1)
		{
			if (exp_o.GetType ().IsArray || exp_o is JArray) {
				if (! (actual is JArray actual_arr)) {
					Assert.True (false, $"[{label}] Expected to get an array here but got {actual}");
					return;
				}

				var exp_v_arr = JArray.FromObject (exp_o);
				Assert.Equal (exp_v_arr.Count, actual_arr.Count ());

				for (int i = 0; i < exp_v_arr.Count; i ++) {
					var exp_i = exp_v_arr [i];
					var act_i = actual_arr [i];

					Assert.True (act_i ["name"]?.Value<string> () == $"[{i}]", $"{label}-[{i}].name");

					await CheckValue (act_i["value"], exp_i, $"{label}-{i}th value");
				}

				return;
			}

			// Not an array
			var exp = exp_o as JObject;
			if (exp == null)
				exp = JObject.FromObject(exp_o);

			num_fields = num_fields < 0 ? exp.Values<JToken>().Count() : num_fields;
			Assert.True(num_fields == actual.Count(), $"[{label}] Number of fields don't match, Expected: {num_fields}, Actual: {actual.Count()}");

			foreach (var kvp in exp) {
				var exp_name = kvp.Key;
				var exp_val = kvp.Value;

				var actual_obj = actual.FirstOrDefault(jt => jt["name"]?.Value<string>() == exp_name);
				if (actual_obj == null) {
					Console.WriteLine($"actual: {actual}, exp_name: {exp_name}, exp_val: {exp_val}");
					Assert.True(actual_obj != null, $"[{label}] Could not find property named '{exp_name}'");
				}

				var actual_val = actual_obj["value"];
				Assert.True(actual_obj != null, $"[{label}] not value found for property named '{exp_name}'");

				if (exp_val.Type == JTokenType.Array) {
					var actual_props = await GetProperties(actual_val["objectId"]?.Value<string>());
					await CheckProps (actual_props, exp_val, $"{label}-{exp_name}");
				} else {
					await CheckValue (actual_val, exp_val, $"{label}#{exp_name}");
				}
			}
		}

		async Task CheckValue (JToken actual_val, JToken exp_val, string label)
		{
			if (exp_val ["type"] == null && actual_val ["objectId"] != null) {
				var new_val = await GetProperties (actual_val ["objectId"].Value<string> ());
				await CheckProps (new_val, exp_val, $"{label}-{actual_val["objectId"]?.Value<string>()}");
				return;
			}

			foreach (var jp in exp_val.Values<JProperty> ()) {
				var exp_val_str = jp.Value.Value<string> ();
				bool null_or_empty_exp_val = String.IsNullOrEmpty (exp_val_str);

				var actual_field_val = actual_val.Values<JProperty> ().FirstOrDefault (a_jp => a_jp.Name == jp.Name);
				var actual_field_val_str = actual_field_val?.Value?.Value<string> ();
				if (null_or_empty_exp_val && String.IsNullOrEmpty (actual_field_val_str))
					continue;

				Assert.True (actual_field_val != null, $"[{label}] Could not find value field named {jp.Name}");

				Assert.True (exp_val_str == actual_field_val_str,
						$"[{label}] Value for json property named {jp.Name} didn't match.\n" +
						$"Expected: {jp.Value.Value<string> ()}\n" +
						$"Actual:   {actual_field_val.Value.Value<string> ()}");
			}
		}

		async Task<JToken> CheckLocalsOnFrame (JToken frame, string script_loc, int line, int column, string function_name, Action<JToken> test_fn = null)
		{
			CheckLocation (script_loc, line, column, ctx.scripts, frame ["location"]);
			Assert.Equal (function_name, frame ["functionName"].Value<string> ());

			return await CheckLocalsOnFrame (frame, test_fn);
		}

		async Task<JToken> CheckLocalsOnFrame (JToken frame, object expected, string label)
		{
			var locals = await GetProperties (frame ["callFrameId"].Value<string> ());
			try {
				await CheckProps (locals, expected, label);
				return locals;
			} catch {
				Console.WriteLine ($"CheckLocalsOnFrame failed for locals: {locals}");
				throw;
			}
		}

		async Task<JToken> CheckLocalsOnFrame (JToken frame, Action<JToken> test_fn)
		{
			var locals = await GetProperties (frame ["callFrameId"].Value<string> ());
			try {
				test_fn (locals);
				return locals;
			} catch {
				Console.WriteLine ($"CheckLocalsOnFrame failed for locals: {locals}");
				throw;
			}
		}

		// Find an object with @name in the *frame*, *fetch* the object, and check against @o
		async Task<JToken> CompareObjectPropertiesOnFrameLocals (JToken locals, string name, object o, string label = null) {
			var obj_props = await CheckObjectOnFrameLocals (locals, name, (jt) => {});
			try {
				if (o != null)
					await CheckProps (obj_props, o, label);
			} catch {
				Console.WriteLine ($"CheckObjectOnFrameLocals failed for locals: {obj_props}");
				throw;
			}
			return obj_props;
		}

		async Task<JToken> CheckObjectOnFrameLocals (JToken frame, string name, Action<JToken> test_fn)
		{
			var locals = await GetProperties (frame ["callFrameId"].Value<string> ());
			return await CheckObjectOnLocals (locals, name, test_fn);
		}

		// Find an object with @name, *fetch* the object, and check against @o
		async Task<JToken> CompareObjectPropertiesFor (JToken locals, string name, object o, string label = null)
		{
			if (label == null)
				label = name;
			var props = await CheckObjectOnLocals (locals, name, (jt) => {});
			try {
				if (o != null)
					await CheckProps (props, o, label);
				return props;
			} catch {
				Console.WriteLine ($"CheckObjectOnFrameLocals failed for locals: {props}");
				throw;
			}
		}

		async Task<JToken> CheckObjectOnLocals (JToken locals, string name, Action<JToken> test_fn)
		{
			var obj = locals.Where (jt => jt ["name"]?.Value<string> () == name)
					.FirstOrDefault ();
			if (obj == null) {
				Console.WriteLine ($"CheckObjectOnLocals failed with locals: {locals}");
				Assert.True (false, $"Could not find a var with name {name} and type object");
			}

			var objectId = obj ["value"]["objectId"]?.Value<string> ();
			Assert.True (!String.IsNullOrEmpty (objectId), $"No objectId found for {name}");

			var props = await GetProperties (objectId);
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

		async Task<JToken> EvaluateOnCallFrame (string id, string expression)
		{
			var evaluate_req = JObject.FromObject (new {
				callFrameId = id,
				expression = expression
			});

			var frame_evaluate = await ctx.cli.SendCommand ("Debugger.evaluateOnCallFrame", evaluate_req, ctx.token);
			if (!frame_evaluate.IsOk)
				Assert.True (false, $"Debugger.evaluateOnCallFrame failed for {evaluate_req.ToString ()}, with Result: {frame_evaluate}");

			var evaluate_result = frame_evaluate.Value ["result"];
			return evaluate_result;
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

		//FIXME: um maybe we don't need to convert jobject right here!
		static JObject TString (string value) =>
			value == null
				? TObject ("string", is_null: true)
				: JObject.FromObject (new { type = "string", value = @value, description = @value });

		static JObject TNumber (int value) =>
			JObject.FromObject (new { type = "number", value = @value.ToString (), description = value.ToString () });

		static JObject TValueType (string className, object members = null) =>
			JObject.FromObject (new { type = "object", isValueType = true, className = className, description = className });

		static JObject TEnum (string className, string descr, object members = null) =>
			JObject.FromObject (new { type = "object", isEnum = true, className = className, description = descr });

		static JObject TObject (string className, bool is_null = false) =>
			is_null
				? JObject.FromObject (new { type = "object", className = className, description = className, subtype = is_null ? "null" : null })
				: JObject.FromObject (new { type = "object", className = className, description = className });

		static JObject TArray (string className)
			=> JObject.FromObject (new { type = "object", className = className, description = className, subtype = "array" });

		static JObject TBool (bool value)
			=> JObject.FromObject (new { type = "boolean", value = @value, description = @value ? "true" : "false" });

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
