//
// ErrorTest.cs
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
//
// (C) 2006 Marek Sieradzki
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
using Microsoft.Build.BuildEngine;
using Microsoft.Build.Framework;
using Microsoft.Build.Tasks;
using Microsoft.Build.Utilities;
using NUnit.Framework;

namespace MonoTests.Microsoft.Build.Tasks {

	internal class TestErrorLogger : ILogger {
		IList errors;
		
		public TestErrorLogger ()
		{
			errors = new ArrayList ();
		}
	
		public LoggerVerbosity Verbosity { get { return LoggerVerbosity.Normal; } set { } }
		
		public string Parameters { get { return null; } set { } }
		
		public void Initialize (IEventSource eventSource)
		{
			eventSource.ErrorRaised += new BuildErrorEventHandler (ErrorHandler);
		}
		
		public void Shutdown ()
		{
		}
		
		private void ErrorHandler (object sender, BuildErrorEventArgs args)
		{
			errors.Add (args);
		}
		
		public int CheckHead (string text, string helpKeyword, string code)
		{
			BuildErrorEventArgs actual;
		
			if (errors.Count > 0) {
				actual = (BuildErrorEventArgs) errors [0];
				errors.RemoveAt (0);
			} else
				return 1;
			
			if (text == actual.Message && helpKeyword == actual.HelpKeyword && code == actual.Code)
				return 0;
			else {
				return 2;
			}
		}
	}

	[TestFixture]
	public class ErrorTest {

		Engine engine;
		Project project;
		TestErrorLogger testLogger;

		[Test]
		public void TestAssignment ()
		{
			string code = "code";
			string helpKeyword = "helpKeyword";
			string text = "text";
			
			Error error = new Error ();
			
			error.Code = code;
			error.HelpKeyword = helpKeyword;
			error.Text = text;

			Assert.AreEqual (code, error.Code, "#1");
			Assert.AreEqual (helpKeyword, error.HelpKeyword, "#2");
			Assert.AreEqual (text, error.Text, "#3");
		}

		[Test]
		public void TestExecution1 ()
		{
			string documentString = @"
                                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<Target Name='1'>
						<Error Text='Text' HelpKeyword='HelpKeyword' Code='Code' />
					</Target>
				</Project>
			";
			
			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			bool result = project.Build ("1");

			Assert.AreEqual (false, result, "#1");
		}
		
		[Test]
		public void TestExecution2 ()
		{
			string documentString = @"
                                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<Target Name='1'>
						<Error Text='Text' HelpKeyword='HelpKeyword' Code='Code' />
					</Target>
				</Project>
			";
			
			engine = new Engine (Consts.BinPath);
			testLogger = new TestErrorLogger ();
			engine.RegisterLogger (testLogger);
			
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);
			project.Build ("1");
			
			Assert.AreEqual (0, testLogger.CheckHead ("Text", "HelpKeyword", "Code"), "A1");
		}

		[Test]
		public void TestExecute1 ()
		{
			Error error = new Error ();
			Assert.AreEqual (false, error.Execute (), "A1");
		}
	}
}
        
