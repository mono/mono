//
// WarningTest.cs
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

	internal class TestWarningLogger : ILogger {
		IList warnings;
		
		public TestWarningLogger ()
		{
			warnings = new ArrayList ();
		}
	
		public LoggerVerbosity Verbosity { get { return LoggerVerbosity.Normal; } set { } }
		
		public string Parameters { get { return null; } set { } }
		
		public void Initialize (IEventSource eventSource)
		{
			eventSource.WarningRaised += new BuildWarningEventHandler (WarningHandler);
		}
		
		public void Shutdown ()
		{
		}
		
		private void WarningHandler (object sender, BuildWarningEventArgs args)
		{
			if (args.Message.StartsWith ("The MSBuild engine") == false)
				warnings.Add (args);
		}
		
		public int CheckHead (string text, string helpKeyword, string code)
		{
			BuildWarningEventArgs actual;
		
			if (warnings.Count > 0) {
				actual = (BuildWarningEventArgs) warnings [0];
				warnings.RemoveAt (0);
			} else
				return 1;
			
			if (text == actual.Message && helpKeyword == actual.HelpKeyword && code == actual.Code)
				return 0;
			else {
				Console.WriteLine (actual.Message);
				return 2;
			}
		}
	}

	[TestFixture]
	public class WarningTest {
	
		Engine engine;
		Project project;
		TestWarningLogger testLogger;
		
		[Test]
		public void TestAssignment ()
		{
			string code = "code";
			string helpKeyword = "helpKeyword";
			string text = "text";
			
			Warning warning = new Warning ();
			
			warning.Code = code;
			warning.HelpKeyword = helpKeyword;
			warning.Text = text;

			Assert.AreEqual (code, warning.Code, "#1");
			Assert.AreEqual (helpKeyword, warning.HelpKeyword, "#2");
			Assert.AreEqual (text, warning.Text, "#3");
		}
		
		[Test]
		public void TestExecution ()
		{
			string documentString = @"
                                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<Target Name='1'>
						<Warning Text='Text' HelpKeyword='HelpKeyword' Code='Code' />
					</Target>
				</Project>
			";
			
			engine = new Engine (Consts.BinPath);
			testLogger = new TestWarningLogger ();
			engine.RegisterLogger (testLogger);
			
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);
			project.Build ("1");
			
			Assert.AreEqual (0, testLogger.CheckHead ("Text", "HelpKeyword", "Code"), "A1");
		}
	}
}	

