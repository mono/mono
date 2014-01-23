//
// ToolTaskTest.cs:
//
// Author:
//   Jonathan Pryor (jonp@xamarin.com)
//
// (C) 2013 Xamarin Inc.
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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using NUnit.Framework;

namespace MonoTests.Microsoft.Build.Utilities {

	[TestFixture]
	public class ToolTaskTest {

		[Test]
		public void LogEventsFromTextOutput ()
		{
			var messages = new[] {
				new {Code = "CS0152", Line = "class1.cs(16,4): error CS0152: The label `case 1:' already occurs in this switch statement" },
			};

			var task = new LogEventsFromTextOutputToolTask ();
			foreach (var m in messages) {
				task.LogEventsFromTextOutput (m.Line);
				Assert.IsTrue (task.Codes.Count > 0, "No error logged for line: {0}", m.Line);
				Assert.AreEqual (m.Code, task.Codes [0]);
				task.Codes.Clear ();
			}
		}
	}

	class LogEventsFromTextOutputToolTask : ToolTask {

		public List<string> Codes {
			get {return engine.Codes;}
		}

		CodeLoggingBuildEngine engine = new CodeLoggingBuildEngine ();

		public LogEventsFromTextOutputToolTask ()
		{
			BuildEngine = engine;
		}

		protected override string GenerateFullPathToTool ()
		{
			throw new NotImplementedException ();
		}

		protected override string ToolName {
			get {throw new NotImplementedException ();}
		}

		public void LogEventsFromTextOutput (string line)
		{
			base.LogEventsFromTextOutput (line, MessageImportance.Normal);
		}
	}

	class CodeLoggingBuildEngine : IBuildEngine {

		public List<string> Codes = new List<string> ();

		public int ColumnNumberOfTaskNode {
			get {
				throw new NotImplementedException ();
			}
		}

		public bool ContinueOnError {
			get {
				throw new NotImplementedException ();
			}
		}

		public int LineNumberOfTaskNode {
			get {
				throw new NotImplementedException ();
			}
		}

		public string ProjectFileOfTaskNode {
			get {
				throw new NotImplementedException ();
			}
		}

		public bool BuildProjectFile (string projectFileName, string[] targetNames, System.Collections.IDictionary globalProperties, System.Collections.IDictionary targetOutputs)
		{
			throw new NotImplementedException ();
		}

		public void LogCustomEvent (CustomBuildEventArgs e)
		{
		}

		public void LogErrorEvent (BuildErrorEventArgs e)
		{
			Codes.Add (e.Code);
		}

		public void LogMessageEvent (BuildMessageEventArgs e)
		{
		}

		public void LogWarningEvent (BuildWarningEventArgs e)
		{
			Codes.Add (e.Code);
		}
	}
}

