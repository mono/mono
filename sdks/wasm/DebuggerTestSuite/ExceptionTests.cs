using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Xunit;
using WebAssembly.Net.Debugging;

namespace DebuggerTests
{

	public class ExceptionTests : DebuggerTestBase {

		[Fact]
		public async Task ExceptionTestAll () {
			var insp = new Inspector ();
			//Collect events
			var scripts = SubscribeToScripts(insp);
			int line = 9;
			int col = 5;
			string entry_method_name = "[debugger-test] DebuggerTests.ExceptionTestsClass:TestExceptions";

			await Ready();
			await insp.Ready (async (cli, token) => {
				ctx = new DebugTestContext (cli, insp, token, scripts);
				var debugger_test_loc = "dotnet://debugger-test.dll/debugger-exception-test.cs";

				await SetPauseOnException ("all");

				var eval_expr = "window.setTimeout(function() { invoke_static_method ("
							+ $"'{entry_method_name}'"
						+ "); }, 1);";

				//stop in the caught exception
				var pause_location = await EvaluateAndCheck (eval_expr, debugger_test_loc, line, col, "run");
				Assert.Equal ("exception", pause_location["reason"]);
				await CheckValue (pause_location ["data"], JObject.FromObject (new {
					type      = "object",
					subtype   = "error",
					className = "DebuggerTests.CustomException",
					uncaught  = false
				}), "exception0.data");

				var exception_members = await GetProperties (pause_location["data"]["objectId"]?.Value<string> ());
				CheckString (exception_members, "message", "not implemented caught");

				//stop in the uncaught exception
				pause_location = await SendCommandAndCheck (null, "Debugger.resume", debugger_test_loc, 19, 4, "run"); 
				Assert.Equal ("exception", pause_location["reason"]);
				await CheckValue (pause_location ["data"], JObject.FromObject (new {
					type      = "object",
					subtype   = "error",
					className = "DebuggerTests.CustomException",
					uncaught  = true
				}), "exception1.data");

				exception_members = await GetProperties (pause_location["data"]["objectId"]?.Value<string> ());
				CheckString (exception_members, "message", "not implemented uncaught");
			});
		}

		[Fact]
		public async Task JSExceptionTestAll () {
			var insp = new Inspector ();
			//Collect events
			var scripts = SubscribeToScripts(insp);
			string entry_method_name = "[debugger-test] DebuggerTests.ExceptionTestsClass:TestExceptions";

			await Ready();
			await insp.Ready (async (cli, token) => {
				ctx = new DebugTestContext (cli, insp, token, scripts);

				await SetPauseOnException ("all");

				var eval_expr = "window.setTimeout(function () { exceptions_test (); }, 1)";
				var pause_location = await EvaluateAndCheck (eval_expr, null, 0, 0, "exception_caught_test", null, null);

				Assert.Equal ("exception", pause_location ["reason"]);
				await CheckValue (pause_location ["data"], JObject.FromObject (new {
					type      = "object",
					subtype   = "error",
					className = "TypeError",
					uncaught  = false
				}), "exception0.data");

				var exception_members = await GetProperties (pause_location ["data"]["objectId"]?.Value<string> ());
				CheckString (exception_members, "message", "exception caught");

				pause_location = await SendCommandAndCheck (null, "Debugger.resume", null, 0, 0, "exception_uncaught_test"); 

				Assert.Equal ("exception", pause_location ["reason"]);
				await CheckValue (pause_location ["data"], JObject.FromObject (new {
					type      = "object",
					subtype   = "error",
					className = "RangeError",
					uncaught  = true
				}), "exception1.data");

				exception_members = await GetProperties (pause_location ["data"]["objectId"]?.Value<string> ());
				CheckString (exception_members, "message", "exception uncaught");
			});
		}

		// FIXME? BUG? We seem to get the stack trace for Runtime.exceptionThrown at `call_method`,
		// but JS shows the original error type, and original trace
		[Fact]
		public async Task ExceptionTestNone () {
			var insp = new Inspector ();
			//Collect events
			var scripts = SubscribeToScripts(insp);
			string entry_method_name = "[debugger-test] DebuggerTests.ExceptionTestsClass:TestExceptions";

			await Ready();
			await insp.Ready (async (cli, token) => {
				ctx = new DebugTestContext (cli, insp, token, scripts);

				await SetPauseOnException("none");

				var eval_expr = "window.setTimeout(function() { invoke_static_method ("
							+ $"'{entry_method_name}'"
						+ "); }, 1);";

				try {
					await EvaluateAndCheck (eval_expr, null, 0, 0, "", null, null);
				} catch (ArgumentException ae) {
					var eo = JObject.Parse (ae.Message);

					// AssertEqual (line, eo ["exceptionDetails"]?["lineNumber"]?.Value<int> (), "lineNumber");
					AssertEqual ("Uncaught", eo ["exceptionDetails"]?["text"]?.Value<string> (), "text");

					await CheckValue (eo ["exceptionDetails"]?["exception"], JObject.FromObject (new {
						type      = "object",
						subtype   = "error",
						className = "Error" // BUG?: "DebuggerTests.CustomException"
					}), "exception");

					return;
				}

				Assert.True (false, "Expected to get an ArgumentException from the uncaught user exception");
			});
		}

		[Fact]
		public async Task JSExceptionTestNone () {
			var insp = new Inspector ();
			//Collect events
			var scripts = SubscribeToScripts(insp);
			string entry_method_name = "[debugger-test] DebuggerTests.ExceptionTestsClass:TestExceptions";

			await Ready();
			await insp.Ready (async (cli, token) => {
				ctx = new DebugTestContext (cli, insp, token, scripts);

				await SetPauseOnException("none");

				var eval_expr = "window.setTimeout(function () { exceptions_test (); }, 1)";

				int line = 41;
				string exceptionType = "RangeError";
				string exceptionMessage = "exception uncaught";
				try {
					await EvaluateAndCheck (eval_expr, null, 0, 0, "", null, null);
				} catch (ArgumentException ae) {
					Console.WriteLine ($"{ae}");
					var eo = JObject.Parse (ae.Message);

					AssertEqual (line, eo ["exceptionDetails"]?["lineNumber"]?.Value<int> (), "lineNumber");
					AssertEqual ("Uncaught", eo ["exceptionDetails"]?["text"]?.Value<string> (), "text");

					await CheckValue (eo ["exceptionDetails"]?["exception"], JObject.FromObject (new {
						type      = "object",
						subtype   = "error",
						className = "RangeError"
					}), "exception");

					return;
				}

				Assert.True (false, "Expected to get an ArgumentException from the uncaught user exception");
			});
		}

		[Theory]
		[InlineData ("function () { exceptions_test (); }", null, 0, 0, "exception_uncaught_test", "RangeError", "exception uncaught")]
		[InlineData ("function () { invoke_static_method ('[debugger-test] DebuggerTests.ExceptionTestsClass:TestExceptions'); }",
						"dotnet://debugger-test.dll/debugger-exception-test.cs", 19, 4, "run",
						"DebuggerTests.CustomException", "not implemented uncaught")]
		public async Task ExceptionTestUncaught (string eval_fn, string loc, int line, int col, string fn_name,
							string exception_type, string exception_message)
		{
			var insp = new Inspector ();
			//Collect events
			var scripts = SubscribeToScripts(insp);
			await Ready();
			await insp.Ready (async (cli, token) => {
				ctx = new DebugTestContext (cli, insp, token, scripts);

				await SetPauseOnException ("uncaught");

				var eval_expr = $"window.setTimeout({eval_fn}, 1);";
				var pause_location = await EvaluateAndCheck (eval_expr, loc, line, col, fn_name);

				Assert.Equal ("exception", pause_location ["reason"]);
				await CheckValue (pause_location ["data"], JObject.FromObject (new {
					type      = "object",
					subtype   = "error",
					className = exception_type,
					uncaught  = true
				}), "exception.data");

				var exception_members = await GetProperties (pause_location ["data"]["objectId"]?.Value<string> ());
				CheckString (exception_members, "message", exception_message);
			});
		}

	}
}
