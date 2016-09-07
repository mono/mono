using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

// Shut up CLS compliance warnings from Json.NET.
[assembly: CLSCompliant (true)]

namespace Mono.Profiling.Tests.Stress {

	// https://github.com/xamarin/benchmarker/blob/master/tools/libdbmodel/Benchmark.cs
	class Benchmark {
		public string Name { get; set; }
		public string TestDirectory { get; set; }
		public bool OnlyExplicit { get; set; }
		public string[] CommandLine { get; set; }
		public string[] ClientCommandLine { get; set; }
		public string[] AOTAssemblies { get; set; }

		public static Benchmark Load (string file)
		{
			return JsonConvert.DeserializeObject<Benchmark> (File.ReadAllText (file));
		}
	}

	static class Program {

		static int Main ()
		{
			var depDir = Path.Combine ("..", "external", "benchmarker");
			var benchDir = Path.Combine (depDir, "benchmarks");
			var testDir = Path.Combine (depDir, "tests");

			var benchmarks = Directory.EnumerateFiles (benchDir, "*.benchmark")
			                 .Select (Benchmark.Load)
			                 .Where (b => !b.OnlyExplicit && b.ClientCommandLine == null)
			                 .OrderBy (b => b.Name)
			                 .ToArray ();

			var monoPath = Path.GetFullPath (Path.Combine ("..", "..", "runtime", "mono-wrapper"));
			var classDir = Path.GetFullPath (Path.Combine ("..", "..", "mcs", "class", "lib", "net_4_x"));

			var rand = new Random ();
			var cpus = Environment.ProcessorCount;

			var successes = 0;
			var failures = 0;

			var sw = Stopwatch.StartNew ();

			for (var i = 0; i < benchmarks.Length; i++) {
				var bench = benchmarks [i];

				var sampleFreq = rand.Next (0, 1001);
				var sampleMode = rand.Next (0, 2) == 1 ? "real" : "process";
				var maxSamples = rand.Next (0, cpus * 2000 + 1);
				var heapShotFreq = rand.Next (0, 11);
				var maxFrames = rand.Next (0, 33);
				var allocMode = rand.Next (0, 2) == 1 ? "alloc" : "noalloc";

				var profOptions = $"sample=cycles/{sampleFreq},sampling-{sampleMode},maxsamples={maxSamples},heapshot={heapShotFreq}gc,maxframes={maxFrames},{allocMode},output=/dev/null";

				var info = new ProcessStartInfo {
					UseShellExecute = false,
					WorkingDirectory = Path.Combine (testDir, bench.TestDirectory),
					FileName = monoPath,
					Arguments = $"--debug --profile=log:{profOptions} " + string.Join (" ", bench.CommandLine),
				};

				info.EnvironmentVariables.Clear ();
				info.EnvironmentVariables.Add ("MONO_PATH", classDir);

				var progress = $"({i + 1}/{benchmarks.Length})";

				Console.ForegroundColor = ConsoleColor.Blue;
				Console.WriteLine ($"[{sw.Elapsed.ToString ("G")}] {progress} Running {bench.Name} with profiler options: {profOptions}");
				Console.ResetColor ();

				var sw2 = Stopwatch.StartNew ();

				using (var proc = Process.Start (info)) {
					proc.WaitForExit ();
					sw2.Stop ();

					Console.WriteLine ();

					if (proc.ExitCode != 0)
						failures++;
					else
						successes++;

					Console.ForegroundColor = proc.ExitCode != 0 ? ConsoleColor.Red : ConsoleColor.Green;
					Console.WriteLine ($"[{sw.Elapsed.ToString ("G")}] {progress} {bench.Name} took {sw2.Elapsed.ToString ("G")} and exited with code: {proc.ExitCode}");
					Console.ResetColor ();
				}
			}

			sw.Stop ();

			Console.ForegroundColor = failures != 0 ? ConsoleColor.Red : ConsoleColor.Green;
			Console.WriteLine ($"[{sw.Elapsed.ToString ("G")}] Finished with {successes}/{benchmarks.Length} passing tests");
			Console.ResetColor ();

			return failures;
		}
	}
}
