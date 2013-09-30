
using System;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Logging;
using NUnit.Framework;

namespace MonoTests.Microsoft.Build.Logging
{
	[TestFixture]
	public class ConsoleLoggerTest
	{
		[Test]
		public void BasicLoggerUsage ()
		{
			string expected = @"file : cat error code: msg

file : cat warning code: msg

__________________________________________________

Project ""project.txt"" (target target(s)):



Build started 2013/01/01 00:00:00.

Target ""target"" in file ""target.txt"":

  start task

  finished task

finished target



finished project



finished build



Time Elapsed 00:00:00.01

".Replace ("\r\n", "\n");
			var sw = new StringWriter();
			var e = new ConsoleLogger(LoggerVerbosity.Diagnostic, msg => sw.WriteLine(msg), c => {}, () => {});
			e.Verbosity = LoggerVerbosity.Diagnostic;
			e.ErrorHandler (null, new BuildErrorEventArgs ("cat", "code", "file", 0, 0, 0, 0, "msg", "help", "sender"));
			e.WarningHandler (null, new BuildWarningEventArgs ("cat", "code", "file", 0, 0, 0, 0, "msg", "help", "sender"));
			e.ProjectStartedHandler (null, new ProjectStartedEventArgs ("start project", "HELPME", "project.txt", "target", null, null));
			e.BuildStartedHandler (null, new BuildStartedEventArgs ("start build", "HELPME", new DateTime (2013, 1, 1)));
			e.TargetStartedHandler (null, new TargetStartedEventArgs ("start target", "HELPME", "target", "project.txt", "target.txt"/*, "parent"*/));
			e.TaskStartedHandler (null, new TaskStartedEventArgs ("start task", "HELPME", "project.txt", "task.txt", "task"));
			e.TaskFinishedHandler (null, new TaskFinishedEventArgs ("finished task", "HELPME", "project.txt", "task.txt", "task", false));
			e.TargetFinishedHandler (null, new TargetFinishedEventArgs ("finished target", "HELPME", "target", "project.txt", "target.txt", false));
			e.ProjectFinishedHandler (null, new ProjectFinishedEventArgs ("finished project", "HELPME", "project.txt", false));
			e.BuildFinishedHandler (null, new BuildFinishedEventArgs ("finished build", "HELPME", false, new DateTime (2013, 1, 1).AddMilliseconds (1)));

			e.CustomEventHandler(null, new MyCustomBuildEventArgs ());
			Assert.AreEqual (expected, sw.ToString ().Replace ("\r\n", "\n"), "#1");
		}
	}
	
	class MyCustomBuildEventArgs : CustomBuildEventArgs
	{
	}
}

