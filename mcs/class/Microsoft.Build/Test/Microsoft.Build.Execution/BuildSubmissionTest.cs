//
// BuildSubmissionTest.cs
//
// Author:
//   Atsushi Enomoto (atsushi@xamarin.com)
//
// Copyright (C) 2013 Xamarin Inc. (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.IO;
using System.Linq;
using System.Xml;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using NUnit.Framework;
using Microsoft.Build.Logging;
using Microsoft.Build.Framework;
using System.Collections.Generic;

namespace MonoTests.Microsoft.Build.Execution
{
	[TestFixture]
	public class BuildSubmissionTest
	{
		[Test]
		public void ResultBeforeExecute ()
		{
			string empty_project_xml = "<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003' />";
			var path = "file://localhost/foo.xml";
			var xml = XmlReader.Create (new StringReader (empty_project_xml), null, path);
			var root = ProjectRootElement.Create (xml);
			var proj = new ProjectInstance (root);
			var bm = new BuildManager ();
			bm.BeginBuild (new BuildParameters ());
			var sub = bm.PendBuildRequest (new BuildRequestData (proj, new string [0]));
			Assert.IsNull (sub.BuildResult, "#1");
		}
		
		// This checks if the build output for each task is written to the loggers and not directly thrown as a Project loader error.
		[Test]
		public void TaskOutputsToLoggers ()
		{
            string project_xml = @"<Project DefaultTargets='Foo' xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <Import Project='$(MSBuildToolsPath)\Microsoft.CSharp.targets' />
  <Target Name='Foo'>
    <ItemGroup>
      <Foo Condition='$(X)' Include='foo.txt' />
    </ItemGroup>
  </Target>
</Project>";
            var xml = XmlReader.Create (new StringReader (project_xml));
            var root = ProjectRootElement.Create (xml);
			root.FullPath = "BuildSubmissionTest.TaskOutputsToLoggers.proj";
            var proj = new ProjectInstance (root);
			Assert.AreEqual ("$(X)", root.Targets.First ().ItemGroups.First ().Items.First ().Condition, "#0");
			var sw = new StringWriter ();
			Assert.IsFalse (proj.Build (new ILogger [] {new ConsoleLogger (LoggerVerbosity.Diagnostic, sw.WriteLine, null, null)}), "#1");
			Assert.IsTrue (sw.ToString ().Contains ("$(X)"), "#2");
		}
		
		[Test]
		public void EndBuildWaitsForSubmissionCompletion ()
		{
			string project_xml = string.Format (@"<Project DefaultTargets='Wait1Sec' xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <Target Name='Wait1Sec'>
    <Exec Command='{0}' />
  </Target>
</Project>", Environment.OSVersion.Platform == PlatformID.Win32NT ? "powershell -command \"Start-Sleep -s 1\"" : "/bin/sleep 1");
			var xml = XmlReader.Create (new StringReader (project_xml));
			var root = ProjectRootElement.Create (xml);
			root.FullPath = "BuildSubmissionTest.EndBuildWaitsForSubmissionCompletion.proj";
			var proj = new ProjectInstance (root);
			var bm = new BuildManager ();
			bm.BeginBuild (new BuildParameters ());
			var waitDone = TimeSpan.MinValue;
			var sw = System.Diagnostics.Stopwatch.StartNew ();
			var sub = bm.PendBuildRequest (new BuildRequestData (proj, new string [] { "Wait1Sec" }));
			sub.ExecuteAsync (cb => waitDone = sw.Elapsed, null);
			bm.EndBuild ();
			Assert.AreEqual (BuildResultCode.Success, sub.BuildResult.OverallResult, "#1");
			var endBuildDone = sw.Elapsed;
			AssertHelper.GreaterOrEqual (endBuildDone, TimeSpan.FromSeconds (1), "#2");
			AssertHelper.GreaterOrEqual (waitDone, TimeSpan.FromSeconds (1), "#3");
			AssertHelper.GreaterOrEqual (endBuildDone, waitDone, "#4");
			AssertHelper.LessOrEqual (endBuildDone, TimeSpan.FromSeconds (10.0), "#5");
			AssertHelper.LessOrEqual (waitDone, TimeSpan.FromSeconds (10.0), "#6");
		}
		
		[Test]
		public void BuildParameterLoggersExplicitlyRequired ()
		{
            string project_xml = @"<Project DefaultTargets='Foo' xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <Import Project='$(MSBuildToolsPath)\Microsoft.CSharp.targets' />
  <Target Name='Foo'>
    <ItemGroup>
      <Foo Condition='$(X)' Include='foo.txt' />
    </ItemGroup>
  </Target>
</Project>";
            var xml = XmlReader.Create (new StringReader (project_xml));
            var root = ProjectRootElement.Create (xml);
			root.FullPath = "BuildSubmissionTest.BuildParameterLoggersExplicitlyRequired.proj";
			var pc = new ProjectCollection ();
			var sw = new StringWriter ();
			pc.RegisterLogger (new ConsoleLogger (LoggerVerbosity.Diagnostic, sw.WriteLine, null, null));
			var proj = new ProjectInstance (root);
			var bm = new BuildManager ();
			var bp = new BuildParameters (pc);
			var br = new BuildRequestData (proj, new string [] {"Foo"});
			Assert.AreEqual (BuildResultCode.Failure, bm.Build (bp, br).OverallResult, "#1");
			// the logger is *ignored*
			Assert.IsFalse (sw.ToString ().Contains ("$(X)"), "#2");
		}
		
		[Test]
		public void ProjectInstanceBuildLoggersExplicitlyRequired ()
		{
            string project_xml = @"<Project DefaultTargets='Foo' xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <Import Project='$(MSBuildToolsPath)\Microsoft.CSharp.targets' />
  <Target Name='Foo'>
    <ItemGroup>
      <Foo Condition='$(X)' Include='foo.txt' />
    </ItemGroup>
  </Target>
</Project>";
            var xml = XmlReader.Create (new StringReader (project_xml));
            var root = ProjectRootElement.Create (xml);
			root.FullPath = "BuildSubmissionTest.TaskOutputsToLoggers.proj";
			var pc = new ProjectCollection ();
			var sw = new StringWriter ();
			pc.RegisterLogger (new ConsoleLogger (LoggerVerbosity.Diagnostic, sw.WriteLine, null, null));
			var proj = new ProjectInstance (root);
			Assert.IsFalse (proj.Build (), "#1");
			// the logger is *ignored* again
			Assert.IsFalse (sw.ToString ().Contains ("$(X)"), "#2");
		}
	}
}

