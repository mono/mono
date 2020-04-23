using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Xunit;
using WebAssembly.Net.Debugging;

namespace DebuggerTests
{

	public class SourceList : DebuggerTestBase {

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

		[Theory]
		[InlineData (false)]
		[InlineData (true)]
		public async Task InspectLocalsAtBreakpointSite (bool use_cfo) =>
			await CheckInspectLocalsAtBreakpointSite (
				"dotnet://debugger-test.dll/debugger-test.cs", 5, 2, "IntAdd",
				"window.setTimeout(function() { invoke_add(); }, 1);",
				use_cfo: use_cfo,
				test_fn: (locals) => {
					CheckNumber (locals, "a", 10);
					CheckNumber (locals, "b", 20);
					CheckNumber (locals, "c", 30);
					CheckNumber (locals, "d", 0);
					CheckNumber (locals, "e", 0);
				}
			);

		[Fact]
		public async Task InspectPrimitiveTypeLocalsAtBreakpointSite () =>
			await CheckInspectLocalsAtBreakpointSite (
				"dotnet://debugger-test.dll/debugger-test.cs", 145, 2, "PrimitiveTypesTest",
				"window.setTimeout(function() { invoke_static_method ('[debugger-test] Math:PrimitiveTypesTest'); }, 1);",
				test_fn: (locals) => {
					CheckSymbol (locals, "c0", "8364 '€'");
					CheckSymbol (locals, "c1", "65 'A'");
				}
			);

		[Theory]
		[InlineData (0, 45, 2, "DelegatesTest", false)]
		[InlineData (0, 45, 2, "DelegatesTest", true)]
		[InlineData (2, 90, 2, "InnerMethod2", false)]
		[InlineData (2, 90, 2, "InnerMethod2", true)]
		public async Task InspectLocalsWithDelegatesAtBreakpointSite (int frame, int line, int col, string method_name, bool use_cfo) =>
			await CheckInspectLocalsAtBreakpointSite (
				"dotnet://debugger-test.dll/debugger-test.cs", line, col, method_name,
				"window.setTimeout(function() { invoke_delegates_test (); }, 1);",
				use_cfo: use_cfo,
				wait_for_event_fn: async (pause_location) => {
					var locals = await GetProperties(pause_location["callFrames"][frame]["callFrameId"].Value<string>());

					await CheckProps (locals, new {
						fn_func			= TDelegate ("System.Func<Math, bool>", "bool <DelegatesTest>|(Math)"),
						fn_func_null		= TObject   ("System.Func<Math, bool>", is_null: true),
						fn_func_arr		= TArray    ("System.Func<Math, bool>[]", 1),
						fn_del			= TDelegate ("Math.IsMathNull", "bool IsMathNullDelegateTarget (Math)"),
						fn_del_null		= TObject   ("Math.IsMathNull", is_null: true),
						fn_del_arr		= TArray    ("Math.IsMathNull[]", 1),

						// Unused locals
						fn_func_unused		= TDelegate ("System.Func<Math, bool>", "bool <DelegatesTest>|(Math)"),
						fn_func_null_unused	= TObject   ("System.Func<Math, bool>", is_null: true),
						fn_func_arr_unused	= TArray    ("System.Func<Math, bool>[]", 1),

						fn_del_unused		= TDelegate ("Math.IsMathNull", "bool IsMathNullDelegateTarget (Math)"),
						fn_del_null_unused	= TObject   ("Math.IsMathNull", is_null: true),
						fn_del_arr_unused	= TArray    ("Math.IsMathNull[]", 1),

						res			= TBool     (false),
						m_obj			= TObject   ("Math")
					}, "locals");

					await CompareObjectPropertiesFor (locals, "fn_func_arr", new [] {
						TDelegate (
							"System.Func<Math, bool>",
							"bool <DelegatesTest>|(Math)")
					}, "locals#fn_func_arr");

					await CompareObjectPropertiesFor (locals, "fn_del_arr", new [] {
						TDelegate (
							"Math.IsMathNull",
							"bool IsMathNullDelegateTarget (Math)")
					}, "locals#fn_del_arr");

					await CompareObjectPropertiesFor (locals, "fn_func_arr_unused", new [] {
						TDelegate (
							"System.Func<Math, bool>",
							"bool <DelegatesTest>|(Math)")
					}, "locals#fn_func_arr_unused");

					await CompareObjectPropertiesFor (locals, "fn_del_arr_unused", new [] {
						TDelegate (
							"Math.IsMathNull",
							"bool IsMathNullDelegateTarget (Math)")
					}, "locals#fn_del_arr_unused");
				}
			);

		[Theory]
		[InlineData (false)]
		[InlineData (true)]
		public async Task InspectLocalsWithGenericTypesAtBreakpointSite (bool use_cfo) =>
			await CheckInspectLocalsAtBreakpointSite (
				"dotnet://debugger-test.dll/debugger-test.cs", 65, 2, "GenericTypesTest",
				"window.setTimeout(function() { invoke_generic_types_test (); }, 1);",
				use_cfo: use_cfo,
				test_fn: (locals) => {
					CheckObject (locals, "list", "System.Collections.Generic.Dictionary<Math[], Math.IsMathNull>");
					CheckObject (locals, "list_null", "System.Collections.Generic.Dictionary<Math[], Math.IsMathNull>", is_null: true);

					CheckArray (locals, "list_arr", "System.Collections.Generic.Dictionary<Math[], Math.IsMathNull>[]");
					CheckObject (locals, "list_arr_null", "System.Collections.Generic.Dictionary<Math[], Math.IsMathNull>[]", is_null: true);

					// Unused locals
					CheckObject (locals, "list_unused", "System.Collections.Generic.Dictionary<Math[], Math.IsMathNull>");
					CheckObject (locals, "list_null_unused", "System.Collections.Generic.Dictionary<Math[], Math.IsMathNull>", is_null: true);

					CheckObject (locals, "list_arr_unused", "System.Collections.Generic.Dictionary<Math[], Math.IsMathNull>[]");
					CheckObject (locals, "list_arr_null_unused", "System.Collections.Generic.Dictionary<Math[], Math.IsMathNull>[]", is_null: true);
				}
			);

		[Theory]
		[InlineData (0, 190, 2, "DelegatesSignatureTest", false)]
		[InlineData (0, 190, 2, "DelegatesSignatureTest", true)]
		[InlineData (2, 90, 2, "InnerMethod2", false)]
		[InlineData (2, 90, 2, "InnerMethod2", true)]
		public async Task InspectDelegateSignaturesWithFunc (int frame, int line, int col, string bp_method, bool use_cfo)
			=> await CheckInspectLocalsAtBreakpointSite (
				"dotnet://debugger-test.dll/debugger-test.cs",
				line, col,
				bp_method,
				"window.setTimeout (function () { invoke_static_method ('[debugger-test] Math:DelegatesSignatureTest'); }, 1)",
				use_cfo: use_cfo,
				wait_for_event_fn: async (pause_location) => {
					var locals = await GetProperties (pause_location ["callFrames"][frame]["callFrameId"].Value<string>());

					await CheckProps (locals, new {
						fn_func		= TDelegate  ("System.Func<Math, Math.GenericStruct<Math.GenericStruct<int[]>>, Math.GenericStruct<bool[]>>",
									      "Math.GenericStruct<bool[]> <DelegatesSignatureTest>|(Math,Math.GenericStruct<Math.GenericStruct<int[]>>)"),

						fn_func_del	= TDelegate  ("System.Func<Math, Math.GenericStruct<Math.GenericStruct<int[]>>, Math.GenericStruct<bool[]>>",
									      "Math.GenericStruct<bool[]> DelegateTargetForSignatureTest (Math,Math.GenericStruct<Math.GenericStruct<int[]>>)"),

						fn_func_null	= TObject    ("System.Func<Math, Math.GenericStruct<Math.GenericStruct<int[]>>, Math.GenericStruct<bool[]>>", is_null: true),
						fn_func_only_ret= TDelegate  ("System.Func<bool>", "bool <DelegatesSignatureTest>|()"),
						fn_func_arr	= TArray     ("System.Func<Math, Math.GenericStruct<Math.GenericStruct<int[]>>, Math.GenericStruct<bool[]>>[]", 1),

						fn_del		= TDelegate  ("Math.DelegateForSignatureTest",
									      "Math.GenericStruct<bool[]> DelegateTargetForSignatureTest (Math,Math.GenericStruct<Math.GenericStruct<int[]>>)"),

						fn_del_l	=  TDelegate ("Math.DelegateForSignatureTest",
									      "Math.GenericStruct<bool[]> <DelegatesSignatureTest>|(Math,Math.GenericStruct<Math.GenericStruct<int[]>>)"),

						fn_del_null	= TObject    ("Math.DelegateForSignatureTest", is_null: true),
						fn_del_arr	= TArray     ("Math.DelegateForSignatureTest[]", 2),
						m_obj		= TObject    ("Math"),
						gs_gs		= TValueType ("Math.GenericStruct<Math.GenericStruct<int[]>>"),
						fn_void_del	= TDelegate  ("Math.DelegateWithVoidReturn",
									      "void DelegateTargetWithVoidReturn (Math.GenericStruct<int[]>)"),

						fn_void_del_arr	= TArray     ("Math.DelegateWithVoidReturn[]", 1),
						fn_void_del_null= TObject    ("Math.DelegateWithVoidReturn", is_null: true),
						gs		= TValueType ("Math.GenericStruct<int[]>"),
						rets		= TArray     ("Math.GenericStruct<bool[]>[]", 6)
					}, "locals");

					await CompareObjectPropertiesFor (locals, "fn_func_arr", new [] {
						TDelegate (
							"System.Func<Math, Math.GenericStruct<Math.GenericStruct<int[]>>, Math.GenericStruct<bool[]>>",
							"Math.GenericStruct<bool[]> <DelegatesSignatureTest>|(Math,Math.GenericStruct<Math.GenericStruct<int[]>>)"),
					}, "locals#fn_func_arr");

					await CompareObjectPropertiesFor (locals, "fn_del_arr", new [] {
						TDelegate (
							"Math.DelegateForSignatureTest",
							"Math.GenericStruct<bool[]> DelegateTargetForSignatureTest (Math,Math.GenericStruct<Math.GenericStruct<int[]>>)"),
						TDelegate (
							"Math.DelegateForSignatureTest",
							"Math.GenericStruct<bool[]> <DelegatesSignatureTest>|(Math,Math.GenericStruct<Math.GenericStruct<int[]>>)")
					}, "locals#fn_del_arr");

					await CompareObjectPropertiesFor (locals, "fn_void_del_arr", new [] {
						TDelegate (
							"Math.DelegateWithVoidReturn",
							"void DelegateTargetWithVoidReturn (Math.GenericStruct<int[]>)")
					}, "locals#fn_void_del_arr");
				});

		[Theory]
		[InlineData (0, 211, 2, "ActionTSignatureTest", false)]
		[InlineData (0, 211, 2, "ActionTSignatureTest", true)]
		[InlineData (2, 90, 2, "InnerMethod2", false)]
		[InlineData (2, 90, 2, "InnerMethod2", true)]
		public async Task ActionTSignatureTest (int frame, int line, int col, string bp_method, bool use_cfo)
			=> await CheckInspectLocalsAtBreakpointSite (
				"dotnet://debugger-test.dll/debugger-test.cs", line, col,
				bp_method,
				"window.setTimeout (function () { invoke_static_method ('[debugger-test] Math:ActionTSignatureTest'); }, 1)",
				use_cfo: use_cfo,
				wait_for_event_fn: async (pause_location) => {
					var locals = await GetProperties (pause_location ["callFrames"][frame]["callFrameId"].Value<string>());

					await CheckProps (locals, new
					{
						fn_action	= TDelegate  ("System.Action<Math.GenericStruct<int[]>>",
									      "void <ActionTSignatureTest>|(Math.GenericStruct<int[]>)"),
						fn_action_del	= TDelegate  ("System.Action<Math.GenericStruct<int[]>>",
									      "void DelegateTargetWithVoidReturn (Math.GenericStruct<int[]>)"),
						fn_action_bare  = TDelegate  ("System.Action",
									      "void|()"),

						fn_action_null	= TObject    ("System.Action<Math.GenericStruct<int[]>>", is_null: true),

						fn_action_arr	= TArray     ("System.Action<Math.GenericStruct<int[]>>[]", 3),

						gs		= TValueType ("Math.GenericStruct<int[]>"),
					}, "locals");

					await CompareObjectPropertiesFor (locals, "fn_action_arr", new [] {
						TDelegate (
							"System.Action<Math.GenericStruct<int[]>>",
							"void <ActionTSignatureTest>|(Math.GenericStruct<int[]>)"),
						TDelegate (
							"System.Action<Math.GenericStruct<int[]>>",
							"void DelegateTargetWithVoidReturn (Math.GenericStruct<int[]>)"),
						TObject ("System.Action<Math.GenericStruct<int[]>>", is_null: true)
					}, "locals#fn_action_arr");
				});

		[Theory]
		[InlineData (0, 228, 2, "NestedDelegatesTest", false)]
		[InlineData (0, 228, 2, "NestedDelegatesTest", true)]
		[InlineData (2, 90, 2, "InnerMethod2", false)]
		[InlineData (2, 90, 2, "InnerMethod2", true)]
		public async Task NestedDelegatesTest (int frame, int line, int col, string bp_method, bool use_cfo)
			=> await CheckInspectLocalsAtBreakpointSite (
				"dotnet://debugger-test.dll/debugger-test.cs", line, col,
				bp_method,
				"window.setTimeout (function () { invoke_static_method ('[debugger-test] Math:NestedDelegatesTest'); }, 1)",
				use_cfo: use_cfo,
				wait_for_event_fn: async (pause_location) => {
					var locals = await GetProperties (pause_location ["callFrames"][frame]["callFrameId"].Value<string>());

					await CheckProps (locals, new {
						fn_func		= TDelegate ("System.Func<System.Func<int, bool>, bool>",
									     "bool <NestedDelegatesTest>|(Func<int, bool>)"),
						fn_func_null 	= TObject   ("System.Func<System.Func<int, bool>, bool>", is_null: true),
						fn_func_arr	= TArray    ("System.Func<System.Func<int, bool>, bool>[]", 1),
						fn_del_arr	= TArray    ("System.Func<System.Func<int, bool>, bool>[]", 1),

						m_obj		= TObject   ("Math"),
						fn_del_null	= TObject   ("System.Func<System.Func<int, bool>, bool>", is_null: true),
						fs		= TDelegate ("System.Func<int, bool>",
									     "bool <NestedDelegatesTest>|(int)")
					}, "locals");

					await CompareObjectPropertiesFor (locals, "fn_func_arr", new [] {
						TDelegate (
							"System.Func<System.Func<int, bool>, bool>",
							"bool <NestedDelegatesTest>|(System.Func<int, bool>)")
					}, "locals#fn_func_arr");

					await CompareObjectPropertiesFor (locals, "fn_del_arr", new [] {
						TDelegate (
							"System.Func<System.Func<int, bool>, bool>",
							"bool DelegateTargetForNestedFunc (Func<int, bool>)")
					}, "locals#fn_del_arr");
				});

		[Theory]
		[InlineData (0, 247, 2, "MethodWithDelegateArgs", false)]
		[InlineData (0, 247, 2, "MethodWithDelegateArgs", true)]
		[InlineData (2, 90, 2, "InnerMethod2", false)]
		[InlineData (2, 90, 2, "InnerMethod2", true)]
		public async Task DelegatesAsMethodArgsTest (int frame, int line, int col, string bp_method, bool use_cfo)
			=> await CheckInspectLocalsAtBreakpointSite (
				"dotnet://debugger-test.dll/debugger-test.cs", line, col,
				bp_method,
				"window.setTimeout (function () { invoke_static_method ('[debugger-test] Math:DelegatesAsMethodArgsTest'); }, 1)",
				use_cfo: use_cfo,
				wait_for_event_fn: async (pause_location) => {
					var locals = await GetProperties (pause_location ["callFrames"][frame]["callFrameId"].Value<string>());

					await CheckProps (locals, new {
						@this		= TObject   ("Math"),
						dst_arr		= TArray    ("Math.DelegateForSignatureTest[]", 2),
						fn_func		= TDelegate ("System.Func<char[], bool>",
									     "bool <DelegatesAsMethodArgsTest>|(char[])"),
						fn_action	= TDelegate ("System.Action<Math.GenericStruct<int>[]>",
									     "void <DelegatesAsMethodArgsTest>|(Math.GenericStruct<int>[])")
					}, "locals");

					await CompareObjectPropertiesFor (locals, "dst_arr", new [] {
						TDelegate ("Math.DelegateForSignatureTest",
							"Math.GenericStruct<bool[]> DelegateTargetForSignatureTest (Math,Math.GenericStruct<Math.GenericStruct<int[]>>)"),
						TDelegate ("Math.DelegateForSignatureTest",
							"Math.GenericStruct<bool[]> <DelegatesAsMethodArgsTest>|(Math,Math.GenericStruct<Math.GenericStruct<int[]>>)"),
					}, "locals#dst_arr");
				});

		[Theory]
		[InlineData (false)]
		[InlineData (true)]
		public async Task MethodWithDelegatesAsyncTest (bool use_cfo)
			=> await CheckInspectLocalsAtBreakpointSite (
				"dotnet://debugger-test.dll/debugger-test.cs", 265, 2,
				"MoveNext", //"DelegatesAsMethodArgsTestAsync"
				"window.setTimeout (function () { invoke_static_method_async ('[debugger-test] Math:MethodWithDelegatesAsyncTest'); }, 1)",
				use_cfo: use_cfo,
				wait_for_event_fn: async (pause_location) => {
					var locals = await GetProperties (pause_location ["callFrames"][0]["callFrameId"].Value<string>());

					await CheckProps (locals, new {
						@this		= TObject   ("Math"),
						_dst_arr	= TArray    ("Math.DelegateForSignatureTest[]", 2),
						_fn_func 	= TDelegate ("System.Func<char[], bool>",
									     "bool <MethodWithDelegatesAsync>|(char[])"),
						_fn_action	= TDelegate ("System.Action<Math.GenericStruct<int>[]>",
									     "void <MethodWithDelegatesAsync>|(Math.GenericStruct<int>[])")
					}, "locals");

					await CompareObjectPropertiesFor (locals, "_dst_arr", new [] {
						TDelegate (
							"Math.DelegateForSignatureTest",
							"Math.GenericStruct<bool[]> DelegateTargetForSignatureTest (Math,Math.GenericStruct<Math.GenericStruct<int[]>>)"),
						TDelegate (
							"Math.DelegateForSignatureTest",
							"Math.GenericStruct<bool[]> <MethodWithDelegatesAsync>|(Math,Math.GenericStruct<Math.GenericStruct<int[]>>)"),
					}, "locals#dst_arr");
				});

		object TGenericStruct(string typearg, string stringField)
			=> new {
				List = TObject ($"System.Collections.Generic.List<{typearg}>"),
				StringField = TString (stringField)
			};

		internal async Task CheckInspectLocalsAtBreakpointSite (string url_key, int line, int column, string function_name, string eval_expression,
						Action<JToken> test_fn = null, Func<JObject, Task> wait_for_event_fn = null, bool use_cfo = false)
		{
			var insp = new Inspector ();
			//Collect events
			var scripts = SubscribeToScripts(insp);

			await Ready ();
			await insp.Ready (async (cli, token) => {
				ctx = new DebugTestContext (cli, insp, token, scripts);
				ctx.UseCallFunctionOnBeforeGetProperties = use_cfo;

				var bp = await SetBreakpoint (url_key, line, column);

				await EvaluateAndCheck (
					eval_expression, url_key, line, column,
					function_name,
					wait_for_event_fn: async (pause_location) => {
						//make sure we're on the right bp

						Assert.Equal (bp.Value ["breakpointId"]?.ToString (), pause_location ["hitBreakpoints"]?[0]?.Value<string> ());

						var top_frame = pause_location ["callFrames"][0];

						var scope = top_frame ["scopeChain"][0];
						Assert.Equal ("dotnet:scope:0", scope ["object"]["objectId"]);
						if (wait_for_event_fn != null)
							await wait_for_event_fn(pause_location);
						else
							await Task.CompletedTask;
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

		[Theory]
		[InlineData (false)]
		[InlineData (true)]
		public async Task InspectLocalsInPreviousFramesDuringSteppingIn2 (bool use_cfo) {
			var insp = new Inspector ();
			//Collect events
			var scripts = SubscribeToScripts(insp);

			await Ready();
			await insp.Ready (async (cli, token) => {
				ctx = new DebugTestContext (cli, insp, token, scripts);
				ctx.UseCallFunctionOnBeforeGetProperties = use_cfo;

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

				var props = await GetObjectOnFrame (pause_location["callFrames"][0], "this");
				Assert.Equal (3, props.Count());
				CheckNumber (props, "A", 10);
				CheckString (props, "B", "xx");
				CheckObject (props, "c", "object");

				// Check UseComplex frame
				var locals_m1 = await GetLocalsForFrame (pause_location ["callFrames"][3], debugger_test_loc, 17, 2, "UseComplex");
				Assert.Equal (7, locals_m1.Count());

				CheckNumber (locals_m1, "a", 10);
				CheckNumber (locals_m1, "b", 20);
				CheckObject (locals_m1, "complex", "Simple.Complex");
				CheckNumber (locals_m1, "c", 30);
				CheckNumber (locals_m1, "d", 50);
				CheckNumber (locals_m1, "e", 60);
				CheckNumber (locals_m1, "f", 0);

				props = await GetObjectOnFrame (pause_location["callFrames"][3], "complex");
				Assert.Equal (3, props.Count());
				CheckNumber (props, "A", 10);
				CheckString (props, "B", "xx");
				CheckObject (props, "c", "object");

				pause_location = await StepAndCheck (StepKind.Over, dep_cs_loc, 16, 2, "DoStuff", times: 2);
				// Check UseComplex frame again
				locals_m1 = await GetLocalsForFrame (pause_location ["callFrames"][1], debugger_test_loc, 17, 2, "UseComplex");
				Assert.Equal (7, locals_m1.Count());

				CheckNumber (locals_m1, "a", 10);
				CheckNumber (locals_m1, "b", 20);
				CheckObject (locals_m1, "complex", "Simple.Complex");
				CheckNumber (locals_m1, "c", 30);
				CheckNumber (locals_m1, "d", 50);
				CheckNumber (locals_m1, "e", 60);
				CheckNumber (locals_m1, "f", 0);

				props = await GetObjectOnFrame (pause_location["callFrames"][1], "complex");
				Assert.Equal (3, props.Count());
				CheckNumber (props, "A", 10);
				CheckString (props, "B", "xx");
				CheckObject (props, "c", "object");
			});
		}

		[Theory]
		[InlineData (false)]
		[InlineData (true)]
		public async Task InspectLocalsInPreviousFramesDuringSteppingIn (bool use_cfo) {
			var insp = new Inspector ();
			//Collect events
			var scripts = SubscribeToScripts(insp);

			await Ready();
			await insp.Ready (async (cli, token) => {
				ctx = new DebugTestContext (cli, insp, token, scripts);
				ctx.UseCallFunctionOnBeforeGetProperties = use_cfo;

				var debugger_test_loc = "dotnet://debugger-test.dll/debugger-test.cs";
				await SetBreakpoint (debugger_test_loc, 102, 3);

				// Will stop in InnerMethod
				var wait_res = await EvaluateAndCheck (
					"window.setTimeout(function() { invoke_outer_method(); }, 1);",
					debugger_test_loc, 102, 3, "InnerMethod",
					locals_fn: (locals) => {
						Assert.Equal (4, locals.Count());
						CheckNumber (locals, "i", 5);
						CheckNumber (locals, "j", 24);
						CheckString (locals, "foo_str", "foo");
						CheckObject (locals, "this", "Math.NestedInMath");
					}
				);

				var this_props = await GetObjectOnFrame (wait_res["callFrames"][0], "this");
				Assert.Equal (2, this_props.Count());
				CheckObject (this_props, "m", "Math");
				CheckValueType (this_props, "SimpleStructProperty", "Math.SimpleStruct");

				var ss_props = await GetObjectOnLocals (this_props, "SimpleStructProperty");
				Assert.Equal (2, ss_props.Count());
				CheckValueType (ss_props, "dt", "System.DateTime");
				CheckValueType (ss_props, "gs", "Math.GenericStruct<System.DateTime>");

				await CheckDateTime (ss_props, "dt", new DateTime (2020, 1, 2, 3, 4, 5));

				// Check OuterMethod frame
				var locals_m1 = await GetLocalsForFrame (wait_res ["callFrames"][1], debugger_test_loc, 78, 2, "OuterMethod");
				Assert.Equal (5, locals_m1.Count());
				// FIXME: Failing test CheckNumber (locals_m1, "i", 5);
				// FIXME: Failing test CheckString (locals_m1, "text", "Hello");
				CheckNumber (locals_m1, "new_i", 0);
				CheckNumber (locals_m1, "k", 0);
				CheckObject (locals_m1, "nim", "Math.NestedInMath");

				// step back into OuterMethod
				await StepAndCheck (StepKind.Over, debugger_test_loc, 82, 2, "OuterMethod", times: 9,
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
				await StepAndCheck (StepKind.Into, "dotnet://debugger-test.dll/debugger-test.cs", 87, 1, "InnerMethod2",
					locals_fn: (locals) => {
						Assert.Equal (3, locals.Count());

						CheckString (locals, "s", "test string");
						//out var: CheckNumber (locals, "k", 0);
						CheckNumber (locals, "i", 24);
					}
				);

				await StepAndCheck (StepKind.Over, "dotnet://debugger-test.dll/debugger-test.cs", 91, 1, "InnerMethod2", times: 4,
					locals_fn: (locals) => {
						Assert.Equal (3, locals.Count());

						CheckString (locals, "s", "test string");
						// FIXME: Failing test CheckNumber (locals, "k", 34);
						CheckNumber (locals, "i", 24);
					}
				);

				await StepAndCheck (StepKind.Over, "dotnet://debugger-test.dll/debugger-test.cs", 83, 2, "OuterMethod", times: 2,
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

				await SetBreakpoint ("dotnet://debugger-test.dll/debugger-test.cs", 77, 2);

				await EvaluateAndCheck ("window.setTimeout(function() { invoke_outer_method(); }, 1);",
					"dotnet://debugger-test.dll/debugger-test.cs", 77, 2, "OuterMethod",
					locals_fn: (locals) => {
						Assert.Equal (5, locals.Count());

						CheckObject (locals, "nim", "Math.NestedInMath");
						CheckNumber (locals, "i", 5);
						CheckNumber (locals, "k", 0);
						CheckNumber (locals, "new_i", 0);
						CheckString (locals, "text", null);
					}
				);

				await StepAndCheck (StepKind.Over, "dotnet://debugger-test.dll/debugger-test.cs", 78, 2, "OuterMethod",
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
				await StepAndCheck (StepKind.Into, "dotnet://debugger-test.dll/debugger-test.cs", 96, 2, "InnerMethod");
				await StepAndCheck (StepKind.Over, "dotnet://debugger-test.dll/debugger-test.cs", 100, 3, "InnerMethod", times: 5,
					locals_fn: (locals) => {
						Assert.Equal (4, locals.Count());

						CheckNumber (locals, "i", 5);
						CheckNumber (locals, "j", 15);
						CheckString (locals, "foo_str", "foo");
						CheckObject (locals, "this", "Math.NestedInMath");
					}
				);

				// Step back to OuterMethod
				await StepAndCheck (StepKind.Over, "dotnet://debugger-test.dll/debugger-test.cs", 79, 2, "OuterMethod", times: 6,
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

		[Theory]
		[InlineData (false)]
		[InlineData (true)]
		public async Task InspectLocalsInAsyncMethods (bool use_cfo) {
			var insp = new Inspector ();
			//Collect events
			var scripts = SubscribeToScripts(insp);

			await Ready();
			await insp.Ready (async (cli, token) => {
				ctx = new DebugTestContext (cli, insp, token, scripts);
				ctx.UseCallFunctionOnBeforeGetProperties = use_cfo;
				var debugger_test_loc = "dotnet://debugger-test.dll/debugger-test.cs";

				await SetBreakpoint (debugger_test_loc, 111, 3);
				await SetBreakpoint (debugger_test_loc, 126, 3);

				// Will stop in Asyncmethod0
				var wait_res = await EvaluateAndCheck (
					"window.setTimeout(function() { invoke_async_method_with_await(); }, 1);",
					debugger_test_loc, 111, 3, "MoveNext", //FIXME:
					locals_fn: (locals) => {
						Assert.Equal (4, locals.Count());
						CheckString (locals, "s", "string from js");
						CheckNumber (locals, "i", 42);
						CheckString (locals, "local0", "value0");
						CheckObject (locals, "this", "Math.NestedInMath");
					}
				);
				Console.WriteLine (wait_res);
				
#if false // Disabled for now, as we don't have proper async traces
				var locals = await GetProperties (wait_res ["callFrames"][2]["callFrameId"].Value<string> ());
				Assert.Equal (4, locals.Count());
				CheckString (locals, "ls", "string from jstest");
				CheckNumber (locals, "li", 52);
#endif

				// TODO: previous frames have async machinery details, so no point checking that right now

				var pause_loc = await SendCommandAndCheck (null, "Debugger.resume", debugger_test_loc, 126, 3, /*FIXME: "AsyncMethodNoReturn"*/ "MoveNext",
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

				var this_props = await GetObjectOnFrame (pause_loc ["callFrames"][0], "this");
				Assert.Equal (2, this_props.Count ());
				CheckObject (this_props, "m", "Math");
				CheckValueType (this_props, "SimpleStructProperty", "Math.SimpleStruct");

				// TODO: Check `this` properties
			});
		}

		[Theory]
		[InlineData (false)]
		[InlineData (true)]
		public async Task InspectLocalsWithStructs (bool use_cfo) {
			var insp = new Inspector ();
			//Collect events
			var scripts = SubscribeToScripts(insp);

			await Ready();
			await insp.Ready (async (cli, token) => {
				ctx = new DebugTestContext (cli, insp, token, scripts);
				ctx.UseCallFunctionOnBeforeGetProperties = use_cfo;
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

				var dt = new DateTime (2021, 2, 3, 4, 6, 7);
				// Check ss_local's properties
				var ss_local_props = await GetObjectOnFrame (pause_location ["callFrames"][0], "ss_local");
				await CheckProps (ss_local_props, new {
							str_member = TString ("set in MethodWithLocalStructs#SimpleStruct#str_member"),
							dt = TValueType ("System.DateTime", dt.ToString ()),
							gs = TValueType ("DebuggerTests.ValueTypesTest.GenericStruct<System.DateTime>"),
							Kind = TEnum ("System.DateTimeKind", "Utc")
						}, "ss_local");

				{
					// Check ss_local.dt
					await CheckDateTime (ss_local_props, "dt", dt);

					// Check ss_local.gs
					var gs_props = await GetObjectOnLocals (ss_local_props, "gs");
					CheckString (gs_props, "StringField", "set in MethodWithLocalStructs#SimpleStruct#gs#StringField");
					CheckObject (gs_props, "List", "System.Collections.Generic.List<System.DateTime>");
				}

				// Check gs_local's properties
				var gs_local_props = await GetObjectOnFrame (pause_location ["callFrames"][0], "gs_local");
				await CheckProps (gs_local_props, new {
						StringField = TString ("gs_local#GenericStruct<ValueTypesTest>#StringField"),
						List        = TObject ("System.Collections.Generic.List<DebuggerTests.ValueTypesTest>", is_null: true),
						Options     = TEnum   ("DebuggerTests.Options", "None")
					}, "gs_local");

				// Check vt_local's properties
				var vt_local_props = await GetObjectOnFrame (pause_location ["callFrames"][0], "vt_local");
				Assert.Equal (5, vt_local_props.Count());

				CheckString (vt_local_props, "StringField", "string#0");
				CheckValueType (vt_local_props, "SimpleStructField", "DebuggerTests.ValueTypesTest.SimpleStruct");
				CheckValueType (vt_local_props, "SimpleStructProperty", "DebuggerTests.ValueTypesTest.SimpleStruct");
				await CheckDateTime (vt_local_props, "DT", new DateTime (2020, 1, 2, 3, 4, 5));
				CheckEnum (vt_local_props, "RGB", "DebuggerTests.RGB", "Blue");

				{
					// SimpleStructProperty
					dt = new DateTime (2022, 3, 4, 5, 7, 8);
					var ssp_props = await CompareObjectPropertiesFor (vt_local_props, "SimpleStructProperty",
						new {
							str_member = TString ("SimpleStructProperty#string#0#SimpleStruct#str_member"),
							dt = TValueType ("System.DateTime", dt.ToString ()),
							gs = TValueType ("DebuggerTests.ValueTypesTest.GenericStruct<System.DateTime>"),
							Kind = TEnum ("System.DateTimeKind", "Utc")
						},
						label: "vt_local_props.SimpleStructProperty");

					await CheckDateTime (ssp_props, "dt", dt);

					// SimpleStructField
					dt = new DateTime (2025, 6, 7, 8, 10, 11);
					var ssf_props = await CompareObjectPropertiesFor (vt_local_props, "SimpleStructField",
						new {
							str_member = TString ("SimpleStructField#string#0#SimpleStruct#str_member"),
							dt = TValueType ("System.DateTime", dt.ToString ()),
							gs = TValueType ("DebuggerTests.ValueTypesTest.GenericStruct<System.DateTime>"),
							Kind = TEnum ("System.DateTimeKind", "Local")
						},
						label: "vt_local_props.SimpleStructField");

					await CheckDateTime (ssf_props, "dt", dt);
				}

				// FIXME: check ss_local.gs.List's members
			});
		}

		[Theory]
		[InlineData (false)]
		[InlineData (true)]
		public async Task InspectValueTypeMethodArgs (bool use_cfo) {
			var insp = new Inspector ();
			//Collect events
			var scripts = SubscribeToScripts(insp);

			await Ready();
			await insp.Ready (async (cli, token) => {
				ctx = new DebugTestContext (cli, insp, token, scripts);
				ctx.UseCallFunctionOnBeforeGetProperties = use_cfo;
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

				var dt = new DateTime (2025, 6, 7, 8, 10, 11);
				var ss_local_as_ss_arg = new {
					str_member = TString    ("ss_local#SimpleStruct#string#0#SimpleStruct#str_member"),
					dt         = TValueType ("System.DateTime", dt.ToString ()),
					gs         = TValueType ("DebuggerTests.ValueTypesTest.GenericStruct<System.DateTime>"),
					Kind       = TEnum      ("System.DateTimeKind", "Local")
				};
				var ss_local_gs = new {
					StringField = TString ("ss_local#SimpleStruct#string#0#SimpleStruct#gs#StringField"),
					List        = TObject ("System.Collections.Generic.List<System.DateTime>"),
					Options     = TEnum   ("DebuggerTests.Options", "Option1")
				};

				// Check ss_arg's properties
				var ss_arg_props = await GetObjectOnFrame (pause_location ["callFrames"][0], "ss_arg");
				await CheckProps (ss_arg_props, ss_local_as_ss_arg, "ss_arg");

				{
					// Check ss_local.dt
					await CheckDateTime (ss_arg_props, "dt", dt);

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
					dt         = TValueType ("System.DateTime", dt.ToString ()),
					gs         = TValueType ("DebuggerTests.ValueTypesTest.GenericStruct<System.DateTime>"),
					Kind       = TEnum      ("System.DateTimeKind", "Utc")
				};

				ss_arg_props = await GetObjectOnFrame (pause_location ["callFrames"][0], "ss_arg");
				await CheckProps (ss_arg_props, ss_arg_updated, "ss_ar");

				{
					// Check ss_local.gs
					await CompareObjectPropertiesFor (ss_arg_props, "gs", new {
									StringField = TString ("ValueTypesTest#MethodWithStructArgs#updated#gs#StringField#3"),
									List        = TObject ("System.Collections.Generic.List<System.DateTime>"),
									Options     = TEnum   ("DebuggerTests.Options", "Option1")
							});

					await CheckDateTime (ss_arg_props, "dt", dt);
				}

				// Check locals on previous frame, same as earlier in this test
				ss_arg_props = await GetObjectOnFrame (pause_location ["callFrames"][1], "ss_local");
				await CheckProps (ss_arg_props, ss_local_as_ss_arg, "ss_local");

				{
					// Check ss_local.dt
					await CheckDateTime (ss_arg_props, "dt", dt);

					// Check ss_local.gs
					var gs_props = await GetObjectOnLocals (ss_arg_props, "gs");
					CheckString (gs_props, "StringField", "ss_local#SimpleStruct#string#0#SimpleStruct#gs#StringField");
					CheckObject (gs_props, "List", "System.Collections.Generic.List<System.DateTime>");
				}

				// ----------- Step back to the caller ---------

				pause_location = await StepAndCheck (StepKind.Over, debugger_test_loc, 22, 3, "TestStructsAsMethodArgs",
							times: 2, locals_fn: (l) => { /* non-null to make sure that locals get fetched */} );
				var locals = await GetProperties (pause_location ["callFrames"][0]["callFrameId"].Value<string> ());
				await CheckProps (locals, new {
					ss_local =  TValueType ("DebuggerTests.ValueTypesTest.SimpleStruct"),
					ss_ret   =  TValueType ("DebuggerTests.ValueTypesTest.SimpleStruct")
					},
					"locals#0");

				ss_arg_props = await GetObjectOnFrame (pause_location ["callFrames"] [0], "ss_local");
				await CheckProps (ss_arg_props, ss_local_as_ss_arg, "ss_local");

				{
					// Check ss_local.gs
					await CompareObjectPropertiesFor (ss_arg_props, "gs", ss_local_gs, label: "ss_local_gs");
				}

				// FIXME: check ss_local.gs.List's members
			});
		}

		[Theory]
		[InlineData (false)]
		[InlineData (true)]
		public async Task InspectLocalsWithStructsStaticAsync (bool use_cfo) {
			var insp = new Inspector ();
			//Collect events
			var scripts = SubscribeToScripts(insp);

			await Ready();
			await insp.Ready (async (cli, token) => {
				ctx = new DebugTestContext (cli, insp, token, scripts);
				ctx.UseCallFunctionOnBeforeGetProperties = use_cfo;
				var debugger_test_loc = "dotnet://debugger-test.dll/debugger-valuetypes-test.cs";

				await SetBreakpoint (debugger_test_loc, 47, 3);

				var pause_location = await EvaluateAndCheck (
					"window.setTimeout(function() { invoke_static_method_async ("
						+ "'[debugger-test] DebuggerTests.ValueTypesTest:MethodWithLocalStructsStaticAsync'"
					+ "); }, 1);",
					debugger_test_loc, 47, 3, "MoveNext"); //BUG: method name

				var locals = await GetProperties (pause_location ["callFrames"][0]["callFrameId"].Value<string> ());
				await CheckProps (locals, new {
					ss_local = TObject ("DebuggerTests.ValueTypesTest.SimpleStruct"),
					gs_local = TValueType ("DebuggerTests.ValueTypesTest.GenericStruct<int>"),
					result   = TBool (true)
					},
					"locals#0");

				var dt = new DateTime (2021, 2, 3, 4, 6, 7);
				// Check ss_local's properties
				var ss_local_props = await GetObjectOnFrame (pause_location ["callFrames"][0], "ss_local");
				await CheckProps (ss_local_props, new {
						str_member = TString ("set in MethodWithLocalStructsStaticAsync#SimpleStruct#str_member"),
						dt         = TValueType ("System.DateTime", dt.ToString ()),
						gs         = TValueType ("DebuggerTests.ValueTypesTest.GenericStruct<System.DateTime>"),
						Kind       = TEnum ("System.DateTimeKind", "Utc")
					}, "ss_local");

				{
					// Check ss_local.dt
					await CheckDateTime (ss_local_props, "dt", dt);

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
				var gs_local_props = await GetObjectOnFrame (pause_location ["callFrames"][0], "gs_local");
				await CheckProps (gs_local_props, new {
						StringField = TString ("gs_local#GenericStruct<ValueTypesTest>#StringField"),
						List        = TObject ("System.Collections.Generic.List<int>"),
						Options     = TEnum   ("DebuggerTests.Options", "Option2")
					}, "gs_local");

				// FIXME: check ss_local.gs.List's members
			});
		}

		[Theory]
		[InlineData (16, 2, "PrimitiveTypeLocals", false, 0, false)]
		[InlineData (16, 2, "PrimitiveTypeLocals", false, 0, true)]
		[InlineData (93, 2, "YetAnotherMethod", true, 2, false)]
		[InlineData (93, 2, "YetAnotherMethod", true, 2, true)]
		public async Task InspectPrimitiveTypeArrayLocals (int line, int col, string method_name, bool test_prev_frame, int frame_idx, bool use_cfo)
			=> await TestSimpleArrayLocals (
				line, col,
				entry_method_name: "[debugger-test] DebuggerTests.ArrayTestsClass:PrimitiveTypeLocals",
				method_name: method_name,
				etype_name: "int",
				local_var_name_prefix: "int",
				array: new [] { TNumber (4), TNumber (70), TNumber (1) },
				array_elements: null,
				test_prev_frame: test_prev_frame,
				frame_idx: frame_idx,
				use_cfo: use_cfo);

		internal static Func<int, int, string, string, object> TSimpleClass = (X, Y, Id, Color) => new {
			X =     TNumber (X),
			Y =     TNumber (Y),
			Id =    TString (Id),
			Color = TEnum ("DebuggerTests.RGB", Color),
			//PointWithCustomGetter = TValueType ("DebuggerTests.Point")
			// only automatic properties are supported currently!
			PointWithCustomGetter = TSymbol ("DebuggerTests.Point { get; }")
		};

		internal static Func<int, int, string, string, object> TPoint = (X, Y, Id, Color) => new {
			X =     TNumber (X),
			Y =     TNumber (Y),
			Id =    TString (Id),
			Color = TEnum ("DebuggerTests.RGB", Color),
		};

		[Theory]
		[InlineData (32, 2, "ValueTypeLocals", false, 0, false)]
		[InlineData (32, 2, "ValueTypeLocals", false, 0, true)]
		[InlineData (93, 2, "YetAnotherMethod", true, 2, false)]
		[InlineData (93, 2, "YetAnotherMethod", true, 2, true)]
		public async Task InspectValueTypeArrayLocals (int line, int col, string method_name, bool test_prev_frame, int frame_idx, bool use_cfo)
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
				frame_idx: frame_idx,
				use_cfo: use_cfo);

		[Theory]
		[InlineData (49, 2, "ObjectTypeLocals", false, 0, false)]
		[InlineData (49, 2, "ObjectTypeLocals", false, 0, true)]
		[InlineData (93, 2, "YetAnotherMethod", true, 2, false)]
		[InlineData (93, 2, "YetAnotherMethod", true, 2, true)]
		public async Task InspectObjectArrayLocals (int line, int col, string method_name, bool test_prev_frame, int frame_idx, bool use_cfo)
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
				frame_idx: frame_idx,
				use_cfo: use_cfo);

		[Theory]
		[InlineData (66, 2, "GenericTypeLocals", false, 0, false)]
		[InlineData (66, 2, "GenericTypeLocals", false, 0, true)]
		[InlineData (93, 2, "YetAnotherMethod", true, 2, false)]
		[InlineData (93, 2, "YetAnotherMethod", true, 2, true)]
		public async Task InspectGenericTypeArrayLocals (int line, int col, string method_name, bool test_prev_frame, int frame_idx, bool use_cfo)
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
				frame_idx: frame_idx,
				use_cfo: use_cfo);

		[Theory]
		[InlineData (82, 2, "GenericValueTypeLocals", false, 0, false)]
		[InlineData (82, 2, "GenericValueTypeLocals", false, 0, true)]
		[InlineData (93, 2, "YetAnotherMethod", true, 2, false)]
		[InlineData (93, 2, "YetAnotherMethod", true, 2, true)]
		public async Task InspectGenericValueTypeArrayLocals (int line, int col, string method_name, bool test_prev_frame, int frame_idx, bool use_cfo)
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
				frame_idx: frame_idx,
				use_cfo: use_cfo);

		[Theory]
		[InlineData (191, 2, "GenericValueTypeLocals2", false, 0, false)]
		[InlineData (191, 2, "GenericValueTypeLocals2", false, 0, true)]
		[InlineData (93, 2, "YetAnotherMethod", true, 2, false)]
		[InlineData (93, 2, "YetAnotherMethod", true, 2, true)]
		public async Task InspectGenericValueTypeArrayLocals2 (int line, int col, string method_name, bool test_prev_frame, int frame_idx, bool use_cfo)
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
				frame_idx: frame_idx,
				use_cfo: use_cfo);

		async Task TestSimpleArrayLocals (int line, int col, string entry_method_name, string method_name, string etype_name,
							string local_var_name_prefix, object[] array, object[] array_elements,
							bool test_prev_frame=false, int frame_idx=0, bool use_cfo = false)
		{
			var insp = new Inspector ();
			//Collect events
			var scripts = SubscribeToScripts(insp);

			await Ready();
			await insp.Ready (async (cli, token) => {
				ctx = new DebugTestContext (cli, insp, token, scripts);
				var debugger_test_loc = "dotnet://debugger-test.dll/debugger-array-test.cs";
				ctx.UseCallFunctionOnBeforeGetProperties = use_cfo;

				await SetBreakpoint (debugger_test_loc, line, col);

				var eval_expr = "window.setTimeout(function() { invoke_static_method ("
							+ $"'{entry_method_name}', { (test_prev_frame ? "true" : "false") }"
						+ "); }, 1);";

				var pause_location = await EvaluateAndCheck (eval_expr, debugger_test_loc, line, col, method_name);

				var locals = await GetProperties (pause_location ["callFrames"][frame_idx]["callFrameId"].Value<string> ());
				Assert.Equal (4, locals.Count ());
				CheckArray (locals, $"{local_var_name_prefix}_arr", $"{etype_name}[]");
				CheckArray (locals, $"{local_var_name_prefix}_arr_empty", $"{etype_name}[]");
				CheckObject (locals, $"{local_var_name_prefix}_arr_null", $"{etype_name}[]", is_null: true);
				CheckBool (locals, "call_other", test_prev_frame);

				var local_arr_name = $"{local_var_name_prefix}_arr";

				JToken prefix_arr;
				if (use_cfo) {   // Use `Runtime.callFunctionOn` to get the properties
					var frame = pause_location ["callFrames"][frame_idx];
					var name = local_arr_name;
					var fl = await GetProperties (frame ["callFrameId"].Value<string> ());
					var l_obj = GetAndAssertObjectWithName (locals, name);
					var l_objectId = l_obj ["value"]["objectId"]?.Value<string> ();

					Assert.True (!String.IsNullOrEmpty (l_objectId), $"No objectId found for {name}");

					prefix_arr = await GetObjectWithCFO (l_objectId);
				} else {
					prefix_arr = await GetObjectOnFrame (pause_location ["callFrames"][frame_idx], local_arr_name);
				}

				await CheckProps (prefix_arr, array, local_arr_name);

				if (array_elements?.Length > 0) {
					for (int i = 0; i < array_elements.Length; i ++) {
						var i_str = i.ToString ();
						var label = $"{local_var_name_prefix}_arr[{i}]";
						if (array_elements [i] == null) {
							var act_i = prefix_arr.FirstOrDefault (jt => jt ["name"]?.Value<string> () == i_str);
							Assert.True (act_i != null, $"[{label}] Couldn't find array element [{i_str}]");

							await CheckValue (act_i ["value"], TObject (etype_name, is_null: true), label);
						} else {
							await CompareObjectPropertiesFor (prefix_arr, i_str, array_elements [i], label: label);
						}
					}
				}

				var props = await GetObjectOnFrame (pause_location ["callFrames"][frame_idx], $"{local_var_name_prefix}_arr_empty");
				await CheckProps (props, new object[0], "${local_var_name_prefix}_arr_empty");
			});

			async Task<JToken> GetObjectWithCFO (string objectId, JObject fn_args = null)
			{
				var fn_decl = "function () { return this; }";
				var cfo_args = JObject.FromObject (new {
					functionDeclaration = fn_decl,
					objectId = objectId
				});

				if (fn_args != null)
					cfo_args ["arguments"] = fn_args;

				// callFunctionOn
				var result = await ctx.cli.SendCommand ("Runtime.callFunctionOn", cfo_args, ctx.token);

				return await GetProperties (result.Value ["result"]["objectId"]?.Value<string> (), fn_args);
			}
		}

		[Theory]
		[InlineData (false)]
		[InlineData (true)]
		public async Task InspectObjectArrayMembers (bool use_cfo)
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
				ctx.UseCallFunctionOnBeforeGetProperties = use_cfo;
				var debugger_test_loc = "dotnet://debugger-test.dll/debugger-array-test.cs";

				await SetBreakpoint (debugger_test_loc, line, col);

				var eval_expr = "window.setTimeout(function() { invoke_static_method ("
							+ $"'{entry_method_name}'"
						+ "); }, 1);";

				var pause_location = await EvaluateAndCheck (eval_expr, debugger_test_loc, line, col, method_name);
				var locals = await GetProperties (pause_location ["callFrames"][frame_idx]["callFrameId"].Value<string> ());
				Assert.Single (locals);
				CheckObject (locals, "c", "DebuggerTests.Container");

				var c_props = await GetObjectOnFrame (pause_location ["callFrames"][frame_idx], "c");
				await CheckProps (c_props, new {
						id = TString ("c#id"),
						ClassArrayProperty = TArray ("DebuggerTests.SimpleClass[]", 3),
						ClassArrayField = TArray ("DebuggerTests.SimpleClass[]", 3),
						PointsProperty = TArray ("DebuggerTests.Point[]", 2),
						PointsField = TArray ("DebuggerTests.Point[]", 2)
					},
					"c"
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

		[Theory]
		[InlineData (false)]
		[InlineData (true)]
		public async Task InspectValueTypeArrayLocalsStaticAsync (bool use_cfo)
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
				ctx.UseCallFunctionOnBeforeGetProperties = use_cfo;
				var debugger_test_loc = "dotnet://debugger-test.dll/debugger-array-test.cs";

				await SetBreakpoint (debugger_test_loc, line, col);

				var eval_expr = "window.setTimeout(function() { invoke_static_method_async ("
							+ $"'{entry_method_name}', false" // *false* here keeps us only in the static method
						+ "); }, 1);";

				var pause_location = await EvaluateAndCheck (eval_expr, debugger_test_loc, line, col, method_name);
				var frame_locals = await GetProperties (pause_location ["callFrames"][frame_idx]["callFrameId"].Value<string> ());
				await CheckProps (frame_locals, new {
					call_other = TBool (false),
					gvclass_arr = TArray ("DebuggerTests.SimpleGenericStruct<DebuggerTests.Point>[]", 2),
					gvclass_arr_empty = TArray ("DebuggerTests.SimpleGenericStruct<DebuggerTests.Point>[]"),
					gvclass_arr_null = TObject ("DebuggerTests.SimpleGenericStruct<DebuggerTests.Point>[]", is_null: true),
					gvclass = TValueType ("DebuggerTests.SimpleGenericStruct<DebuggerTests.Point>"),
					// BUG: this shouldn't be null!
					points = TObject ("DebuggerTests.Point[]", is_null: true)
				}, "ValueTypeLocalsAsync#locals");

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
		[Theory]
		[InlineData (false)]
		[InlineData (true)]
		public async Task InspectValueTypeArrayLocalsInstanceAsync (bool use_cfo)
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
				ctx.UseCallFunctionOnBeforeGetProperties = use_cfo;
				var debugger_test_loc = "dotnet://debugger-test.dll/debugger-array-test.cs";

				await SetBreakpoint (debugger_test_loc, line, col);

				var eval_expr = "window.setTimeout(function() { invoke_static_method_async ("
							+ $"'{entry_method_name}', true"
						+ "); }, 1);";

				// BUG: Should be InspectValueTypeArrayLocalsInstanceAsync
				var pause_location = await EvaluateAndCheck (eval_expr, debugger_test_loc, line, col, "MoveNext");

				var frame_locals = await GetProperties (pause_location ["callFrames"][frame_idx]["callFrameId"].Value<string> ());
				await CheckProps (frame_locals, new {
					t1 = TObject ("DebuggerTests.SimpleGenericStruct<DebuggerTests.Point>"),
					@this = TObject ("DebuggerTests.ArrayTestsClass"),
					point_arr = TArray ("DebuggerTests.Point[]", 2),
					point = TValueType ("DebuggerTests.Point")
				}, "InspectValueTypeArrayLocalsInstanceAsync#locals");

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

		// This tests `callFunctionOn` with a function that the vscode-js-debug extension uses
		// Using this here as a non-trivial test case
		[Theory]
		[InlineData ("big_array_js_test (10);", "/other.js", 5, 1, 10, false)]
		[InlineData ("big_array_js_test (0);", "/other.js", 5, 1, 0, true)]
		[InlineData ("invoke_static_method ('[debugger-test] Math:CallFunctionOnTest', 10);", "dotnet://debugger-test.dll/debugger-test.cs", 311, 2, 10, false)]
		[InlineData ("invoke_static_method ('[debugger-test] Math:CallFunctionOnTest', 0);", "dotnet://debugger-test.dll/debugger-test.cs", 311, 2, 0, true)]
		public async Task CFO_CheckVSCodeTestFunction1 (string eval_fn, string bp_loc, int line, int col, int len, bool roundtrip)
		{
			string vscode_fn0 = "function(){const e={__proto__:this.__proto__},t=Object.getOwnPropertyNames(this);for(let r=0;r<t.length;++r){const n=t[r],i=n>>>0;if(String(i>>>0)===n&&i>>>0!=4294967295)continue;const a=Object.getOwnPropertyDescriptor(this,n);a&&Object.defineProperty(e,n,a)}return e}";

			await RunCallFunctionOn (eval_fn, vscode_fn0, "big", bp_loc, line, col, res_array_len: len, roundtrip: roundtrip,
				test_fn: async (result) => {

					var is_js = bp_loc.EndsWith (".js", StringComparison.Ordinal);
					var obj_accessors = await ctx.cli.SendCommand ("Runtime.getProperties", JObject.FromObject (new {
						objectId = result.Value ["result"]["objectId"].Value<string> (),
						accessorPropertiesOnly = true,
						ownProperties = false
					}), ctx.token);
					if (is_js)
						await CheckProps (obj_accessors.Value ["result"], new { __proto__ = TIgnore () }, "obj_accessors");
					else
						AssertEqual (0, obj_accessors.Value ["result"]?.Count (), "obj_accessors-count");

					// Check for a __proto__ object
					// isOwn = true, accessorPropertiesOnly = false
					var obj_own = await ctx.cli.SendCommand ("Runtime.getProperties", JObject.FromObject (new {
						objectId = result.Value ["result"]["objectId"].Value<string> (),
						accessorPropertiesOnly = false,
						ownProperties = true
					}), ctx.token);

					await CheckProps (obj_own.Value ["result"], new {
						length = TNumber (len),
						// __proto__ = TArray (type, 0) // Is this one really required?
					}, $"obj_own", num_fields: is_js ? 2 : 1);

				});
		}

		void CheckJFunction (JToken actual, string className, string label)
		{
			AssertEqual ("function", actual ["type"]?.Value<string> (), $"{label}-type");
			AssertEqual (className, actual ["className"]?.Value<string> (), $"{label}-className");
		}

		// This tests `callFunctionOn` with a function that the vscode-js-debug extension uses
		// Using this here as a non-trivial test case
		[Theory]
		[InlineData ("big_array_js_test (10);", "/other.js", 5, 1, 10)]
		[InlineData ("big_array_js_test (0);", "/other.js", 5, 1, 0)]
		[InlineData ("invoke_static_method ('[debugger-test] Math:CallFunctionOnTest', 10);", "dotnet://debugger-test.dll/debugger-test.cs", 311, 2, 10)]
		[InlineData ("invoke_static_method ('[debugger-test] Math:CallFunctionOnTest', 0);", "dotnet://debugger-test.dll/debugger-test.cs", 311, 2, 0)]
		public async Task CFO_CheckVSCodeTestFunction2 (string eval_fn, string bp_loc, int line, int col, int len)
		{
			var fetch_start_idx = 2;
			var num_elems_fetch = 3;
			string vscode_fn1 = "function(e,t){const r={},n=-1===e?0:e,i=-1===t?this.length:e+t;for(let e=n;e<i&&e<this.length;++e){const t=Object.getOwnPropertyDescriptor(this,e);t&&Object.defineProperty(r,e,t)}return r}";

			await RunCallFunctionOn (eval_fn, vscode_fn1, "big", bp_loc, line, col,
				fn_args: JArray.FromObject (new [] {
						new { @value = fetch_start_idx },
						new { @value = num_elems_fetch }
					}),
				test_fn: async (result) => {

					var is_js = bp_loc.EndsWith (".js", StringComparison.Ordinal);

					// isOwn = false, accessorPropertiesOnly = true
					var obj_accessors = await ctx.cli.SendCommand ("Runtime.getProperties", JObject.FromObject (new {
						objectId = result.Value ["result"]["objectId"].Value<string> (),
						accessorPropertiesOnly = true,
						ownProperties = false
					}), ctx.token);
					if (is_js)
						await CheckProps (obj_accessors.Value ["result"], new { __proto__ = TIgnore () }, "obj_accessors");
					else
						AssertEqual (0, obj_accessors.Value ["result"]?.Count (), "obj_accessors-count");

					// Ignoring the __proto__ property

					// isOwn = true, accessorPropertiesOnly = false
					var obj_own = await ctx.cli.SendCommand ("Runtime.getProperties", JObject.FromObject (new {
						objectId = result.Value ["result"]["objectId"].Value<string> (),
						accessorPropertiesOnly = false,
						ownProperties = true
					}), ctx.token);

					var obj_own_val = obj_own.Value ["result"];
					var num_elems_recd = len == 0 ? 0 : num_elems_fetch;
					AssertEqual (is_js ? num_elems_recd + 1 : num_elems_recd, obj_own_val.Count (), $"obj_own-count");

					if (is_js)
						CheckObject (obj_own_val, "__proto__", "Object");

					for (int i = fetch_start_idx; i < fetch_start_idx + num_elems_recd; i ++)
						CheckNumber (obj_own_val, i.ToString (), 1000 + i);
			});
		}

		[Theory]
		[InlineData ("big_array_js_test (10);", "/other.js", 5, 1, false)]
		[InlineData ("big_array_js_test (10);", "/other.js", 5, 1, true)]
		[InlineData ("invoke_static_method ('[debugger-test] Math:CallFunctionOnTest', 10);", "dotnet://debugger-test.dll/debugger-test.cs", 311, 2, false)]
		[InlineData ("invoke_static_method ('[debugger-test] Math:CallFunctionOnTest', 10);", "dotnet://debugger-test.dll/debugger-test.cs", 311, 2, true)]
		public async Task CFO_RunOnArrayReturnEmptyArray (string eval_fn, string bp_loc, int line, int col, bool roundtrip)
		{
			var ret_len = 0;

			await RunCallFunctionOn (eval_fn,
				"function () { return []; }",
				"big", bp_loc, line, col,
				res_array_len: ret_len,
				roundtrip: roundtrip,
				test_fn: async (result) => {
					var is_js = bp_loc.EndsWith (".js", StringComparison.Ordinal);

					// getProperties (isOwn = false, accessorPropertiesOnly = true)
					var obj_accessors = await ctx.cli.SendCommand ("Runtime.getProperties", JObject.FromObject (new {
						objectId = result.Value ["result"]["objectId"].Value<string> (),
						accessorPropertiesOnly = true,
						ownProperties = false
					}), ctx.token);
					if (is_js)
						await CheckProps (obj_accessors.Value ["result"], new { __proto__ = TIgnore () }, "obj_accessors");
					else
						AssertEqual (0, obj_accessors.Value ["result"]?.Count (), "obj_accessors-count");

					// getProperties (isOwn = true, accessorPropertiesOnly = false)
					var obj_own = await ctx.cli.SendCommand ("Runtime.getProperties", JObject.FromObject (new {
						objectId = result.Value ["result"]["objectId"].Value<string> (),
						accessorPropertiesOnly = false,
						ownProperties = true
					}), ctx.token);

					await CheckProps (obj_own.Value ["result"], new {
						length = TNumber (ret_len),
						// __proto__ returned by js
					}, $"obj_own", num_fields: is_js ? 2 : 1);
			});
		}

		[Theory]
		[InlineData ("big_array_js_test (10);", "/other.js", 5, 1, false)]
		[InlineData ("big_array_js_test (10);", "/other.js", 5, 1, true)]
		[InlineData ("invoke_static_method ('[debugger-test] Math:CallFunctionOnTest', 10);", "dotnet://debugger-test.dll/debugger-test.cs", 311, 2, false)]
		[InlineData ("invoke_static_method ('[debugger-test] Math:CallFunctionOnTest', 10);", "dotnet://debugger-test.dll/debugger-test.cs", 311, 2, true)]
		public async Task CFO_RunOnArrayReturnArray (string eval_fn, string bp_loc, int line, int col, bool roundtrip)
		{
			var ret_len = 5;
			await RunCallFunctionOn (eval_fn,
				"function (m) { return Object.values (this).filter ((k, i) => i%m == 0); }",
				"big", bp_loc, line, col,
				fn_args: JArray.FromObject (new [] { new { value = 2 } }),
				res_array_len: ret_len,
				roundtrip: roundtrip,
				test_fn: async (result) => {
					var is_js = bp_loc.EndsWith (".js");

					// getProperties (own=false)
					var obj_accessors = await ctx.cli.SendCommand ("Runtime.getProperties", JObject.FromObject (new {
						objectId = result.Value ["result"]["objectId"].Value<string> (),
						accessorPropertiesOnly = true,
						ownProperties = false
					}), ctx.token);

					if (is_js)
						await CheckProps (obj_accessors.Value ["result"], new { __proto__ = TIgnore () }, "obj_accessors");
					else
						AssertEqual (0, obj_accessors.Value ["result"]?.Count (), "obj_accessors-count");

					// getProperties (own=true)
					// isOwn = true, accessorPropertiesOnly = false
					var obj_own = await ctx.cli.SendCommand ("Runtime.getProperties", JObject.FromObject (new {
						objectId = result.Value ["result"]["objectId"].Value<string> (),
						accessorPropertiesOnly = false,
						ownProperties = true
					}), ctx.token);

					// AssertEqual (2, obj_own.Value ["result"].Count (), $"{label}-obj_own.count");

					var obj_own_val = obj_own.Value ["result"];
					await CheckProps (obj_own_val, new {
						length = TNumber (ret_len),
						// __proto__ returned by JS
					}, $"obj_own", num_fields: (is_js ? ret_len  + 2 : ret_len + 1));

					for (int i = 0; i < ret_len; i ++)
						CheckNumber (obj_own_val, i.ToString (), i*2 + 1000);
			});
		}

		[Theory]
		[InlineData (false)]
		[InlineData (true)]
		public async Task CFO_RunOnVTArray (bool roundtrip)
		=>	await RunCallFunctionOn (
				"invoke_static_method ('[debugger-test] Math:CallFunctionOnTest', 10);",
				"function (m) { return Object.values (this).filter ((k, i) => i%m == 0); }",
				"ss_arr",
				"dotnet://debugger-test.dll/debugger-test.cs", 311, 2,
				fn_args: JArray.FromObject (new [] { new { value = 2 } }),
				res_array_len: 5,
				roundtrip: roundtrip,
				test_fn: async (result) => {
					var ret_len = 5;

					// getProperties (own=false)
					var obj_accessors = await ctx.cli.SendCommand ("Runtime.getProperties", JObject.FromObject (new {
						objectId = result.Value ["result"]["objectId"].Value<string> (),
						accessorPropertiesOnly = true,
						ownProperties = false
					}), ctx.token);

					AssertEqual (0, obj_accessors.Value ["result"]?.Count (), "obj_accessors-count");

					// getProperties (own=true)
					// isOwn = true, accessorPropertiesOnly = false
					var obj_own = await ctx.cli.SendCommand ("Runtime.getProperties", JObject.FromObject (new {
						objectId = result.Value ["result"]["objectId"].Value<string> (),
						accessorPropertiesOnly = false,
						ownProperties = true
					}), ctx.token);

					var obj_own_val = obj_own.Value ["result"];
					await CheckProps (obj_own_val, new {
						length = TNumber (ret_len),
						// __proto__ returned by JS
					}, "obj_own", num_fields: ret_len + 1);

					for (int i = 0; i < ret_len; i ++) {
						var act_i = CheckValueType (obj_own_val, i.ToString (), "Math.SimpleStruct");

						// Valuetypes can get sent as part of the container's getProperties, so ensure that we can access it
						var act_i_props = await GetProperties (act_i ["value"]["objectId"]?.Value<string> ());
						await CheckProps (act_i_props, new {
							dt = TValueType ("System.DateTime", new DateTime (2020 + (i*2), 1, 2, 3, 4, 5).ToString ()),
							gs = TValueType ("Math.GenericStruct<System.DateTime>")
						}, "obj_own ss_arr[{i}]");

						var gs_props = await GetObjectOnLocals (act_i_props, "gs");
						await CheckProps (gs_props, new {
							List = TObject ("System.Collections.Generic.List<System.DateTime>", is_null: true),
							StringField = TString ($"ss_arr # {i*2} # gs # StringField")
						}, "obj_own ss_arr[{i}].gs");

					}
			});

		[Theory]
		[InlineData (false)]
		[InlineData (true)]
		public async Task CFO_RunOnCFOValueTypeResult (bool roundtrip)
		=>	await RunCallFunctionOn (
				eval_fn:    "invoke_static_method ('[debugger-test] Math:CallFunctionOnTest', 10);",
				fn_decl:    "function () { return this; }",
				local_name: "simple_struct",
				bp_loc:     "dotnet://debugger-test.dll/debugger-test.cs", 311, 2,
				roundtrip:  roundtrip,
				test_fn: async (result) => {

					// getProperties (own=false)
					var obj_accessors = await ctx.cli.SendCommand ("Runtime.getProperties", JObject.FromObject (new {
						objectId = result.Value ["result"]["objectId"].Value<string> (),
						accessorPropertiesOnly = true,
						ownProperties = false
					}), ctx.token);
					AssertEqual (0, obj_accessors.Value ["result"].Count (), "obj_accessors-count");

					// getProperties (own=true)
					// isOwn = true, accessorPropertiesOnly = false
					var obj_own = await ctx.cli.SendCommand ("Runtime.getProperties", JObject.FromObject (new {
						objectId = result.Value ["result"]["objectId"].Value<string> (),
						accessorPropertiesOnly = false,
						ownProperties = true
					}), ctx.token);

					var obj_own_val = obj_own.Value ["result"];
					var dt = new DateTime (2020, 1, 2, 3, 4, 5);
					await CheckProps (obj_own_val, new {
						dt = TValueType ("System.DateTime", dt.ToString ()),
						gs = TValueType ("Math.GenericStruct<System.DateTime>")
					}, $"obj_own-props");

					await CheckDateTime (obj_own_val, "dt", dt);

					var gs_props = await GetObjectOnLocals (obj_own_val, "gs");
					await CheckProps (gs_props, new {
						List = TObject ("System.Collections.Generic.List<System.DateTime>", is_null: true),
						StringField = TString ($"simple_struct # gs # StringField")
					}, "simple_struct.gs-props");
			});

		[Theory]
		[InlineData (false)]
		[InlineData (true)]
		public async Task CFO_RunOnJSObject (bool roundtrip)
		=>	await RunCallFunctionOn (
				"object_js_test ();",
				"function () { return this; }",
				"obj", "/other.js", 14, 1,
				fn_args: JArray.FromObject (new [] { new { value = 2 } }),
				roundtrip: roundtrip,
				test_fn: async (result) => {

					// getProperties (own=false)
					var obj_accessors = await ctx.cli.SendCommand ("Runtime.getProperties", JObject.FromObject (new {
						objectId = result.Value ["result"]["objectId"].Value<string> (),
						accessorPropertiesOnly = true,
						ownProperties = false
					}), ctx.token);

					await CheckProps (obj_accessors.Value ["result"], new { __proto__ = TIgnore () }, "obj_accessors");

					// getProperties (own=true)
					// isOwn = true, accessorPropertiesOnly = false
					var obj_own = await ctx.cli.SendCommand ("Runtime.getProperties", JObject.FromObject (new {
						objectId = result.Value ["result"]["objectId"].Value<string> (),
						accessorPropertiesOnly = false,
						ownProperties = true
					}), ctx.token);

					var obj_own_val = obj_own.Value ["result"];
					await CheckProps (obj_own_val, new {
						a_obj = TObject ("Object"),
						b_arr = TArray ("Array", 2)
					}, "obj_own", num_fields: 3);
			});

		[Theory]
		[InlineData ("big_array_js_test (10);", "/other.js", 5, 1, false)]
		[InlineData ("big_array_js_test (10);", "/other.js", 5, 1, true)]
		[InlineData ("invoke_static_method ('[debugger-test] Math:CallFunctionOnTest', 10);", "dotnet://debugger-test.dll/debugger-test.cs", 311, 2, false)]
		[InlineData ("invoke_static_method ('[debugger-test] Math:CallFunctionOnTest', 10);", "dotnet://debugger-test.dll/debugger-test.cs", 311, 2, true)]
		public async Task CFO_RunOnArrayReturnObjectArrayByValue (string eval_fn, string bp_loc, int line, int col, bool roundtrip)
		{
			var ret_len = 5;
			await RunCallFunctionOn (eval_fn,
					"function () { return Object.values (this).filter ((k, i) => i%2 == 0); }",
					"big", bp_loc, line, col, returnByValue: true, roundtrip: roundtrip,
				test_fn: async (result) => {
					// Check cfo result
					AssertEqual ("object", result.Value ["result"]["type"]?.Value<string> (), "cfo-res-type");
					AssertEqual (1, result.Value.Values ().Count (), "cfo-res-value-count");

					var actual = result.Value ["result"]?["value"].Values<JToken> ().ToArray ();
					for (int i = 0; i < ret_len; i ++) {
						var exp_num = i*2 + 1000;
						if (bp_loc.EndsWith (".js", StringComparison.Ordinal))
							AssertEqual (exp_num, actual [i].Value<int> (), $"[{i}]");
						else {
							AssertEqual ("number", actual [i]?["type"]?.Value<string> (), $"[{i}]-type");
							AssertEqual (exp_num.ToString (), actual [i]?["description"]?.Value<string> (), $"[{i}]-description");
							AssertEqual (exp_num, actual [i]?["value"]?.Value<int> (), $"[{i}]-value");
						}
					}
					await Task.CompletedTask;
			});
		}

		[Theory]
		[InlineData ("big_array_js_test (10);", "/other.js", 5, 1, false)]
		[InlineData ("big_array_js_test (10);", "/other.js", 5, 1, true)]
		[InlineData ("invoke_static_method ('[debugger-test] Math:CallFunctionOnTest', 10);", "dotnet://debugger-test.dll/debugger-test.cs", 311, 2, false)]
		[InlineData ("invoke_static_method ('[debugger-test] Math:CallFunctionOnTest', 10);", "dotnet://debugger-test.dll/debugger-test.cs", 311, 2, true)]
		public async Task CFO_RunOnArrayReturnArrayByValue (string eval_fn, string bp_loc, int line, int col, bool roundtrip)
		=>	await RunCallFunctionOn (eval_fn,
				"function () { return Object.getOwnPropertyNames (this); }",
				"big", bp_loc, line, col, returnByValue: true,
				roundtrip: roundtrip,
				test_fn: async (result) => {
					// Check cfo result
					AssertEqual ("object", result.Value ["result"]["type"]?.Value<string> (), "cfo-res-type");

					var exp = new JArray ();
					for (int i = 0; i < 10; i ++)
						exp.Add (i.ToString ());
					exp.Add ("length");

					var actual = result.Value ["result"]?["value"];
					if (!JObject.DeepEquals (exp, actual)) {
						Assert.True (false, $"Results don't match.\nExpected: {exp}\nActual:  {actual}");
					}
					await Task.CompletedTask;
			});

		[Theory]
		[InlineData (null)]
		[InlineData (false)]
		[InlineData (true)]
		public async Task CFO_CheckErrorsWithSilent (bool? silent)
		{
			var insp = new Inspector ();
			//Collect events
			var scripts = SubscribeToScripts(insp);

			await Ready();
			await insp.Ready (async (cli, token) => {
				ctx = new DebugTestContext (cli, insp, token, scripts);
				await SetBreakpoint ("dotnet://debugger-test.dll/debugger-test.cs", 311, 2);

				// callFunctionOn
				var eval_expr = "window.setTimeout(function() { invoke_static_method ('[debugger-test] Math:CallFunctionOnTest', 10); }, 1);";
				var result = await ctx.cli.SendCommand ("Runtime.evaluate", JObject.FromObject (new { expression = eval_expr }), ctx.token);
				var pause_location = await ctx.insp.WaitFor (Inspector.PAUSE);

				// Check the object at the bp
				var cfo_args = JObject.FromObject (new {
					functionDeclaration = "function () { throw Error ('test error'); }",
					objectId = "dotnet:object:xyasd",
				});

				if (silent.HasValue)
					cfo_args ["silent"] = silent;

				// callFunctionOn
				result = await ctx.cli.SendCommand ("Runtime.callFunctionOn", cfo_args, ctx.token);
				Assert.True ((silent ?? false) == result.IsOk);
			});
		}

		/*
		 * 1. runs `Runtime.callFunctionOn` on the objectId,
		 * if @roundtrip == false, then
		 *     -> calls @test_fn for that result (new objectId)
		 * else
		 *     -> runs it again on the *result's* objectId.
		 *        -> calls @test_fn on the *new* result objectId
		 *
		 * Returns: result of `Runtime.callFunctionOn`
		 */ 
		async Task RunCallFunctionOn (string eval_fn, string fn_decl, string local_name, string bp_loc, int line, int col, int res_array_len = -1,
						Func<Result, Task> test_fn = null, bool returnByValue = false, JArray fn_args = null, bool roundtrip = false)
		{
			var insp = new Inspector ();
			//Collect events
			var scripts = SubscribeToScripts(insp);

			await Ready();
			await insp.Ready (async (cli, token) => {
				ctx = new DebugTestContext (cli, insp, token, scripts);
				await SetBreakpoint (bp_loc, line, col);

				// callFunctionOn
				var eval_expr = $"window.setTimeout(function() {{ {eval_fn} }}, 1);";
				var result = await ctx.cli.SendCommand ("Runtime.evaluate", JObject.FromObject (new { expression = eval_expr }), ctx.token);
				var pause_location = await ctx.insp.WaitFor (Inspector.PAUSE);

				// Um for js we get "scriptId": "6"
				// CheckLocation (bp_loc, line, col, ctx.scripts, pause_location ["callFrames"][0]["location"]);

				// Check the object at the bp
				var frame_locals = await GetProperties (pause_location ["callFrames"][0]["scopeChain"][0]["object"]["objectId"].Value<string> ());
				var obj = GetAndAssertObjectWithName (frame_locals, local_name);
				var obj_id = obj ["value"]["objectId"].Value<string> ();

				var cfo_args = JObject.FromObject (new {
					functionDeclaration = fn_decl,
					objectId = obj_id
				});

				if (fn_args != null)
					cfo_args ["arguments"] = fn_args;

				if (returnByValue)
					cfo_args ["returnByValue"] = returnByValue;

				// callFunctionOn
				result = await ctx.cli.SendCommand ("Runtime.callFunctionOn", cfo_args, ctx.token);
				await CheckCFOResult (result);

				// If it wasn't `returnByValue`, then try to run a new function
				// on that *returned* object
				// This second function, just returns the object as-is, so the same
				// test_fn is re-usable.
				if (!returnByValue && roundtrip) {
					cfo_args = JObject.FromObject (new {
						functionDeclaration = "function () { return this; }",
						objectId = result.Value ["result"]["objectId"]?.Value<string> ()
					});

					if (fn_args != null)
						cfo_args ["arguments"] = fn_args;

					result = await ctx.cli.SendCommand ("Runtime.callFunctionOn", cfo_args, ctx.token);

					await CheckCFOResult (result);
				}

				if (test_fn != null)
					await test_fn (result);

				return;

				async Task CheckCFOResult (Result result)
				{
					if (returnByValue)
						return;

					if (res_array_len < 0)
						await CheckValue (result.Value ["result"], TObject ("Object"), $"cfo-res");
					else
						await CheckValue (result.Value ["result"], TArray ("Array", res_array_len), $"cfo-res");
				}
			});
		}

		[Theory]
		[InlineData (false)]
		[InlineData (true)]
		public async Task InspectValueTypeArrayLocalsInAsyncStaticStructMethod (bool use_cfo)
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
				ctx.UseCallFunctionOnBeforeGetProperties = use_cfo;
				var debugger_test_loc = "dotnet://debugger-test.dll/debugger-array-test.cs";

				await SetBreakpoint (debugger_test_loc, line, col);
				//await SetBreakpoint (debugger_test_loc, 143, 3);

				var eval_expr = "window.setTimeout(function() { invoke_static_method_async ("
							+ $"'{entry_method_name}', false"
						+ "); }, 1);";

				// BUG: Should be InspectValueTypeArrayLocalsInstanceAsync
				var pause_location = await EvaluateAndCheck (eval_expr, debugger_test_loc, line, col, "MoveNext");

				var frame_locals = await GetProperties (pause_location ["callFrames"][frame_idx]["callFrameId"].Value<string> ());
				await CheckProps (frame_locals, new {
					call_other = TBool (false),
					local_i  = TNumber (5),
					sc = TSimpleClass (10, 45, "sc#Id", "Blue")
				}, "InspectValueTypeArrayLocalsInAsyncStaticStructMethod#locals");
			});
		}

		[Theory]
		[InlineData (false)]
		[InlineData (true)]
		public async Task InspectValueTypeArrayLocalsInAsyncInstanceStructMethod (bool use_cfo)
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
				ctx.UseCallFunctionOnBeforeGetProperties = use_cfo;
				var debugger_test_loc = "dotnet://debugger-test.dll/debugger-array-test.cs";

				await SetBreakpoint (debugger_test_loc, line, col);

				var eval_expr = "window.setTimeout(function() { invoke_static_method_async ("
							+ $"'{entry_method_name}', true"
						+ "); }, 1);";

				// BUG: Should be InspectValueTypeArrayLocalsInstanceAsync
				var pause_location = await EvaluateAndCheck (eval_expr, debugger_test_loc, line, col, "MoveNext");

				var frame_locals = await GetProperties (pause_location ["callFrames"][frame_idx]["callFrameId"].Value<string> ());
				await CheckProps (frame_locals, new {
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

		[Theory]
		[InlineData (123, 3, "MethodWithLocalsForToStringTest", false, false)]
		[InlineData (133, 3, "MethodWithArgumentsForToStringTest", true, false)]
		[InlineData (175, 3, "MethodWithArgumentsForToStringTestAsync", true, true)]
		[InlineData (165, 3, "MethodWithArgumentsForToStringTestAsync", false, true)]
		public async Task InspectLocalsForToStringDescriptions (int line, int col, string method_name, bool call_other, bool invoke_async)
		{
			var insp = new Inspector ();
			//Collect events
			var scripts = SubscribeToScripts(insp);
			string entry_method_name = $"[debugger-test] DebuggerTests.ValueTypesTest:MethodWithLocalsForToStringTest{(invoke_async ? "Async" : String.Empty)}";
			int frame_idx = 0;

			await Ready();
			await insp.Ready (async (cli, token) => {
				ctx = new DebugTestContext (cli, insp, token, scripts);
				var debugger_test_loc = "dotnet://debugger-test.dll/debugger-valuetypes-test.cs";

				await SetBreakpoint (debugger_test_loc, line, col);

				var eval_expr = "window.setTimeout(function() {"
									+ (invoke_async ? "invoke_static_method_async (" : "invoke_static_method (")
										+ $"'{entry_method_name}',"
										+ (call_other ? "true" : "false")
									+ "); }, 1);";
				Console.WriteLine ($"{eval_expr}");

				var pause_location = await EvaluateAndCheck (eval_expr, debugger_test_loc, line, col, invoke_async ? "MoveNext" : method_name);

				var dt0 = new DateTime (2020, 1, 2, 3, 4, 5);
				var dt1 = new DateTime (2010, 5, 4, 3, 2, 1);
				var ts = dt0 - dt1;
				var dto = new DateTimeOffset (dt0, new TimeSpan(4, 5, 0));

				var frame_locals = await GetProperties (pause_location ["callFrames"][frame_idx]["callFrameId"].Value<string> ());
				await CheckProps (frame_locals, new {
					call_other     = TBool      (call_other),
					dt0            = TValueType ("System.DateTime", dt0.ToString ()),
					dt1            = TValueType ("System.DateTime", dt1.ToString ()),
					dto            = TValueType ("System.DateTimeOffset", dto.ToString ()),
					ts             = TValueType ("System.TimeSpan", ts.ToString ()),
					dec            = TValueType ("System.Decimal", "123987123"),
					guid           = TValueType ("System.Guid", "3D36E07E-AC90-48C6-B7EC-A481E289D014"),
					dts            = TArray     ("System.DateTime[]", 2),
					obj            = TObject    ("DebuggerTests.ClassForToStringTests"),
					sst            = TObject    ("DebuggerTests.StructForToStringTests")
				}, "locals#0");

				var dts_0 = new DateTime (1983, 6, 7, 5, 6, 10);
				var dts_1 = new DateTime (1999, 10, 15, 1, 2, 3);
				var dts_elements = await GetObjectOnLocals (frame_locals, "dts");
				await CheckDateTime (dts_elements, "0", dts_0);
				await CheckDateTime (dts_elements, "1", dts_1);

				// TimeSpan
				await CompareObjectPropertiesFor (frame_locals, "ts",
						new {
							Days       = TNumber (3530),
							Minutes    = TNumber (2),
							Seconds    = TNumber (4),
						}, "ts_props", num_fields: 12);

				// DateTimeOffset
				await CompareObjectPropertiesFor (frame_locals, "dto",
						new {
							Day        = TNumber (2),
							Year       = TNumber (2020),
							DayOfWeek  = TEnum   ("System.DayOfWeek", "Thursday")
						}, "dto_props", num_fields: 22);

				var DT = new DateTime (2004, 10, 15, 1, 2, 3);
				var DTO = new DateTimeOffset (dt0, new TimeSpan(2, 14, 0));

				var obj_props = await CompareObjectPropertiesFor (frame_locals, "obj",
						new {
							DT         = TValueType ("System.DateTime", DT.ToString ()),
							DTO        = TValueType ("System.DateTimeOffset", DTO.ToString ()),
							TS         = TValueType ("System.TimeSpan", ts.ToString ()),
							Dec        = TValueType ("System.Decimal", "1239871"),
							Guid       = TValueType ("System.Guid", "3D36E07E-AC90-48C6-B7EC-A481E289D014")
						}, "obj_props");

				DTO = new DateTimeOffset (dt0, new TimeSpan (3, 15, 0));
				var sst_props = await CompareObjectPropertiesFor (frame_locals, "sst",
						new {
							DT         = TValueType ("System.DateTime", DT.ToString ()),
							DTO        = TValueType ("System.DateTimeOffset", DTO.ToString ()),
							TS         = TValueType ("System.TimeSpan", ts.ToString ()),
							Dec        = TValueType ("System.Decimal", "1239871"),
							Guid       = TValueType ("System.Guid", "3D36E07E-AC90-48C6-B7EC-A481E289D014")
						}, "sst_props");
			});
		}

		[Fact]
		public async Task InspectLocals () {
			var insp = new Inspector ();
			var scripts = SubscribeToScripts (insp);

			await Ready();
			await insp.Ready (async (cli, token) => {
				ctx = new DebugTestContext (cli, insp, token, scripts);

				var wait_res = await RunUntil ("locals_inner");
				var locals = await GetProperties (wait_res ["callFrames"][1]["callFrameId"].Value<string> ());
				});
		}

		[Theory]
		[InlineData (false)]
		[InlineData (true)]
		public async Task InspectLocalsWithPointers (bool use_cfo)
			=> await CheckInspectLocalsAtBreakpointSite (
				"dotnet://debugger-test.dll/debugger-test.cs", 294, 2,
				"PointersTest",
				"window.setTimeout(function() { invoke_static_method_async ('[debugger-test] Math:PointersTest'); })",
				use_cfo: use_cfo,
				wait_for_event_fn: async (pause_location) => {
					var locals = await GetProperties (pause_location ["callFrames"][0]["callFrameId"].Value<string>());

					var dt = new DateTime (5, 6, 7, 8, 9, 10);
					await CheckProps (locals, new {
						ivalue0        = TNumber    (5),
						ivalue1        = TNumber    (10),
						ip             = TPointer   ("int*"),
						ip_null        = TPointer   ("int*", is_null: true),
						ipp            = TPointer   ("int**"),

						ipa            = TArray     ("int*[]", 3),
						cvalue0        = TSymbol    ("113 'q'"),
						cp             = TPointer   ("char*"),
						dt             = TValueType ("System.DateTime", dt.ToString ()),
						vp             = TPointer   ("void*"),
						vp_null        = TPointer   ("void*", is_null: true),
						dtp            = TPointer   ("System.DateTime*"),
						dtp_null       = TPointer   ("System.DateTime*", is_null: true)
					}, "locals");

				});

		[Theory]
		[InlineData (false)]
		[InlineData (true)]
		public async Task InspectLocalsForStructInstanceMethod (bool use_cfo)
			=> await CheckInspectLocalsAtBreakpointSite (
				"dotnet://debugger-test.dll/debugger-array-test.cs", 236, 3,
				"GenericInstanceMethod<DebuggerTests.SimpleClass>",
				"window.setTimeout(function() { invoke_static_method_async ('[debugger-test] DebuggerTests.EntryClass:run'); })",
				use_cfo: use_cfo,
				wait_for_event_fn: async (pause_location) => {
					var frame_locals = await GetProperties (pause_location ["callFrames"][0]["callFrameId"].Value<string>());

					await CheckProps (frame_locals, new {
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

		//
		//TODO add tests covering basic stepping behavior as step in/out/over
	}

}
