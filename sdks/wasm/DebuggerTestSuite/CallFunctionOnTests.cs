using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Xunit;
using WebAssembly.Net.Debugging;

namespace DebuggerTests
{

	public class CallFunctionOnTests : DebuggerTestBase {

		// This tests `callFunctionOn` with a function that the vscode-js-debug extension uses
		// Using this here as a non-trivial test case
		[Theory]
		[InlineData ("big_array_js_test (10);", "/other.js", 5, 1, 10, false)]
		[InlineData ("big_array_js_test (0);", "/other.js", 5, 1, 0, true)]
		[InlineData ("invoke_static_method ('[debugger-test] Math:CallFunctionOnTest', 10);", "dotnet://debugger-test.dll/debugger-test.cs", 311, 2, 10, false)]
		[InlineData ("invoke_static_method ('[debugger-test] Math:CallFunctionOnTest', 0);", "dotnet://debugger-test.dll/debugger-test.cs", 311, 2, 0, true)]
		public async Task CheckVSCodeTestFunction1 (string eval_fn, string bp_loc, int line, int col, int len, bool roundtrip)
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
		public async Task CheckVSCodeTestFunction2 (string eval_fn, string bp_loc, int line, int col, int len)
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
		public async Task RunOnArrayReturnEmptyArray (string eval_fn, string bp_loc, int line, int col, bool roundtrip)
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
		public async Task RunOnArrayReturnArray (string eval_fn, string bp_loc, int line, int col, bool roundtrip)
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
		public async Task RunOnVTArray (bool roundtrip)
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
		public async Task RunOnCFOValueTypeResult (bool roundtrip)
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
		public async Task RunOnJSObject (bool roundtrip)
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
		public async Task RunOnArrayReturnObjectArrayByValue (string eval_fn, string bp_loc, int line, int col, bool roundtrip)
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
		public async Task RunOnArrayReturnArrayByValue (string eval_fn, string bp_loc, int line, int col, bool roundtrip)
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
		public async Task CheckErrorsWithSilent (bool? silent)
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
	}

}
