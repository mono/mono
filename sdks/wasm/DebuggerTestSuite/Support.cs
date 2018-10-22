using System;
using System.Linq;
using System.Threading.Tasks;

using System.Net.WebSockets;
using System.Threading;
using System.IO;
using System.Text;
using System.Collections.Generic;

using Mono.WebAssembly;
using Newtonsoft.Json.Linq;
using Xunit;

namespace DebuggerTests
{
	class Inspector
	{
		// InspectorClient client;
		Dictionary<string, TaskCompletionSource<JObject>> notifications = new Dictionary<string, TaskCompletionSource<JObject>> ();
		Dictionary<string, Func<JObject, CancellationToken, Task>> eventListeners = new Dictionary<string, Func<JObject, CancellationToken, Task>> ();

		public const string PAUSE = "pause";
		public const string READY = "ready";

		public Task<JObject> WaitFor(string what) {
			if (notifications.ContainsKey (what))
				throw new Exception ($"Invalid internal state, waiting for {what} while another wait is already setup");
			var n = new TaskCompletionSource<JObject> ();
			notifications [what] = n;
			return n.Task;
		}

		void NotifyOf (string what, JObject args) {
			if (!notifications.ContainsKey (what))
				throw new Exception ($"Invalid internal state, notifying of {what}, but nobody waiting");
			notifications [what].SetResult (args);
			notifications.Remove (what);
		}

		public void On(string evtName, Func<JObject, CancellationToken, Task> cb) {
			eventListeners[evtName] = cb;
		}

		async Task OnMessage(string method, JObject args, CancellationToken token)
		{
			switch (method) {
			case "Debugger.paused":
				NotifyOf (PAUSE, args);
				break;
			case "Mono.runtimeReady":
				NotifyOf (READY, args);
				break;
			}
			if (eventListeners.ContainsKey (method))
				await eventListeners[method](args, token);
		}

		public async Task Ready (Func<InspectorClient, CancellationToken, Task> cb = null) {
			using (var cts = new CancellationTokenSource ()) {
				cts.CancelAfter (10 * 1000); //tests have 10 seconds to complete!
				var uri = new Uri ("ws://localhost:9300/launch-chrome-and-connect");
				using (var client = new InspectorClient ()) {
					await client.Connect (uri, OnMessage, async token => {
						Task[] init_cmds = new Task [] {
							client.SendCommand ("Profiler.enable", null, token),
							client.SendCommand ("Runtime.enable", null, token),
							client.SendCommand ("Debugger.enable", null, token),
							client.SendCommand ("Runtime.runIfWaitingForDebugger", null, token),
							WaitFor (READY),
						};
						// await Task.WhenAll (init_cmds);
						Console.WriteLine ("waiting for the runtime to be ready");
						await init_cmds [4];
						Console.WriteLine ("runtime ready, TEST TIME");
						if (cb != null)
							await cb (client, token);
					}, cts.Token);
				}
			}
		}
	}

	public class DebuggerTestBase {
		static string GuessTestPath () {
			var cwd = Environment.CurrentDirectory;
			Console.WriteLine ("guessing from {0}", cwd);
			//tests run from DebuggerTestSuite/bin/Debug/netcoreapp2.1
			var new_path = Path.Combine (cwd, "../../../../bin/debugger-test-suite");
			if (File.Exists (Path.Combine (new_path, "debugger-driver.html")))
				return new_path;
			return null;
		}

		public DebuggerTestBase () {
			//FIXME probe for chromium on linux
			string chrome_path = null;
			if (File.Exists ("/Applications/Google Chrome Canary.app/Contents/MacOS/Google Chrome Canary"))
				chrome_path = "/Applications/Google Chrome Canary.app/Contents/MacOS/Google Chrome Canary";
			else if (File.Exists ("/Applications/Google Chrome.app/Contents/MacOS/Google Chrome"))
				chrome_path = "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome";
			if (chrome_path == null)
				throw new Exception ("Could not find an installed Chrome to use");

			//FIXME how would I locate it otherwise?
			var test_path = Environment.GetEnvironmentVariable ("TEST_SUITE_PATH");
			//Lets try to guest
			if (test_path == null || !Directory.Exists (test_path))
				test_path = GuessTestPath ();

			if (test_path == null || !Directory.Exists (test_path)) {
				throw new Exception ("Missing or Invalid TEST_SUITE_PATH env var");
			}

			WsProxy.TestHarnessProxy.Start (chrome_path, test_path, "debugger-driver.html");
		}
	}
}