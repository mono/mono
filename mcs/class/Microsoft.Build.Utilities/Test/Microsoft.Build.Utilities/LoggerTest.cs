//
// TaskLoggingHelperTest.cs
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
//
// (C) 2005 Marek Sieradzki
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
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NUnit.Framework;

namespace MonoTests.Microsoft.Build.Utilities {

	class LoggerExtension : Logger {
		public LoggerExtension () : base () { }

		public override void Initialize (IEventSource eventSource)
		{
		}
	}

	[TestFixture]
	public class LoggerTest {
		[Test]
		public void TestAssignment ()
		{
			LoggerExtension le = new LoggerExtension ();

			Assert.IsNull (le.Parameters, "A1");
			Assert.AreEqual (LoggerVerbosity.Quiet, le.Verbosity, "A2");

			le.Parameters = "a;b";
			le.Verbosity = LoggerVerbosity.Detailed;

			Assert.AreEqual ("a;b", le.Parameters, "A3");
			Assert.AreEqual (LoggerVerbosity.Detailed, le.Verbosity, "A4");
		}

		[Test]
		public void TestFormatErrorEvent1 ()
		{
			LoggerExtension le = new LoggerExtension ();
			string subcategory = "subcategory";
			string code = "code";
			string file = "file";
			int lineNumber = 1;
			int columnNumber = 2;
			int endLineNumber = 3;
			int endColumnNumber = 4;
			string message = "message";
			string helpKeyword = "helpKeyword";
			string senderName = "senderName";

			BuildErrorEventArgs beea = new BuildErrorEventArgs (
				subcategory, code, file, lineNumber, columnNumber,
				endLineNumber, endColumnNumber, message, helpKeyword, senderName);

			Assert.AreEqual ("file(1,2,3,4): subcategory error code: message", le.FormatErrorEvent (beea), "A1");
		}

		[Test]
		public void TestFormatErrorEvent2 ()
		{
			LoggerExtension le = new LoggerExtension ();
			string subcategory = "subcategory";
			string code = "code";
			string file = "file";
			int lineNumber = 1;
			int columnNumber = 0;
			int endLineNumber = 0;
			int endColumnNumber = 0;
			string message = "message";
			string helpKeyword = "helpKeyword";
			string senderName = "senderName";

			BuildErrorEventArgs beea = new BuildErrorEventArgs (
				subcategory, code, file, lineNumber, columnNumber,
				endLineNumber, endColumnNumber, message, helpKeyword, senderName);

			Assert.AreEqual ("file(1): subcategory error code: message", le.FormatErrorEvent (beea), "A1");
		}

		[Test]
		public void TestFormatErrorEvent3 ()
		{
			LoggerExtension le = new LoggerExtension ();
			string subcategory = "subcategory";
			string code = "code";
			string file = "file";
			int lineNumber = 1;
			int columnNumber = 2;
			int endLineNumber = 0;
			int endColumnNumber = 0;
			string message = "message";
			string helpKeyword = "helpKeyword";
			string senderName = "senderName";

			BuildErrorEventArgs beea = new BuildErrorEventArgs (
				subcategory, code, file, lineNumber, columnNumber,
				endLineNumber, endColumnNumber, message, helpKeyword, senderName);

			Assert.AreEqual ("file(1,2): subcategory error code: message", le.FormatErrorEvent (beea), "A1");
		}

		[Test]
		public void TestFormatErrorEvent4 ()
		{
			LoggerExtension le = new LoggerExtension ();
			string subcategory = "subcategory";
			string code = "code";
			string file = "file";
			int lineNumber = 0;
			int columnNumber = 0;
			int endLineNumber = 0;
			int endColumnNumber = 0;
			string message = "message";
			string helpKeyword = "helpKeyword";
			string senderName = "senderName";

			BuildErrorEventArgs beea = new BuildErrorEventArgs (
				subcategory, code, file, lineNumber, columnNumber,
				endLineNumber, endColumnNumber, message, helpKeyword, senderName);

			Assert.AreEqual ("file : subcategory error code: message", le.FormatErrorEvent (beea), "A1");
		}

		[Test]
		public void TestFormatErrorEvent5 ()
		{
			LoggerExtension le = new LoggerExtension ();
			string subcategory = "subcategory";
			string code = "code";
			string file = "file";
			int lineNumber = 0;
			int columnNumber = 0;
			int endLineNumber = 1;
			int endColumnNumber = 0;
			string message = "message";
			string helpKeyword = "helpKeyword";
			string senderName = "senderName";

			BuildErrorEventArgs beea = new BuildErrorEventArgs (
				subcategory, code, file, lineNumber, columnNumber,
				endLineNumber, endColumnNumber, message, helpKeyword, senderName);

			Assert.AreEqual ("file : subcategory error code: message", le.FormatErrorEvent (beea), "A1");
		}
		[Test]
		public void TestFormatErrorEvent6 ()
		{
			LoggerExtension le = new LoggerExtension ();
			string subcategory = "subcategory";
			string code = "code";
			string file = "file";
			int lineNumber = 0;
			int columnNumber = 0;
			int endLineNumber = 0;
			int endColumnNumber = 1;
			string message = "message";
			string helpKeyword = "helpKeyword";
			string senderName = "senderName";

			BuildErrorEventArgs beea = new BuildErrorEventArgs (
				subcategory, code, file, lineNumber, columnNumber,
				endLineNumber, endColumnNumber, message, helpKeyword, senderName);

			Assert.AreEqual ("file : subcategory error code: message", le.FormatErrorEvent (beea), "A1");
		}
		[Test]
		public void TestFormatErrorEvent7 ()
		{
			LoggerExtension le = new LoggerExtension ();
			string subcategory = "subcategory";
			string code = "code";
			string file = "file";
			int lineNumber = 0;
			int columnNumber = 1;
			int endLineNumber = 0;
			int endColumnNumber = 0;
			string message = "message";
			string helpKeyword = "helpKeyword";
			string senderName = "senderName";

			BuildErrorEventArgs beea = new BuildErrorEventArgs (
				subcategory, code, file, lineNumber, columnNumber,
				endLineNumber, endColumnNumber, message, helpKeyword, senderName);

			Assert.AreEqual ("file : subcategory error code: message", le.FormatErrorEvent (beea), "A1");
		}

		[Test]
		public void TestFormatWarningEvent1 ()
		{
			LoggerExtension le = new LoggerExtension ();
			string subcategory = "subcategory";
			string code = "code";
			string file = "file";
			int lineNumber = 1;
			int columnNumber = 2;
			int endLineNumber = 3;
			int endColumnNumber = 4;
			string message = "message";
			string helpKeyword = "helpKeyword";
			string senderName = "senderName";

			BuildWarningEventArgs beea = new BuildWarningEventArgs (
				subcategory, code, file, lineNumber, columnNumber,
				endLineNumber, endColumnNumber, message, helpKeyword, senderName);

			Assert.AreEqual ("file(1,2,3,4): subcategory warning code: message", le.FormatWarningEvent (beea), "A1");
		}
		[Test]
		public void TestFormatWarningEvent2 ()
		{
			LoggerExtension le = new LoggerExtension ();
			string subcategory = "subcategory";
			string code = "code";
			string file = "file";
			int lineNumber = 1;
			int columnNumber = 0;
			int endLineNumber = 0;
			int endColumnNumber = 0;
			string message = "message";
			string helpKeyword = "helpKeyword";
			string senderName = "senderName";

			BuildWarningEventArgs beea = new BuildWarningEventArgs (
				subcategory, code, file, lineNumber, columnNumber,
				endLineNumber, endColumnNumber, message, helpKeyword, senderName);

			Assert.AreEqual ("file(1): subcategory warning code: message", le.FormatWarningEvent (beea), "A1");
		}
		[Test]
		public void TestFormatWarningEvent3 ()
		{
			LoggerExtension le = new LoggerExtension ();
			string subcategory = "subcategory";
			string code = "code";
			string file = "file";
			int lineNumber = 1;
			int columnNumber = 2;
			int endLineNumber = 0;
			int endColumnNumber = 0;
			string message = "message";
			string helpKeyword = "helpKeyword";
			string senderName = "senderName";

			BuildWarningEventArgs beea = new BuildWarningEventArgs (
				subcategory, code, file, lineNumber, columnNumber,
				endLineNumber, endColumnNumber, message, helpKeyword, senderName);

			Assert.AreEqual ("file(1,2): subcategory warning code: message", le.FormatWarningEvent (beea), "A1");
		}
		[Test]
		public void TestFormatWarningEvent4 ()
		{
			LoggerExtension le = new LoggerExtension ();
			string subcategory = "subcategory";
			string code = "code";
			string file = "file";
			int lineNumber = 0;
			int columnNumber = 0;
			int endLineNumber = 0;
			int endColumnNumber = 0;
			string message = "message";
			string helpKeyword = "helpKeyword";
			string senderName = "senderName";

			BuildWarningEventArgs beea = new BuildWarningEventArgs (
				subcategory, code, file, lineNumber, columnNumber,
				endLineNumber, endColumnNumber, message, helpKeyword, senderName);

			Assert.AreEqual ("file : subcategory warning code: message", le.FormatWarningEvent (beea), "A1");
		}
		[Test]
		public void TestFormatWarningEvent5 ()
		{
			LoggerExtension le = new LoggerExtension ();
			string subcategory = "subcategory";
			string code = "code";
			string file = "file";
			int lineNumber = 0;
			int columnNumber = 0;
			int endLineNumber = 1;
			int endColumnNumber = 0;
			string message = "message";
			string helpKeyword = "helpKeyword";
			string senderName = "senderName";

			BuildWarningEventArgs beea = new BuildWarningEventArgs (
				subcategory, code, file, lineNumber, columnNumber,
				endLineNumber, endColumnNumber, message, helpKeyword, senderName);

			Assert.AreEqual ("file : subcategory warning code: message", le.FormatWarningEvent (beea), "A1");
		}
		[Test]
		public void TestFormatWarningEvent6 ()
		{
			LoggerExtension le = new LoggerExtension ();
			string subcategory = "subcategory";
			string code = "code";
			string file = "file";
			int lineNumber = 0;
			int columnNumber = 0;
			int endLineNumber = 0;
			int endColumnNumber = 1;
			string message = "message";
			string helpKeyword = "helpKeyword";
			string senderName = "senderName";

			BuildWarningEventArgs beea = new BuildWarningEventArgs (
				subcategory, code, file, lineNumber, columnNumber,
				endLineNumber, endColumnNumber, message, helpKeyword, senderName);

			Assert.AreEqual ("file : subcategory warning code: message", le.FormatWarningEvent (beea), "A1");
		}
		[Test]
		public void TestFormatWarningEvent7 ()
		{
			LoggerExtension le = new LoggerExtension ();
			string subcategory = "subcategory";
			string code = "code";
			string file = "file";
			int lineNumber = 0;
			int columnNumber = 1;
			int endLineNumber = 0;
			int endColumnNumber = 0;
			string message = "message";
			string helpKeyword = "helpKeyword";
			string senderName = "senderName";

			BuildWarningEventArgs beea = new BuildWarningEventArgs (
				subcategory, code, file, lineNumber, columnNumber,
				endLineNumber, endColumnNumber, message, helpKeyword, senderName);

			Assert.AreEqual ("file : subcategory warning code: message", le.FormatWarningEvent (beea), "A1");
		}

		[Test]
		public void TestIsVerbosityAtLeast ()
		{
			LoggerExtension le = new LoggerExtension ();

			le.Verbosity = LoggerVerbosity.Quiet;
			Assert.IsTrue (le.IsVerbosityAtLeast (LoggerVerbosity.Quiet), "A1");
			Assert.IsFalse (le.IsVerbosityAtLeast (LoggerVerbosity.Minimal), "A2");

			le.Verbosity = LoggerVerbosity.Minimal;
			Assert.IsTrue (le.IsVerbosityAtLeast (LoggerVerbosity.Minimal), "A3");
			Assert.IsFalse (le.IsVerbosityAtLeast (LoggerVerbosity.Normal), "A4");

			le.Verbosity = LoggerVerbosity.Normal;
			Assert.IsTrue (le.IsVerbosityAtLeast (LoggerVerbosity.Normal), "A5");
			Assert.IsFalse (le.IsVerbosityAtLeast (LoggerVerbosity.Detailed), "A6");

			le.Verbosity = LoggerVerbosity.Detailed;
			Assert.IsTrue (le.IsVerbosityAtLeast (LoggerVerbosity.Detailed), "A7");
			Assert.IsFalse (le.IsVerbosityAtLeast (LoggerVerbosity.Diagnostic), "A8");
		}
	}
}
