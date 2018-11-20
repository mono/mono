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
using System.Collections;
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

		[Test, TestCaseSource (nameof (GetErrorParsingTestData))]
		public void LogEventsFromTextOutput (string lineText, LogEvent expected)
		{
			var task = new LogEventsFromTextOutputToolTask ();
			task.LogEventsFromTextOutput (lineText);
			Assert.AreEqual (task.LogEvents.Count, 1);
			var result = task.LogEvents[0];
			task.LogEvents.Clear ();

			if (result != null && result.Origin == task.GetType ().Name.ToUpper ())
				result.Origin = "#TASKNAME";

			if (expected == null) {
				Assert.IsNull (result, "#nomatch");
				return;
			}

			Assert.IsNotNull (result, "#match");
			Assert.AreEqual (expected.Origin, result.Origin, "#origin");
			Assert.AreEqual (expected.Line, result.Line, "#line");
			Assert.AreEqual (expected.Column, result.Column, "#column");
			Assert.AreEqual (expected.EndLine, result.EndLine, "#endline");
			Assert.AreEqual (expected.EndColumn, result.EndColumn, "#endcolumn");
			Assert.AreEqual (expected.IsError, result.IsError, "#iserror");
			Assert.AreEqual (expected.Subcategory ?? "", result.Subcategory, "#subcategory");
			Assert.AreEqual (expected.Code, result.Code, "number");
			Assert.AreEqual (expected.Message ?? "", result.Message, "#message");
		}

		static IEnumerable<TestCaseData> GetErrorParsingTestData ()
		{
			yield return new TestCaseData (
				"error   CS66",
				null
			).SetName ("NoColon");

			yield return new TestCaseData (
				"error   CS66 : ",
				new LogEvent {
					Origin = "#TASKNAME",
					IsError = true,
					Code = "CS66"
				}
			).SetName ("Minimal");

			yield return new TestCaseData (
				"pineapple   CS66 : ",
				null
			).SetName ("InvalidCategory");

			yield return new TestCaseData (
				"ERROR  CS66 : ",
				new LogEvent {
					Origin = "#TASKNAME",
					IsError = true,
					Code = "CS66"
				}
			).SetName ("CaseInsensitivity");

			yield return new TestCaseData (
				": error  CS66 : ",
				new LogEvent {
					Origin = "#TASKNAME",
					IsError = true,
					Code = "CS66"
				}
			).SetName ("EmptyOrigin");

			yield return new TestCaseData (
				"     : error  CS66 : ",
				new LogEvent {
					Origin = "#TASKNAME",
					IsError = true,
					Code = "CS66"
				}
			).SetName ("BlankOrigin");

			yield return new TestCaseData (
				"error   CS66 : error in 'hello:thing'",
				new LogEvent {
					Origin = "#TASKNAME",
					IsError = true,
					Code = "CS66",
					Message = "error in 'hello:thing'",
				}
			).SetName ("NoOriginButErrorLikeMessage");

			yield return new TestCaseData (
				"   C:\\class.cs   (23,344)  :    error   CS66   : blah    ",
				new LogEvent {
					Origin = "C:\\class.cs",
					Line = 23,
					Column = 344,
					IsError = true,
					Code = "CS66",
					Message = "blah",
				}
			).SetName ("Whitespace");

			yield return new TestCaseData (
				"class1.cs(16,4): error CS0152: The label `case 1:' already occurs in this switch statement",
				new LogEvent {
					Origin = "class1.cs",
					Line = 16,
					Column = 4,
					IsError = true,
					Code = "CS0152",
					Message = "The label `case 1:' already occurs in this switch statement",
				}
			).SetName ("RangeLineCol");

			yield return new TestCaseData (
				"class1.cs(16,4-56): error X: blah",
				new LogEvent {
					Origin = "class1.cs",
					Line = 16,
					Column = 4,
					EndColumn = 56,
					IsError = true,
					Code = "X",
					Message = "blah",
				}
			).SetName ("RangeLineColCol");

			yield return new TestCaseData (
				"class1.cs(16,4,56,7): error X: blah",
				new LogEvent {
					Origin = "class1.cs",
					Line = 16,
					Column = 4,
					EndLine = 56,
					EndColumn = 7,
					IsError = true,
					Code = "X",
					Message = "blah",
				}
			).SetName ("RangeLineColLineCol");

			yield return new TestCaseData (
				"class1.cs(1-77): error X: blah",
				new LogEvent {
					Origin = "class1.cs",
					Line = 1,
					EndLine = 77,
					IsError = true,
					Code = "X",
					Message = "blah",
				}
			).SetName ("RangeLineLine");

			yield return new TestCaseData (
				"class1.cs(1-77-89): error X: blah",
				new LogEvent {
					Origin = "class1.cs",
					IsError = true,
					Code = "X",
					Message = "blah",
				}
			).SetName ("BadRangeTooManyDashes");

			yield return new TestCaseData (
				"class1.cs(1&77-89): error X: blah",
				new LogEvent {
					Origin = "class1.cs(1&77-89)",
					IsError = true,
					Code = "X",
					Message = "blah",
				}
			).SetName ("BadRangePunctuation");

			yield return new TestCaseData (
				"class1.cs(ASDF): error X: blah",
				new LogEvent {
					Origin = "class1.cs(ASDF)",
					IsError = true,
					Code = "X",
					Message = "blah",
				}
			).SetName ("BadRangeAlpha");

			yield return new TestCaseData (
				"class1.cs(12AA45): error X: blah",
				new LogEvent {
					Origin = "class1.cs(12AA45)",
					IsError = true,
					Code = "X",
					Message = "blah",
				}
			).SetName ("BadRangeAlphaNumeric");

			yield return new TestCaseData (
				"class1.cs(1-77,89-56): error X: blah",
				new LogEvent {
					Origin = "class1.cs",
					IsError = true,
					Code = "X",
					Message = "blah",
				}
			).SetName ("BadRangeLineLineColCol");

			yield return new TestCaseData (
				"class1.cs(1,77,89): error X: blah",
				new LogEvent {
					Origin = "class1.cs",
					IsError = true,
					Code = "X",
					Message = "blah",
				}
			).SetName ("BadRangeThreeCommas");

			yield return new TestCaseData (
				"class1.cs(0): error X:",
				new LogEvent {
					Origin = "class1.cs",
					IsError = true,
					Code = "X",
				}
			).SetName ("RangeZero");

			yield return new TestCaseData (
				"class1.cs(2,1234567890192929293833838380): error X:",
				new LogEvent {
					Origin = "class1.cs",
					Line = 2,
					IsError = true,
					Code = "X",
				}
			).SetName ("BadRangeOverflowCol");

			yield return new TestCaseData (
				"class1.cs(2,1234567890192929293833838380,5,7): error X:",
				new LogEvent {
					Line = 2,
					EndLine = 5,
					EndColumn = 7,
					Origin = "class1.cs",
					IsError = true,
					Code = "X",
				}
			).SetName ("BadRangeOverflowBeforeValues");

			yield return new TestCaseData (
				"c:\\foo error XXX: fatal error YYY : error blah : thing",
				new LogEvent {
					Origin = "c:\\foo error XXX",
					IsError = true,
					Subcategory = "fatal",
					Code = "YYY",
					Message = "error blah : thing",
				}
			).SetName ("LotsOfColons");

			yield return new TestCaseData (
				"Main.cs(17,20): warning CS0168: The variable 'foo' is declared but never used",
				new LogEvent {
					Origin = "Main.cs",
					Line = 17,
					Column = 20,
					Code = "CS0168",
					Message = "The variable 'foo' is declared but never used",
				}
			).SetName ("MSExample1");

			yield return new TestCaseData (
				"C:\\dir1\\foo.resx(2) : error BC30188: Declaration expected.",
				new LogEvent {
					Origin = "C:\\dir1\\foo.resx",
					Line = 2,
					IsError = true,
					Code = "BC30188",
					Message = "Declaration expected.",
				}
			).SetName ("MSExample2");

			yield return new TestCaseData (
				"cl : Command line warning D4024 : unrecognized source file type 'foo.cs', object file assumed",
				new LogEvent {
					Origin = "cl",
					Subcategory = "Command line",
					Code = "D4024",
					Message = "unrecognized source file type 'foo.cs', object file assumed",
				}
			).SetName ("MSExample3");

			yield return new TestCaseData (
				"error CS0006: Metadata file 'System.dll' could not be found.",
				new LogEvent {
					Origin = "#TASKNAME",
					IsError = true,
					Code = "CS0006",
					Message = "Metadata file 'System.dll' could not be found.",
				}
			).SetName ("MSExample4");

			yield return new TestCaseData (
				"C:\\sourcefile.cpp(134) : error C2143: syntax error : missing ';' before '}'",
				new LogEvent {
					Origin = "C:\\sourcefile.cpp",
					Line = 134,
					IsError = true,
					Code = "C2143",
					Message = "syntax error : missing ';' before '}'",
				}
			).SetName ("MSExample5");

			yield return new TestCaseData (
				"LINK : fatal error LNK1104: cannot open file 'somelib.lib'",
				new LogEvent {
					Origin = "LINK",
					Subcategory = "fatal",
					IsError = true,
					Code = "LNK1104",
					Message = "cannot open file 'somelib.lib'",
				}
			).SetName ("MSExample6");

			yield return new TestCaseData (
				"/foo (bar)/baz/Component1.fs(3,5): error FS0201: Namespaces cannot contain values.",
				new LogEvent {
					Origin = "/foo (bar)/baz/Component1.fs",
					Line = 3,
					Column = 5,
					IsError = true,
					Code = "FS0201",
					Message = "Namespaces cannot contain values.",
				}
			).SetName ("ParensInFilename");

			yield return new TestCaseData (
				"fatal error XXX: stuff.",
				new LogEvent {
					Origin = "#TASKNAME",
					IsError = true,
					Subcategory = "fatal",
					Code = "XXX",
					Message = "stuff.",
				}
			).SetName ("SubcategoryNoOrigin");
		}

		[Test]
		public void ToolExeAndPath ()
		{
			TestToolTask a = new TestToolTask ();
			Assert.AreEqual (a.ToolExe, "TestTool.exe", "#1");
			a.ToolExe = "Foo";
			Assert.AreEqual (a.ToolExe, "Foo", "#2");
			a.ToolExe = "";
			Assert.AreEqual (a.ToolExe, "TestTool.exe", "#3");

			Assert.AreEqual (a.ToolPath, null, "#4");
			a.ToolPath = "Bar";
			Assert.AreEqual (a.ToolPath, "Bar", "#5");
			a.ToolPath = "";
			Assert.AreEqual (a.ToolPath, "", "#6");

			a.Execute ();
		}

		[Test]
		public void Execute_1 ()
		{
			var t = new TestExecuteToolTask ();
			t.OnExecuteTool = delegate { Assert.Fail ("#1"); };
			t.BuildEngine = new MockBuildEngine ();
			Assert.IsFalse (t.Execute (), "result");
		}

		[Test]
		public void Execute_2 ()
		{
			var t = new TestExecuteToolTask ();
			t.BuildEngine = new MockBuildEngine ();
			var monoProcess = Process.GetCurrentProcess ().MainModule.FileName;
			t.ToolPath = Path.GetDirectoryName (monoProcess);
			t.ToolExe = Path.GetFileName (monoProcess);

			t.OnExecuteTool = (pathToTool, responseFileCommands, commandLineCommands) => {
				Assert.AreEqual (monoProcess, pathToTool, "#1");
				Assert.AreEqual ("", responseFileCommands, "#2");
				Assert.AreEqual ("", commandLineCommands, "#3");

			};

			Assert.IsTrue (t.Execute (), "result");
		}

		[Test]
		public void Execute_3 ()
		{
			var t = new TestExecuteToolTask ();
			t.FullPathToTool = "fpt";
			t.BuildEngine = new MockBuildEngine ();
			t.ToolExe = "Makefile.mk";

			t.OnExecuteTool = (pathToTool, responseFileCommands, commandLineCommands) => {
				Assert.AreEqual ("Makefile.mk", pathToTool, "#1");
				Assert.AreEqual ("", responseFileCommands, "#2");
				Assert.AreEqual ("", commandLineCommands, "#3");

			};

			Assert.IsFalse (t.Execute (), "result");
		}
	}

	public class LogEvent
	{
		public string Origin { get; set; }
		public int Line { get; set; }
		public int Column { get; set; }
		public int EndLine { get; set; }
		public int EndColumn { get; set; }
		public string Subcategory { get; set; }
		public bool IsError { get; set; }
		public string Code { get; set; }
		public string Message { get; set; }
	}

	class LogEventsFromTextOutputToolTask : ToolTask {

		public List<LogEvent> LogEvents {
			get { return engine.LogEvents; }
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

		public List<LogEvent> LogEvents = new List<LogEvent> ();

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
			LogEvents.Add (null);
		}

		public void LogErrorEvent (BuildErrorEventArgs e)
		{
			LogEvents.Add (new LogEvent {
				IsError = true,
				Subcategory = e.Subcategory,
				Origin = e.File,
				Line = e.LineNumber,
				EndLine = e.EndLineNumber,
				Column = e.ColumnNumber,
				EndColumn = e.EndColumnNumber,
				Code = e.Code,
				Message = e.Message,
			});
		}

		public void LogMessageEvent (BuildMessageEventArgs e)
		{
			LogEvents.Add (null);
		}

		public void LogWarningEvent (BuildWarningEventArgs e)
		{
			LogEvents.Add (new LogEvent {
				Subcategory = e.Subcategory,
				Origin = e.File,
				Line = e.LineNumber,
				EndLine = e.EndLineNumber,
				Column = e.ColumnNumber,
				EndColumn = e.EndColumnNumber,
				Code = e.Code,
				Message = e.Message,
			});
		}
	}

	class TestToolTask : ToolTask {

		protected override string ToolName {
			get { return "TestTool.exe"; }
		}

		protected override string GenerateFullPathToTool ()
		{
			return "";
		}
	}

	class MockBuildEngine : IBuildEngine
	{
		public int ColumnNumberOfTaskNode {
			get {
				return 0;
			}
		}

		public bool ContinueOnError {
			get {
				throw new NotImplementedException ();
			}
		}

		public int LineNumberOfTaskNode {
			get {
				return 0;
			}
		}

		public string ProjectFileOfTaskNode {
			get {
				return "ProjectFileOfTaskNode";
			}
		}

		public bool BuildProjectFile (string projectFileName, string[] targetNames, IDictionary globalProperties, IDictionary targetOutputs)
		{
			throw new NotImplementedException ();
		}

		public void LogCustomEvent (CustomBuildEventArgs e)
		{
		}

		public void LogErrorEvent (BuildErrorEventArgs e)
		{
			Console.WriteLine (e.Message);
		}

		public void LogMessageEvent (BuildMessageEventArgs e)
		{
		}

		public void LogWarningEvent (BuildWarningEventArgs e)
		{
		}
	}

	class TestExecuteToolTask : ToolTask
	{
		public Action<string, string, string> OnExecuteTool;
		public string FullPathToTool;

		protected override string ToolName {
			get { return "TestTool.exe"; }
		}

		protected override bool CallHostObjectToExecute ()
		{
			return base.CallHostObjectToExecute ();
		}

		protected override string GenerateFullPathToTool ()
		{
			return FullPathToTool;
		}

		protected override int ExecuteTool (string pathToTool, string responseFileCommands, string commandLineCommands)
		{
			if (OnExecuteTool != null)
				OnExecuteTool (pathToTool, responseFileCommands, commandLineCommands);

			return 0;
		}
	}
}

