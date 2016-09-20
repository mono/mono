using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using Mono.Unix.Native;
using Newtonsoft.Json;

// Shut up CLS compliance warnings from Json.NET.
[assembly: CLSCompliant (true)]

namespace Mono.Profiling.Tests.Stress {

	// https://github.com/xamarin/benchmarker/blob/master/tools/libdbmodel/Benchmark.cs
	sealed class Benchmark {

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

	sealed class TestResult {

		public Benchmark Benchmark { get; set; }
		public ProcessStartInfo StartInfo { get; set; }
		public Stopwatch Stopwatch { get; set; } = new Stopwatch ();
		public int? ExitCode { get; set; }
		public StringBuilder StandardOutput { get; set; } = new StringBuilder ();
		public StringBuilder StandardError { get; set; } = new StringBuilder ();
	}

	static class Program {

		static readonly TimeSpan _timeout = TimeSpan.FromHours (6);

		static string FilterInvalidXmlChars (string text) {
			return Regex.Replace (text, @"[^\x09\x0A\x0D\x20-\uD7FF\uE000-\uFFFD\u10000-\u10FFFF]", string.Empty);
		}

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

			var results = new List<TestResult> (benchmarks.Length);

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
					RedirectStandardOutput = true,
					RedirectStandardError = true,
				};

				info.EnvironmentVariables.Clear ();
				info.EnvironmentVariables.Add ("MONO_PATH", classDir);

				var progress = $"({i + 1}/{benchmarks.Length})";

				Console.ForegroundColor = ConsoleColor.Blue;
				Console.WriteLine ($"[{sw.Elapsed.ToString ("G")}] {progress} Running {bench.Name} with profiler options: {profOptions}");
				Console.ResetColor ();

				var result = new TestResult {
					Benchmark = bench,
					StartInfo = info,
				};

				using (var proc = new Process ()) {
					proc.StartInfo = info;

					proc.OutputDataReceived += (sender, args) => {
						if (args.Data != null)
							result.StandardOutput.AppendLine (args.Data);
					};

					proc.ErrorDataReceived += (sender, args) => {
						if (args.Data != null)
							result.StandardError.AppendLine (args.Data);
					};

					result.Stopwatch.Start ();

					proc.Start ();

					proc.BeginOutputReadLine ();
					proc.BeginErrorReadLine ();

					if (!proc.WaitForExit ((int) _timeout.TotalMilliseconds)) {
						// Force a thread dump.
						Syscall.kill (proc.Id, Signum.SIGQUIT);
						Thread.Sleep (1000);

						try {
							proc.Kill ();
						} catch (Exception) {
						}
					} else
						result.ExitCode = proc.ExitCode;

					result.Stopwatch.Stop ();
				}

				var resultStr = result.ExitCode == null ? "timed out" : $"exited with code: {result.ExitCode}";

				Console.ForegroundColor = result.ExitCode != 0 ? ConsoleColor.Red : ConsoleColor.Green;
				Console.WriteLine ($"[{sw.Elapsed.ToString ("G")}] {progress} {bench.Name} took {result.Stopwatch.Elapsed.ToString ("G")} and {resultStr}");
				Console.ResetColor ();

				if (result.ExitCode != 0) {
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine ("===== stdout =====");
					Console.ResetColor ();

					Console.WriteLine (result.StandardOutput.ToString ());

					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine ("===== stderr =====");
					Console.ResetColor ();

					Console.WriteLine (result.StandardError.ToString ());
				}

				results.Add (result);
			}

			sw.Stop ();

			var successes = results.Count (r => r.ExitCode == 0);
			var failures = results.Count (r => r.ExitCode != null && r.ExitCode != 0);
			var timeouts = results.Count (r => r.ExitCode == null);

			var settings = new XmlWriterSettings {
				NewLineOnAttributes = true,
				Indent = true,
			};

			using (var writer = XmlWriter.Create ("TestResult-profiler-stress.xml", settings)) {
				writer.WriteStartDocument ();
				writer.WriteComment ("This file represents the results of running a test suite");

				writer.WriteStartElement ("test-results");
				writer.WriteAttributeString ("name", "profiler-stress-tests.dummy");
				writer.WriteAttributeString ("total", results.Count.ToString ());
				writer.WriteAttributeString ("failures", failures.ToString ());
				writer.WriteAttributeString ("not-run", "0");
				writer.WriteAttributeString ("date", DateTime.Now.ToString ("yyyy-MM-dd"));
				writer.WriteAttributeString ("time", DateTime.Now.ToString ("HH:mm:ss"));

				writer.WriteStartElement ("environment");
				writer.WriteAttributeString ("nunit-version", "2.4.8.0");
				writer.WriteAttributeString ("clr-version", Environment.Version.ToString ());
				writer.WriteAttributeString ("os-version", Environment.OSVersion.ToString ());
				writer.WriteAttributeString ("platform", Environment.OSVersion.Platform.ToString ());
				writer.WriteAttributeString ("cwd", Environment.CurrentDirectory);
				writer.WriteAttributeString ("machine-name", Environment.MachineName);
				writer.WriteAttributeString ("user", Environment.UserName);
				writer.WriteAttributeString ("user-domain", Environment.UserDomainName);
				writer.WriteEndElement ();

				writer.WriteStartElement ("culture-info");
				writer.WriteAttributeString ("current-culture", CultureInfo.CurrentCulture.Name);
				writer.WriteAttributeString ("current-uiculture", CultureInfo.CurrentUICulture.Name);
				writer.WriteEndElement ();

				writer.WriteStartElement ("test-suite");
				writer.WriteAttributeString ("name", "profiler-stress-tests.dummy");
				writer.WriteAttributeString ("success", (failures + timeouts == 0).ToString ());
				writer.WriteAttributeString ("time", ((int) sw.Elapsed.TotalSeconds).ToString ());
				writer.WriteAttributeString ("asserts", (failures + timeouts).ToString ());
				writer.WriteStartElement ("results");

				writer.WriteStartElement ("test-suite");
				writer.WriteAttributeString ("name", "MonoTests");
				writer.WriteAttributeString ("success", (failures + timeouts == 0).ToString ());
				writer.WriteAttributeString ("time", ((int) sw.Elapsed.TotalSeconds).ToString ());
				writer.WriteAttributeString ("asserts", (failures + timeouts).ToString ());
				writer.WriteStartElement ("results");

				writer.WriteStartElement ("test-suite");
				writer.WriteAttributeString ("name", "profiler-stress");
				writer.WriteAttributeString ("success", (failures + timeouts == 0).ToString ());
				writer.WriteAttributeString ("time", ((int) sw.Elapsed.TotalSeconds).ToString ());
				writer.WriteAttributeString ("asserts", (failures + timeouts).ToString ());
				writer.WriteStartElement ("results");

				foreach (var result in results) {
					var timeoutStr = result.ExitCode == null ? "_timeout" : string.Empty;

					writer.WriteStartElement ("test-case");
					writer.WriteAttributeString ("name", $"MonoTests.profiler-stress.{result.Benchmark.Name}{timeoutStr}");
					writer.WriteAttributeString ("executed", "True");
					writer.WriteAttributeString ("success", (result.ExitCode == 0).ToString ());
					writer.WriteAttributeString ("time", ((int) result.Stopwatch.Elapsed.TotalSeconds).ToString ());
					writer.WriteAttributeString ("asserts", result.ExitCode == 0 ? "0" : "1");

					if (result.ExitCode != 0) {
						writer.WriteStartElement ("failure");

						writer.WriteStartElement ("message");
						writer.WriteCData (FilterInvalidXmlChars (result.StandardOutput.ToString ()));
						writer.WriteEndElement ();

						writer.WriteStartElement ("stack-trace");
						writer.WriteCData (FilterInvalidXmlChars (result.StandardError.ToString ()));
						writer.WriteEndElement ();

						writer.WriteEndElement ();
					}

					writer.WriteEndElement ();
				}

				writer.WriteEndElement ();
				writer.WriteEndElement ();

				writer.WriteEndElement ();
				writer.WriteEndElement ();

				writer.WriteEndElement ();
				writer.WriteEndElement ();

				writer.WriteEndElement ();

				writer.WriteEndDocument ();
			}

			var failureStr = failures + timeouts != 0 ? $" ({failures} failures, {timeouts} timeouts)" : string.Empty;

			Console.ForegroundColor = failures + timeouts != 0 ? ConsoleColor.Red : ConsoleColor.Green;
			Console.WriteLine ($"[{sw.Elapsed.ToString ("G")}] Finished with {successes}/{results.Count} passing tests{failureStr}");
			Console.ResetColor ();

			return failures + timeouts;
		}
	}
}
