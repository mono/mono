// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Mono.Profiler.Log;

namespace MonoTests.Mono.Profiler.Log {

	sealed class ProfilerTestRun {

		sealed class ProfilerTestVisitor : LogEventVisitor {

			public ProfilerTestRun Run { get; }

			public List<LogEvent> Events { get; } = new List<LogEvent> ();

			public ProfilerTestVisitor (ProfilerTestRun run)
			{
				Run = run;
			}

			public override void VisitBefore (LogEvent ev)
			{
				Events.Add (ev);
			}
		}

		public string Name { get; }

		public string Options { get; }

		readonly string _output;

		static volatile int _id;

		static string _testAssemblyPath;
		static Process _currentProcess;

		public ProfilerTestRun (string name, string options)
		{
			_testAssemblyPath = Path.Combine (Path.GetDirectoryName (System.Reflection.Assembly.GetExecutingAssembly ().Location), "log-profiler-test.exe");
			_currentProcess = Process.GetCurrentProcess();
			Name = name;
			Options = options;
			_output = Path.GetFullPath ($"test-{_id++}.mlpd");
		}

		public void Run (Action<IReadOnlyList<LogEvent>> action)
		{
			RunTest ();
			var events = ParseFile ();

			action (events);
		}

		void RunTest ()
		{
			using (var proc = new Process ()) {
				proc.StartInfo = new ProcessStartInfo {
					UseShellExecute = false,
					FileName = _currentProcess.MainModule.FileName,
					Arguments = $"--debug --profile=log:nodefaults,output=\"{_output}\",{Options} {_testAssemblyPath} {Name}",
				};

				proc.Start ();
				proc.WaitForExit ();

				if (proc.ExitCode != 0)
					throw new Exception ($"Profiler test process exited with code: {proc.ExitCode}");
			}
		}

		IReadOnlyList<LogEvent> ParseFile ()
		{
			var visitor = new ProfilerTestVisitor (this);

			using (var stream = new LogStream (File.OpenRead (_output)))
				new LogProcessor (stream, null, visitor).Process ();

			return visitor.Events;
		}
	}
}
