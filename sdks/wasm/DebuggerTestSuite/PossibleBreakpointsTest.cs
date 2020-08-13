using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Xunit;

namespace DebuggerTests
{
	public class PossibleBreakpointsTest : DebuggerTestBase
	{
		const string JsTestFile = "/possible-breakpoints.js";
		const int JsStartLine = 2;

		const string ManagedTestFile = "dotnet://debugger-test.dll/possible-breakpoints.cs";
		const int ManagedStartLine = 5;

		[Fact]
		public Task TestGetPossibleBreakpointsJsNoEnd () => TestGetPossibleBreakpoints (JsTestFile, JsStartLine, null, new ExpectedLocation[] {
				new ExpectedLocation (JsStartLine, 1),
				new ExpectedLocation (JsStartLine, 9, "call"),
				new ExpectedLocation (JsStartLine + 1, 1),
				new ExpectedLocation (JsStartLine + 1, 9, "call"),
				new ExpectedLocation (JsStartLine + 2, 0, "return"),
				new ExpectedLocation (JsStartLine + 5, 1),
				new ExpectedLocation (JsStartLine + 5, 9, "call"),
				new ExpectedLocation (JsStartLine + 6, 0, "return")
			});

		[Fact]
		public Task TestGetPossibleBreakpointsJsWithEnd () => TestGetPossibleBreakpoints (JsTestFile, JsStartLine, JsStartLine + 5, new ExpectedLocation[] {
				new ExpectedLocation (JsStartLine, 1),
				new ExpectedLocation (JsStartLine, 9, "call"),
				new ExpectedLocation (JsStartLine + 1, 1),
				new ExpectedLocation (JsStartLine + 1, 9, "call"),
				new ExpectedLocation (JsStartLine + 2, 0, "return")
			});

		[Fact]
		public Task TestGetPossibleBreakpointsJsEndBeforeStart () => TestGetPossibleBreakpoints (JsTestFile, JsStartLine + 5, JsStartLine, new ExpectedLocation[0]);

		[Fact]
		public Task TestGetPossibleBreakpointsJsHighStart () => TestGetPossibleBreakpoints (JsTestFile, 9999, null, new ExpectedLocation[0]);

		[Fact]
		public Task TestGetPossibleBreakpointsJsNegativeStart () => TestGetPossibleBreakpoints (JsTestFile, -1, null, new ExpectedLocation[0]);

		[Fact]
		public Task TestGetPossibleBreakpointsJsNegativeEnd () => TestGetPossibleBreakpoints (JsTestFile, 9999, -1, new ExpectedLocation[0]);

		[Fact]
		public Task TestGetPossibleBreakpointsManagedNoEnd () => TestGetPossibleBreakpoints (ManagedTestFile, ManagedStartLine, null, new ExpectedLocation[] {
				new ExpectedLocation (ManagedStartLine, 2),
				new ExpectedLocation (ManagedStartLine + 1, 2),
				new ExpectedLocation (ManagedStartLine + 2, 1),
				new ExpectedLocation (ManagedStartLine + 4, 47),
				new ExpectedLocation (ManagedStartLine + 5, 2),
				new ExpectedLocation (ManagedStartLine + 6, 1)
			});

		[Fact]
		public Task TestGetPossibleBreakpointsManagedWithEnd () => TestGetPossibleBreakpoints (ManagedTestFile, ManagedStartLine, ManagedStartLine + 3, new ExpectedLocation[] {
				new ExpectedLocation (ManagedStartLine, 2),
				new ExpectedLocation (ManagedStartLine + 1, 2),
				new ExpectedLocation (ManagedStartLine + 2, 1)
			});

		[Fact]
		public Task TestGetPossibleBreakpointsManagedEndBeforeStart () => TestGetPossibleBreakpoints (ManagedTestFile, ManagedStartLine + 3, ManagedStartLine, new ExpectedLocation[0]);

		[Fact]
		public Task TestGetPossibleBreakpointsManagedHighStart () => TestGetPossibleBreakpoints (ManagedTestFile, 9999, null, new ExpectedLocation[0]);

		[Fact]
		public Task TestGetPossibleBreakpointsManagedNegativeStart () => TestGetPossibleBreakpoints (ManagedTestFile, -1, null, new ExpectedLocation[0]);

		[Fact]
		public Task TestGetPossibleBreakpointsManagedNegativeEnd () => TestGetPossibleBreakpoints (ManagedTestFile, 9999, -1, new ExpectedLocation[0]);

		class ExpectedLocation
		{
			public int Line { get; private set; }
			public int? Column { get; private set; }
			public string Type { get; private set; }

			public ExpectedLocation (int line, int? column = null, string type = null)
			{
				Line = line;
				Column = column;
				Type = type;
			}
		}

		async Task TestGetPossibleBreakpoints (string file, int startLine, int? endLine, ExpectedLocation[] expectedLocations)
		{
			var insp = new Inspector ();
			var scripts = SubscribeToScripts (insp);

			await Ready ();
			await insp.Ready (async (cli, token) =>
			{
				ctx = new DebugTestContext (cli, insp, token, scripts);

				var url = dicFileToUrl[file];
				var scriptId = dicUrlToId[url];

				var req = JObject.FromObject (new
				{
					start = new
					{
						scriptId, lineNumber = startLine
					}
				});

				if (endLine != null)
					req["end"] = JObject.FromObject (new { scriptId, lineNumber = endLine.Value });

				var res = await ctx.cli.SendCommand ("Debugger.getPossibleBreakpoints", req, ctx.token);
				if (startLine < 0 || (endLine ?? 0) < 0) {
					Assert.True (res.IsErr);
					Assert.False (res.IsOk);
					Assert.Equal (-32000, res.Error["code"]?.Value<int> ());
					if (startLine < 0)
						Assert.Equal ("start.lineNumber and start.columnNumber should be >= 0", res.Error["message"]);
					else
						Assert.Equal ("end.lineNumber and end.columnNumber should be >= 0", res.Error["message"]);
					return;
				}

				Assert.True (res.IsOk);

				var locations = res.Value["locations"].ToArray ();
				Assert.Equal (expectedLocations.Length, locations.Length);

				for (int i = 0; i < expectedLocations.Length; i++)
					AssertLocation (expectedLocations[i], locations[i]);

				void AssertLocation (ExpectedLocation expected, JToken location)
				{
					Assert.Equal (scriptId, location["scriptId"]);
					Assert.Equal (expected.Line, location["lineNumber"]?.Value<int> ());
					if (expected.Column != null)
						Assert.Equal (expected.Column.Value, location["columnNumber"]?.Value<int> ());
					else
						Assert.Null (location["columnNumber"]);
					if (expected.Type != null)
						Assert.Equal (expected.Type, location["type"]);
					else
						Assert.Null (location["type"]);
				}
			});
		}
	}
}
