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

namespace MonoTests.Microsoft.Build.Execution
{
	[TestFixture]
	public class BuildSubmissionTest
	{
		// This checks if the build output for each task is written to the loggers and not directly thrown as a Project loader error.
		[Test]
		public void TaskOutputsToLoggers ()
		{
            string project_xml = @"<Project DefaultTargets='Foo;Bar' xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <Import Project='$(MSBuildToolsPath)\Microsoft.Common.targets' />
  <Target Name='Foo'>
    <ItemGroup>
      <Foo Condition='$(X)' Include='foo.txt' />
    </ItemGroup>
  </Target>
</Project>";
            var xml = XmlReader.Create (new StringReader (project_xml));
            var root = ProjectRootElement.Create (xml);
            var proj = new ProjectInstance (root);
			var sw = new StringWriter ();
			Assert.IsFalse (proj.Build (new ILogger [] {new ConsoleLogger (default (LoggerVerbosity), msg => sw.Write (msg), null, null)}), "#1");
			Assert.IsTrue (sw.ToString ().Contains ("$(X)"), "#2");
		}
	}
}

