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

			await SetPauseOnException("all");

			var eval_expr = "window.setTimeout(function() { invoke_static_method ("
							+ $"'{entry_method_name}'"
						+ "); }, 1);";

				//stop in the caught exception
				var pause_location = await EvaluateAndCheck (eval_expr, debugger_test_loc, line, col, "run");
				Assert.Equal ("exception", pause_location["reason"]);
				var exception_members = await GetProperties (pause_location["data"]["objectId"]?.Value<string> ());
				CheckString (exception_members, "_message", "not implemented caught");
				//stop in the uncaught exception
				pause_location = await SendCommandAndCheck (null, "Debugger.resume", debugger_test_loc, 19, 4, "run"); 
				Assert.Equal ("exception", pause_location["reason"]);
				exception_members = await GetProperties (pause_location["data"]["objectId"]?.Value<string> ());
				CheckString (exception_members, "_message", "not implemented uncaught");
			});
		}

		[Fact]
		public async Task ExceptionTestUncaught () {
			var insp = new Inspector ();
			//Collect events
			var scripts = SubscribeToScripts(insp);
			int line = 19;
			int col = 4;
			string entry_method_name = "[debugger-test] DebuggerTests.ExceptionTestsClass:TestExceptions";

			await Ready();
			await insp.Ready (async (cli, token) => {
				ctx = new DebugTestContext (cli, insp, token, scripts);
				var debugger_test_loc = "dotnet://debugger-test.dll/debugger-exception-test.cs";

				await SetPauseOnException("uncaught");

				var eval_expr = "window.setTimeout(function() { invoke_static_method ("
							+ $"'{entry_method_name}'"
						+ "); }, 1);";

				var pause_location = await EvaluateAndCheck (eval_expr, debugger_test_loc, line, col, "run");
				Assert.Equal ("exception", pause_location["reason"]);
				var exception_members = await GetProperties (pause_location["data"]["objectId"]?.Value<string> ());
				CheckString (exception_members, "_message", "not implemented uncaught");
			});
		}

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

				var eval_expr = "window.setTimeout(function() { try {invoke_static_method ("
							+ $"'{entry_method_name}'"
						+ "); } catch (error) { debugger; }}, 1); ";

				var pause_location = await EvaluateAndCheck (eval_expr, null, 0, 0, "", null, null);
			});
		}
	}
}
