using System;
using System.Linq;
using System.Threading.Tasks;

using System.Net.WebSockets;
using System.Threading;
using System.IO;
using System.Text;
using System.Collections.Generic;

using WsProxy;
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
			//System.Console.WriteLine("OnMessage " + method + args);
			switch (method) {
			case "Debugger.paused":
				NotifyOf (PAUSE, args);
				break;
			case "Mono.runtimeReady":
				NotifyOf (READY, args);
				break;
			case "Runtime.consoleAPICalled":
				Console.WriteLine ("CWL: {0}", args? ["args"]? [0]? ["value"]);
				break;
			}
			if (eventListeners.ContainsKey (method))
				await eventListeners[method](args, token);
		}

		public async Task Ready (Func<InspectorClient, CancellationToken, Task> cb = null) {
			using (var cts = new CancellationTokenSource ()) {
				cts.CancelAfter (60 * 1000); //tests have 1 minute to complete
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
						if (cb != null) {
							Console.WriteLine("await cb(client, token)");
							await cb(client, token);
						}
					}, cts.Token);
				}
			}
		}
	}

	public class DebuggerTestBase {
		static string FindTestPath () {
			//FIXME how would I locate it otherwise?
			var test_path = Environment.GetEnvironmentVariable ("TEST_SUITE_PATH");
			//Lets try to guest
			if (test_path != null && Directory.Exists (test_path))
				return test_path;

			var cwd = Environment.CurrentDirectory;
			Console.WriteLine ("guessing from {0}", cwd);
			//tests run from DebuggerTestSuite/bin/Debug/netcoreapp2.1
			var new_path = Path.Combine (cwd, "../../../../bin/debugger-test-suite");
			if (File.Exists (Path.Combine (new_path, "debugger-driver.html")))
				return new_path;

			throw new Exception ("Missing TEST_SUITE_PATH env var and could not guess path from CWD");
		}

		static string[] PROBE_LIST = new[] {
			"/Applications/Google Chrome Canary.app/Contents/MacOS/Google Chrome Canary",
			"/Applications/Google Chrome.app/Contents/MacOS/Google Chrome",
			"/usr/bin/chromium",
			"/usr/bin/chromium-browser",
		};
		static string chrome_path;


		static String FindChromePath ()
		{
			if (chrome_path != null)
				return chrome_path;
			foreach (var s in PROBE_LIST){
				if (File.Exists (s)) {
					chrome_path = s;
					Console.WriteLine($"Using chrome path: ${s}");
					return s;
				}
			}
			throw new Exception ("Could not find an installed Chrome to use");
		}

		public DebuggerTestBase () {
			WsProxy.TestHarnessProxy.Start (FindChromePath (), FindTestPath (), "debugger-driver.html");
		}
	}
} 
