using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace crashbisector
{
	class BisectInfo {
		const int timeout = 60;

		public string MonoPath { get; set; }
		public string OptName { get; set; }
		public IEnumerable<string> Args { get; set; }
		Random rand;

		public BisectInfo () {
			rand = new Random ();
		}

		string Run (string bisectArg) {
			var args = Args;

			if (bisectArg == null) {
				args = new string[] { "-v" }.Concat (args);
			} else {
				args = new string[] { bisectArg }.Concat (args);
			}
			var startInfo = new ProcessStartInfo {
				FileName = MonoPath,
				Arguments = string.Join (" ", args),
				UseShellExecute = false,
				RedirectStandardOutput = true,
				RedirectStandardError = true
			};
			startInfo.EnvironmentVariables.Add ("MONO_DEBUG", "no-gdb-backtrace");

			using (var process = Process.Start (startInfo)) {
				var stdoutTask = Task.Factory.StartNew (() => new StreamReader (process.StandardOutput.BaseStream).ReadToEnd (), TaskCreationOptions.LongRunning);
				var stderrTask = Task.Factory.StartNew (() => new StreamReader (process.StandardError.BaseStream).ReadToEnd (), TaskCreationOptions.LongRunning);

				var success = process.WaitForExit (timeout < 0 ? -1 : (Math.Min (Int32.MaxValue / 1000, timeout) * 1000));
				if (!success || process.ExitCode != 0) {
					return null;
				}

				var stdout = stdoutTask.Result;

				return stdout;
			}
		}

		bool RunWithMethods (IEnumerable<string> methods) {
			var path = Path.GetTempFileName ();
			File.WriteAllLines (path, methods);
			var stdout = Run (String.Format ("--bisect={0}:{1}", OptName, path));
			File.Delete (path);
			return stdout != null;
		}

		IEnumerable<int> EliminationOrder (int numChunks) {
			var chunks = new int [numChunks];
			for (var i = 0; i < numChunks; ++i)
				chunks [i] = i;
			for (var i = 0; i < numChunks; ++i) {
				var j = rand.Next (i, numChunks);
				var tmp = chunks [i];
				chunks [i] = chunks [j];
				chunks [j] = tmp;
			}
			return chunks;
		}

		bool TryEliminate (IEnumerable<string> methods, int chunkSize) {
			var count = methods.Count ();
			if (chunkSize < 1 || chunkSize > count)
				throw new Exception ("I can't do math.");

			var numChunks = (count + chunkSize - 1) / chunkSize;
			foreach (var i in EliminationOrder (numChunks)) {
				var firstIndex = i * chunkSize;
				var lastPlusOneIndex = (i + 1) * chunkSize;
				var methodsLeft = methods.Take (firstIndex).Concat (methods.Skip (lastPlusOneIndex));

				if (chunkSize == 1)
					Console.WriteLine ("Running without method at position {0}", firstIndex);
				else
					Console.WriteLine ("Running without methods at positions {0} to {1}", firstIndex, lastPlusOneIndex - 1);
				var success = RunWithMethods (methodsLeft);
				Console.WriteLine ("Crashed: {0}", !success);

				if (!success) {
					Console.WriteLine ("Eliminating further from {0} methods.", methodsLeft.Count ());
					return EliminationStep (methodsLeft);
				}
			}

			return false;
		}

		bool EliminationStep (IEnumerable<string> methods) {
			var count = methods.Count ();

			if (count < 2) {
				Console.WriteLine ("Can't eliminate further.  Methods required to crash are:\n{0}",
					string.Join ("\n", methods));
				return true;
			}

			if (count >= 9) {
				var chunkSize = (int)Math.Floor (Math.Sqrt (count));
				Console.WriteLine ("Trying eliminating chunks of {0}.", chunkSize);
				if (TryEliminate (methods, chunkSize))
					return true;
				Console.WriteLine ("Chunks didn't succeed, eliminating individual methods.");
			}

			if (TryEliminate (methods, 1))
				return true;

			Console.WriteLine ("Couldn't eliminate any method.  Methods required to crash are:\n{0}",
				string.Join ("\n", methods));
			return true;
		}

		bool BisectStep (IEnumerable<string> methods) {
			var count = methods.Count ();

			if (count == 0) {
				Console.WriteLine ("Error: No methods left - what happened?");
				return false;
			}
			if (count == 1) {
				Console.WriteLine ("Found the offending method: {0}", methods.First ());
				return true;
			}

			var half = count / 2;
			var firstHalf = methods.Take (half);
			var secondHalf = methods.Skip (half);
			Console.WriteLine ("Splitting into two halves: {0} and {1} methods.", firstHalf.Count (), secondHalf.Count ());

			Console.WriteLine ("Running first half.");
			var firstSuccess = RunWithMethods (firstHalf);
			Console.WriteLine ("Crashed: {0}", !firstSuccess);

			if (!firstSuccess) {
				Console.WriteLine ("Continuing with first half.");
				return BisectStep (firstHalf);
			}

			Console.WriteLine ("Running second half.");
			var secondSuccess = RunWithMethods (secondHalf);
			Console.WriteLine ("Crashed: {0}", !secondSuccess);

			if (!secondSuccess) {
				Console.WriteLine ("Continuing with second half.");
				return BisectStep (secondHalf);
			}

			Console.WriteLine ("Error: Both halves succeeded, can't bisect.  Trying elimination.");
			return EliminationStep (methods);
		}

		public bool Bisect () {
			Console.WriteLine ("Running to gather methods.");
			var stdout = Run (null);
			if (stdout == null) {
				Console.Error.WriteLine ("Error: Failed to execute without optimization.");
				Environment.Exit (1);
			}

			var regex = new Regex ("converting[^\n]* method ([^\n]+)\n");
			var matches = regex.Matches (stdout);
			var methods = new List<string> ();
			foreach (Match match in matches) {
				var method = match.Groups [1].Value;
				methods.Add (method);
			}

			Console.WriteLine ("Bisecting {0} methods.", methods.Count);

			Console.WriteLine ("Running with all methods, just to make sure.");
			var success = RunWithMethods (methods);
			if (success) {
				Console.WriteLine ("Error: Ran successfully with all methods optimized.  Nothing to bisect.");
				return false;
			}
			Console.WriteLine ("Crashed.  Bisecting.");
			return BisectStep (methods);
		}
	}

	class MainClass
	{
		static void UsageAndExit (int exitCode) {
			Console.Error.WriteLine ("Usage: crash-bisector.exe --mono MONO-EXECUTABLE --opt OPTION-NAME -- MONO-ARG ...");
			Environment.Exit (exitCode);
		}
			
		public static void Main (string[] args)
		{
			string monoPath = null;
			string optName = null;

			var argIndex = 0;
			while (argIndex < args.Length) {
				if (args [argIndex] == "--mono") {
					monoPath = args [argIndex + 1];
					argIndex += 2;
				} else if (args [argIndex] == "--opt") {
					optName = args [argIndex + 1];
					argIndex += 2;
				} else if (args [argIndex] == "--help") {
					UsageAndExit (0);
				} else if (args [argIndex] == "--") {
					argIndex += 1;
					break;
				} else {
					UsageAndExit (1);
				}
			}

			if (monoPath == null || optName == null || argIndex == args.Length)
				UsageAndExit (1);

			var bisectInfo = new BisectInfo {
				MonoPath = monoPath,
				OptName = optName,
				Args = args.Skip (argIndex)
			};
			var success = bisectInfo.Bisect ();
			Environment.Exit (success ? 0 : 1);
		}
	}
}
