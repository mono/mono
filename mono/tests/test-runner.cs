//
// test-runner.cs
//
// Author:
//   Zoltan Varga (vargaz@gmail.com)
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
//
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//
using System;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using System.Text;
using System.Text.RegularExpressions;

#if !FULL_AOT_DESKTOP && !MOBILE
using Mono.Unix.Native;
#endif

//
// This is a simple test runner with support for parallel execution
//

public class TestRunner
{
	const string TEST_TIME_FORMAT = "mm\\:ss\\.fff";
	const string ENV_TIMEOUT = "TEST_DRIVER_TIMEOUT_SEC";
	const string MONO_PATH = "MONO_PATH";
	const string MONO_GAC_PREFIX = "MONO_GAC_PREFIX";

	class ProcessData {
		public string test;
		public StringBuilder stdout, stderr;
		public object stdoutLock = new object (), stderrLock = new object ();
		public string stdoutName, stderrName;
		public TimeSpan duration;
	}

	class TestInfo {
		public string test, opt_set;
	}

	public static int Main (String[] args) {
		// Defaults
		int concurrency = 1;
		int timeout = 2 * 60; // in seconds
		int expectedExitCode = 0;
		bool verbose = false;
		string testsuiteName = null;
		string inputFile = null;

		string disabled_tests = null;
		string runtime = "mono";
		string config = null;
		string mono_path = null;
		string runtime_args = null;
		string mono_gac_prefix = null;
		var opt_sets = new List<string> ();

		// Process options
		int i = 0;
		while (i < args.Length) {
			if (args [i].StartsWith ("-")) {
				if (args [i] == "-j") {
					if (i + 1 >= args.Length) {
						Console.WriteLine ("Missing argument to -j command line option.");
						return 1;
					}
					if (args [i + 1] == "a")
						concurrency = Environment.ProcessorCount;
					else
						concurrency = Int32.Parse (args [i + 1]);
					i += 2;
				} else if (args [i] == "--timeout") {
					if (i + 1 >= args.Length) {
						Console.WriteLine ("Missing argument to --timeout command line option.");
						return 1;
					}
					timeout = Int32.Parse (args [i + 1]);
					i += 2;
				} else if (args [i] == "--disabled") {
					if (i + 1 >= args.Length) {
						Console.WriteLine ("Missing argument to --disabled command line option.");
						return 1;
					}
					disabled_tests = args [i + 1];
					i += 2;
				} else if (args [i] == "--runtime") {
					if (i + 1 >= args.Length) {
						Console.WriteLine ("Missing argument to --runtime command line option.");
						return 1;
					}
					runtime = args [i + 1];
					i += 2;
				} else if (args [i] == "--runtime-args") {
					if (i + 1 >= args.Length) {
						Console.WriteLine ("Missing argument to --runtime-args command line option.");
						return 1;
					}
					runtime_args = (runtime_args ?? "") + " " + args [i + 1];
					i += 2;
				} else if (args [i] == "--config") {
					if (i + 1 >= args.Length) {
						Console.WriteLine ("Missing argument to --config command line option.");
						return 1;
					}
					config = args [i + 1];
					i += 2;
				} else if (args [i] == "--opt-sets") {
					if (i + 1 >= args.Length) {
						Console.WriteLine ("Missing argument to --opt-sets command line option.");
						return 1;
					}
					foreach (var s in args [i + 1].Split ())
						opt_sets.Add (s);
					i += 2;
				} else if (args [i] == "--expected-exit-code") {
					if (i + 1 >= args.Length) {
						Console.WriteLine ("Missing argument to --expected-exit-code command line option.");
						return 1;
					}
					expectedExitCode = Int32.Parse (args [i + 1]);
					i += 2;
				} else if (args [i] == "--testsuite-name") {
					if (i + 1 >= args.Length) {
						Console.WriteLine ("Missing argument to --testsuite-name command line option.");
						return 1;
					}
					testsuiteName = args [i + 1];
					i += 2;
				} else if (args [i] == "--input-file") {
					if (i + 1 >= args.Length) {
						Console.WriteLine ("Missing argument to --input-file command line option.");
						return 1;
					}
					inputFile = args [i + 1];
					i += 2;
				} else if (args [i] == "--mono-path") {
					if (i + 1 >= args.Length) {
						Console.WriteLine ("Missing argument to --mono-path command line option.");
						return 1;
					}
					mono_path = args [i + 1].Substring(0, args [i + 1].Length);

					i += 2;
				} else if (args [i] == "--mono-gac-prefix") {
					if (i + 1 >= args.Length) {
						Console.WriteLine ("Missing argument to --mono-gac-prefix command line option.");
						return 1;
					}
					mono_gac_prefix = args[i + 1];
					i += 2;
				} else if (args [i] == "--verbose") {
					verbose = true;
					i ++;
				} else {
					Console.WriteLine ("Unknown command line option: '" + args [i] + "'.");
					return 1;
				}
			} else {
				break;
			}
		}

		if (String.IsNullOrEmpty (testsuiteName)) {
			Console.WriteLine ("Missing the required --testsuite-name command line option.");
			return 1;
		}

		var disabled = new Dictionary <string, string> ();

		if (disabled_tests != null) {
			foreach (string test in disabled_tests.Split ())
				disabled [test] = test;
		}

		var tests = new List<string> ();

		if (!String.IsNullOrEmpty (inputFile)) {
			tests.AddRange (File.ReadAllLines (inputFile));
		} else {
			// The remaining arguments are the tests
			for (int j = i; j < args.Length; ++j)
				if (!disabled.ContainsKey (args [j]))
					tests.Add (args [j]);
		}

		var passed = new List<ProcessData> ();
		var failed = new List<ProcessData> ();
		var timedout = new List<ProcessData> ();

		object monitor = new object ();

		Console.WriteLine ("Running tests: ");

		var test_info = new Queue<TestInfo> ();
		if (opt_sets.Count == 0) {
			foreach (string s in tests)
				test_info.Enqueue (new TestInfo { test = s });
		} else {
			foreach (string opt in opt_sets) {
				foreach (string s in tests)
					test_info.Enqueue (new TestInfo { test = s, opt_set = opt });
			}
		}

		/* compute the max length of test names, to have an optimal output width */
		int output_width = -1;
		foreach (TestInfo ti in test_info) {
			if (ti.test.Length > output_width)
				output_width = Math.Min (120, ti.test.Length);
		}

		List<Thread> threads = new List<Thread> (concurrency);

		DateTime test_start_time = DateTime.UtcNow;

		for (int j = 0; j < concurrency; ++j) {
			Thread thread = new Thread (() => {
				while (true) {
					TestInfo ti;

					lock (monitor) {
						if (test_info.Count == 0)
							break;
						ti = test_info.Dequeue ();
					}

					var output = new StringWriter ();

					string test = ti.test;
					string opt_set = ti.opt_set;

					if (verbose) {
						output.Write (String.Format ("{{0,-{0}}} ", output_width), test);
					} else {
						Console.Write (".");
					}

					/* Spawn a new process */

					string process_args = "";

					if (opt_set != null)
						process_args += " -O=" + opt_set;
					if (runtime_args != null)
						process_args += " " + runtime_args;

					process_args += " " + test;

					ProcessStartInfo info = new ProcessStartInfo (runtime, process_args);
					info.UseShellExecute = false;
					info.RedirectStandardOutput = true;
					info.RedirectStandardError = true;
					info.EnvironmentVariables[ENV_TIMEOUT] = timeout.ToString();
					if (config != null)
						info.EnvironmentVariables["MONO_CONFIG"] = config;
					if (mono_path != null)
						info.EnvironmentVariables[MONO_PATH] = mono_path;
					if (mono_gac_prefix != null)
						info.EnvironmentVariables[MONO_GAC_PREFIX] = mono_gac_prefix;
					Process p = new Process ();
					p.StartInfo = info;

					ProcessData data = new ProcessData ();
					data.test = test;

					string log_prefix = "";
					if (opt_set != null)
						log_prefix = "." + opt_set.Replace ("-", "no").Replace (",", "_");

					data.stdoutName = test + log_prefix + ".stdout";
					data.stdout = new StringBuilder ();

					data.stderrName = test + log_prefix + ".stderr";
					data.stderr = new StringBuilder ();

					p.OutputDataReceived += delegate (object sender, DataReceivedEventArgs e) {
						lock (data.stdoutLock) {
							if (e.Data != null)
								data.stdout.AppendLine (e.Data);
						}
					};

					p.ErrorDataReceived += delegate (object sender, DataReceivedEventArgs e) {
						lock (data.stderrLock) {
							if (e.Data != null)
								data.stderr.AppendLine (e.Data);
						}
					};

					var start = DateTime.UtcNow;

					p.Start ();

					p.BeginOutputReadLine ();
					p.BeginErrorReadLine ();

					if (!p.WaitForExit (timeout * 1000)) {
						var end = DateTime.UtcNow;
						data.duration =  end - start;

						lock (monitor) {
							timedout.Add (data);
						}

						// Force the process to print a thread dump
						TryThreadDump (p.Id, data);

						if (verbose) {
							output.Write ($"timed out ({timeout}s)");
						}

						try {
							p.Kill ();
						} catch {
						}
					} else if (p.ExitCode != expectedExitCode) {
						var end = DateTime.UtcNow;
						data.duration =  end - start;

						lock (monitor) {
							failed.Add (data);
						}

						if (verbose)
							output.Write ("failed, time: {0}, exit code: {1}", data.duration.ToString (TEST_TIME_FORMAT), p.ExitCode);
					} else {
						var end = DateTime.UtcNow;
						data.duration =  end - start;

						lock (monitor) {
							passed.Add (data);
						}

						if (verbose)
							output.Write ("passed, time: {0}", data.duration.ToString (TEST_TIME_FORMAT));
					}

					p.Close ();

					lock (monitor) {
						if (verbose)
							Console.WriteLine (output.ToString ());
					}
				}
			});

			thread.Start ();

			threads.Add (thread);
		}

		for (int j = 0; j < threads.Count; ++j)
			threads [j].Join ();

		TimeSpan test_time = DateTime.UtcNow - test_start_time;

		int npassed = passed.Count;
		int nfailed = failed.Count;
		int ntimedout = timedout.Count;

		XmlWriterSettings xmlWriterSettings = new XmlWriterSettings ();
		xmlWriterSettings.NewLineOnAttributes = true;
		xmlWriterSettings.Indent = true;

		string xmlPath = String.Format ("TestResult-{0}.xml", testsuiteName);
		using (XmlWriter writer = XmlWriter.Create (xmlPath, xmlWriterSettings)) {
			// <?xml version="1.0" encoding="utf-8" standalone="no"?>
			writer.WriteStartDocument ();
			// <!--This file represents the results of running a test suite-->
			writer.WriteComment ("This file represents the results of running a test suite");
			// <test-results name="/home/charlie/Dev/NUnit/nunit-2.5/work/src/bin/Debug/tests/mock-assembly.dll" total="21" errors="1" failures="1" not-run="7" inconclusive="1" ignored="4" skipped="0" invalid="3" date="2010-10-18" time="13:23:35">
			writer.WriteStartElement ("test-results");
			writer.WriteAttributeString ("name", String.Format ("{0}-tests.dummy", testsuiteName));
			writer.WriteAttributeString ("total", (npassed + nfailed + ntimedout).ToString());
			writer.WriteAttributeString ("failures", (nfailed + ntimedout).ToString());
			writer.WriteAttributeString ("not-run", "0");
			writer.WriteAttributeString ("date", DateTime.Now.ToString ("yyyy-MM-dd"));
			writer.WriteAttributeString ("time", DateTime.Now.ToString ("HH:mm:ss"));
			//   <environment nunit-version="2.4.8.0" clr-version="4.0.30319.17020" os-version="Unix 3.13.0.45" platform="Unix" cwd="/home/directhex/Projects/mono/mcs/class/corlib" machine-name="marceline" user="directhex" user-domain="marceline" />
			writer.WriteStartElement ("environment");
			writer.WriteAttributeString ("nunit-version", "2.4.8.0" );
			writer.WriteAttributeString ("clr-version", Environment.Version.ToString() );
			writer.WriteAttributeString ("os-version", Environment.OSVersion.ToString() );
			writer.WriteAttributeString ("platform", Environment.OSVersion.Platform.ToString() );
			writer.WriteAttributeString ("cwd", Environment.CurrentDirectory );
			writer.WriteAttributeString ("machine-name", Environment.MachineName );
			writer.WriteAttributeString ("user", Environment.UserName );
			writer.WriteAttributeString ("user-domain", Environment.UserDomainName );
			writer.WriteEndElement ();
			//   <culture-info current-culture="en-GB" current-uiculture="en-GB" />
			writer.WriteStartElement ("culture-info");
			writer.WriteAttributeString ("current-culture", CultureInfo.CurrentCulture.Name );
			writer.WriteAttributeString ("current-uiculture", CultureInfo.CurrentUICulture.Name );
			writer.WriteEndElement ();
			//   <test-suite name="corlib_test_net_4_5.dll" success="True" time="114.318" asserts="0">
			writer.WriteStartElement ("test-suite");
			writer.WriteAttributeString ("name", String.Format ("{0}-tests.dummy", testsuiteName));
			writer.WriteAttributeString ("success", (nfailed + ntimedout == 0).ToString());
			writer.WriteAttributeString ("time", test_time.TotalSeconds.ToString(CultureInfo.InvariantCulture));
			writer.WriteAttributeString ("asserts", (nfailed + ntimedout).ToString());
			//     <results>
			writer.WriteStartElement ("results");
			//       <test-suite name="MonoTests" success="True" time="114.318" asserts="0">
			writer.WriteStartElement ("test-suite");
			writer.WriteAttributeString ("name","MonoTests");
			writer.WriteAttributeString ("success", (nfailed + ntimedout == 0).ToString());
			writer.WriteAttributeString ("time", test_time.TotalSeconds.ToString(CultureInfo.InvariantCulture));
			writer.WriteAttributeString ("asserts", (nfailed + ntimedout).ToString());
			//         <results>
			writer.WriteStartElement ("results");
			//           <test-suite name="MonoTests" success="True" time="114.318" asserts="0">
			writer.WriteStartElement ("test-suite");
			writer.WriteAttributeString ("name", testsuiteName);
			writer.WriteAttributeString ("success", (nfailed + ntimedout == 0).ToString());
			writer.WriteAttributeString ("time", test_time.TotalSeconds.ToString(CultureInfo.InvariantCulture));
			writer.WriteAttributeString ("asserts", (nfailed + ntimedout).ToString());
			//             <results>
			writer.WriteStartElement ("results");
			// Dump all passing tests first
			foreach (ProcessData pd in passed) {
				// <test-case name="MonoTests.Microsoft.Win32.RegistryKeyTest.bug79051" executed="True" success="True" time="0.063" asserts="0" />
				writer.WriteStartElement ("test-case");
				writer.WriteAttributeString ("name", String.Format ("MonoTests.{0}.{1}", testsuiteName, pd.test));
				writer.WriteAttributeString ("executed", "True");
				writer.WriteAttributeString ("success", "True");
				writer.WriteAttributeString ("time", pd.duration.TotalSeconds.ToString(CultureInfo.InvariantCulture));
				writer.WriteAttributeString ("asserts", "0");
				writer.WriteEndElement ();
			}
			// Now dump all failing tests
			foreach (ProcessData pd in failed) {
				// <test-case name="MonoTests.Microsoft.Win32.RegistryKeyTest.bug79051" executed="True" success="True" time="0.063" asserts="0" />
				writer.WriteStartElement ("test-case");
				writer.WriteAttributeString ("name", String.Format ("MonoTests.{0}.{1}", testsuiteName, pd.test));
				writer.WriteAttributeString ("executed", "True");
				writer.WriteAttributeString ("success", "False");
				writer.WriteAttributeString ("time", pd.duration.TotalSeconds.ToString(CultureInfo.InvariantCulture));
				writer.WriteAttributeString ("asserts", "1");
				writer.WriteStartElement ("failure");
				writer.WriteStartElement ("message");
				writer.WriteCData (FilterInvalidXmlChars (pd.stdout.ToString ()));
				writer.WriteEndElement ();
				writer.WriteStartElement ("stack-trace");
				writer.WriteCData (FilterInvalidXmlChars (pd.stderr.ToString ()));
				writer.WriteEndElement ();
				writer.WriteEndElement ();
				writer.WriteEndElement ();
			}
			// Then dump all timing out tests
			foreach (ProcessData pd in timedout) {
				// <test-case name="MonoTests.Microsoft.Win32.RegistryKeyTest.bug79051" executed="True" success="True" time="0.063" asserts="0" />
				writer.WriteStartElement ("test-case");
				writer.WriteAttributeString ("name", String.Format ("MonoTests.{0}.{1}_timedout", testsuiteName, pd.test));
				writer.WriteAttributeString ("executed", "True");
				writer.WriteAttributeString ("success", "False");
				writer.WriteAttributeString ("time", pd.duration.TotalSeconds.ToString(CultureInfo.InvariantCulture));
				writer.WriteAttributeString ("asserts", "1");
				writer.WriteStartElement ("failure");
				writer.WriteStartElement ("message");
				writer.WriteCData (FilterInvalidXmlChars (pd.stdout.ToString ()));
				writer.WriteEndElement ();
				writer.WriteStartElement ("stack-trace");
				writer.WriteCData (FilterInvalidXmlChars (pd.stderr.ToString ()));
				writer.WriteEndElement ();
				writer.WriteEndElement ();
				writer.WriteEndElement ();
			}
			//             </results>
			writer.WriteEndElement ();
			//           </test-suite>
			writer.WriteEndElement ();
			//         </results>
			writer.WriteEndElement ();
			//       </test-suite>
			writer.WriteEndElement ();
			//     </results>
			writer.WriteEndElement ();
			//   </test-suite>
			writer.WriteEndElement ();
			// </test-results>
			writer.WriteEndElement ();
			writer.WriteEndDocument ();

			string babysitterXmlList = Environment.GetEnvironmentVariable("MONO_BABYSITTER_NUNIT_XML_LIST_FILE");
			if (!String.IsNullOrEmpty(babysitterXmlList)) {
				try {
					string fullXmlPath = Path.GetFullPath(xmlPath);
					File.AppendAllText(babysitterXmlList, fullXmlPath + Environment.NewLine);
				} catch (Exception e) {
					Console.WriteLine("Attempted to record XML path to file {0} but failed.", babysitterXmlList);
				}
			}
		}

		if (verbose) {
			Console.WriteLine ();
			Console.WriteLine ("Time: {0}", test_time.ToString (TEST_TIME_FORMAT));
			Console.WriteLine ();
			Console.WriteLine ("{0,4} test(s) passed", npassed);
			Console.WriteLine ("{0,4} test(s) failed", nfailed);
			Console.WriteLine ("{0,4} test(s) timed out", ntimedout);
		} else {
			Console.WriteLine ();
			Console.WriteLine (String.Format ("{0} test(s) passed, {1} test(s) did not pass.", npassed, nfailed));
		}

		if (nfailed > 0) {
			Console.WriteLine ();
			Console.WriteLine ("Failed test(s):");
			foreach (ProcessData pd in failed) {
				Console.WriteLine ();
				Console.WriteLine (pd.test);
				DumpFile (pd.stdoutName, pd.stdout.ToString ());
				DumpFile (pd.stderrName, pd.stderr.ToString ());
			}
		}

		if (npassed > 0) {
			Console.WriteLine ();
			Console.WriteLine ("Passed test(s):");
			foreach (ProcessData pd in passed) {
				Console.WriteLine ();
				Console.WriteLine (pd.test);
				DumpFile (pd.stdoutName, pd.stdout.ToString ());
				DumpFile (pd.stderrName, pd.stderr.ToString ());
			}
		}

		if (ntimedout > 0) {
			Console.WriteLine ();
			Console.WriteLine ("Timed out test(s):");
			foreach (ProcessData pd in timedout) {
				Console.WriteLine ();
				Console.WriteLine (pd.test);
				DumpFile (pd.stdoutName, pd.stdout.ToString ());
				DumpFile (pd.stderrName, pd.stderr.ToString ());
			}
		}

		return (ntimedout == 0 && nfailed == 0) ? 0 : 1;
	}
	
	static void DumpFile (string filename, string text) {
		Console.WriteLine ("=============== {0} ===============", filename);
		Console.WriteLine (text);
		Console.WriteLine ("=============== EOF ===============");
	}

	static string FilterInvalidXmlChars (string text) {
		// Spec at http://www.w3.org/TR/2008/REC-xml-20081126/#charsets says only the following chars are valid in XML:
		// Char ::= #x9 | #xA | #xD | [#x20-#xD7FF] | [#xE000-#xFFFD] | [#x10000-#x10FFFF]	/* any Unicode character, excluding the surrogate blocks, FFFE, and FFFF. */
		return Regex.Replace (text, @"[^\x09\x0A\x0D\x20-\uD7FF\uE000-\uFFFD\u10000-\u10FFFF]", "");
	}

	static void TryThreadDump (int pid, ProcessData data)
	{
		try {
			TryGDB (pid, data);
			return;
		} catch {
		}

#if !FULL_AOT_DESKTOP && !MOBILE
		/* LLDB cannot produce managed stacktraces for all the threads */
		try {
			Syscall.kill (pid, Signum.SIGQUIT);
			Thread.Sleep (1000);
		} catch {
		}
#endif

		try {
			TryLLDB (pid, data);
			return;
		} catch {
		}
	}

	static void TryLLDB (int pid, ProcessData data)
	{
		string filename = Path.GetTempFileName ();

		using (StreamWriter sw = new StreamWriter (new FileStream (filename, FileMode.Open, FileAccess.Write)))
		{
			sw.WriteLine ("process attach --pid " + pid);
			sw.WriteLine ("thread list");
			sw.WriteLine ("thread backtrace all");
			sw.WriteLine ("detach");
			sw.WriteLine ("quit");
			sw.Flush ();

			ProcessStartInfo psi = new ProcessStartInfo {
				FileName = "lldb",
				Arguments = "--batch --source \"" + filename + "\" --no-lldbinit",
				UseShellExecute = false,
				RedirectStandardError = true,
				RedirectStandardOutput = true,
			};

			using (Process process = new Process { StartInfo = psi })
			{
				process.OutputDataReceived += delegate (object sender, DataReceivedEventArgs e) {
					lock (data.stdoutLock) {
						if (e.Data != null)
							data.stdout.AppendLine (e.Data);
					}
				};

				process.ErrorDataReceived += delegate (object sender, DataReceivedEventArgs e) {
					lock (data.stderrLock) {
						if (e.Data != null)
							data.stderr.AppendLine (e.Data);
					}
				};

				process.Start ();
				process.BeginOutputReadLine ();
				process.BeginErrorReadLine ();
				if (!process.WaitForExit (60 * 1000))
					process.Kill ();
			}
		}
	}

	static void TryGDB (int pid, ProcessData data)
	{
		string filename = Path.GetTempFileName ();

		using (StreamWriter sw = new StreamWriter (new FileStream (filename, FileMode.Open, FileAccess.Write)))
		{
			sw.WriteLine ("attach " + pid);
			sw.WriteLine ("info threads");
			sw.WriteLine ("thread apply all p mono_print_thread_dump(0)");
			sw.WriteLine ("thread apply all backtrace");
			sw.Flush ();

			ProcessStartInfo psi = new ProcessStartInfo {
				FileName = "gdb",
				Arguments = "-batch -x \"" + filename + "\" -nx",
				UseShellExecute = false,
				RedirectStandardError = true,
				RedirectStandardOutput = true,
			};

			using (Process process = new Process { StartInfo = psi })
			{
				process.OutputDataReceived += delegate (object sender, DataReceivedEventArgs e) {
					lock (data.stdoutLock) {
						if (e.Data != null)
							data.stdout.AppendLine (e.Data);
					}
				};

				process.ErrorDataReceived += delegate (object sender, DataReceivedEventArgs e) {
					lock (data.stderrLock) {
						if (e.Data != null)
							data.stderr.AppendLine (e.Data);
					}
				};

				process.Start ();
				process.BeginOutputReadLine ();
				process.BeginErrorReadLine ();
				if (!process.WaitForExit (60 * 1000))
					process.Kill ();
			}
		}
	}
}
