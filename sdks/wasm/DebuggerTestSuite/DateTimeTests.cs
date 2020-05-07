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
using System.Globalization;

namespace DebuggerTests
{
	public class DateTimeList : DebuggerTestBase {
		public DateTimeList() : base ("debugger-driver.html") {}

		[Fact]
		public async Task CheckThatAllLocaleSourcesAreSent () {
			var insp = new Inspector ();
			//Collect events
			var scripts = SubscribeToScripts(insp);

			await Ready();
			//all sources are sent before runtime ready is sent, nothing to check
			await insp.Ready ();
			Assert.Contains ("dotnet://debugger-test.dll/debugger-datetime-test.cs", scripts.Values);
		}

		[Theory]
		[InlineData ("en-US")]
		// [InlineData ("es-ES")]
		[InlineData ("de-DE")]
		// [InlineData ("ja-JP")]
		[InlineData ("ka-GE")]
		[InlineData ("hu-HU")]
		public async Task CheckDateTimeLocale (string locale) {
			var insp = new Inspector ();
			var scripts = SubscribeToScripts(insp);

			await Ready();
			await insp.Ready (async (cli, token) => {
				ctx = new DebugTestContext (cli, insp, token, scripts);
				var debugger_test_loc = "dotnet://debugger-test.dll/debugger-datetime-test.cs";

				await SetBreakpoint (debugger_test_loc, 20, 3);
				
				var pause_location = await EvaluateAndCheck (
					"window.setTimeout(function() { invoke_static_method ('[debugger-test] DebuggerTests.DateTimeTest:LocaleTest'," 
					+ $"'{locale}'); }}, 1);",
					debugger_test_loc, 20, 3, "LocaleTest",
					locals_fn: (locals) => {
						CultureInfo.CurrentCulture = new CultureInfo (locale, false);
						DateTime dt = new DateTime (2020, 1, 2, 3, 4, 5);
						string dt_str = dt.ToString();

						DateTimeFormatInfo dtfi = CultureInfo.GetCultureInfo(locale).DateTimeFormat;
						var fdtp = dtfi.FullDateTimePattern;
						var ldp = dtfi.LongDatePattern;
						var ltp = dtfi.LongTimePattern;
						var sdp = dtfi.ShortDatePattern;
						var stp = dtfi.ShortTimePattern;

						CheckString(locals, "fdtp", fdtp);
						CheckString(locals, "ldp", ldp);
						CheckString(locals, "ltp", ltp);
						CheckString(locals, "sdp", sdp);
						CheckString(locals, "stp", stp);
						CheckDateTime(locals, "dt", dt);
						CheckString(locals, "dt_str", dt_str);
					}
				);
				
				
			});
		}

	}
}