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

namespace DebuggerTests
{
	public class SourceList : DebuggerTestBase {
		[Fact]
		public async Task CheckThatAllSourcesAreSent () {
			Dictionary<string, string> scripts = new Dictionary<string, string> ();
			var insp = new Inspector ();
			insp.On("Debugger.scriptParsed", async (args, c) => {
				var script_id = args? ["scriptId"]?.Value<string> ();
				var url = args["url"]?.Value<string> ();
				if (script_id.StartsWith("dotnet://"))
					scripts [script_id] = url;
				await Task.FromResult (0);
			});

			//all sources are sent before runtime ready is sent, nothing to check
			await insp.Ready ();
			Assert.Equal (3, scripts.Count);
			Assert.True (scripts.ContainsValue ("dotnet://debugger-test.dll/debugger-test.cs"));
			Assert.True (scripts.ContainsValue ("dotnet://debugger-test.dll/debugger-test2.cs"));
			Assert.True (scripts.ContainsValue ("dotnet://Simple.Dependency.dll/dependency.cs"));
		}
	
	}
}