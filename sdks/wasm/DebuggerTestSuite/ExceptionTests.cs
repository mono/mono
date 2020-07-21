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

				var pause_location = await EvaluateAndCheck (eval_expr, debugger_test_loc, line, col, "run");
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
			});
		}
	}
}
