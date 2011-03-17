//
// MessageTest.cs
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


	[TestFixture]
	public class MessageTest {
	
		Engine engine;
		Project project;
		TestMessageLogger testLogger;

		[Test]
		public void TestDefaultValues()
		{
			Message message = new Message();
			Assert.AreEqual(null, message.Text, "A1");
			Assert.AreEqual(null, message.Importance, "A2");
		}
		
		[Test]
		public void TestAssignment ()
		{
			string importance = "importance";
			string text = "text";
			
			Message message = new Message ();
			
			message.Importance = importance;
			message.Text = text;
			
			Assert.AreEqual (importance, message.Importance, "A1");
			Assert.AreEqual (text, message.Text, "A2");
		}
		
		[Test]
		public void TestExecution ()
		{
			string documentString = @"
                                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<Target Name='1'>
						<Message Text='Text1' Importance='Low'/>
						<Message Text='Text2' Importance='Normal'/>
						<Message Text='Text3' Importance='High'/>
						<Message Text='Text4' Importance='low'/>
						<Message Text='Text5' Importance='normal'/>
						<Message Text='Text6' Importance='high'/>
						<Message Text='Text7' />
						<Message Text='Text8' Importance='weird_importance'/>
					</Target>
				</Project>
			";
			
			engine = new Engine (Consts.BinPath);
			testLogger = new TestMessageLogger ();
			engine.RegisterLogger (testLogger);
			
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);
			if (project.Build ("1")) {
				testLogger.DumpMessages ();
				Assert.Fail ("Build should have failed");
			}
			
			Assert.AreEqual (0, testLogger.CheckAny ("Text1", MessageImportance.Low), "A1");
			Assert.AreEqual (0, testLogger.CheckAny ("Text2", MessageImportance.Normal), "A2");
			Assert.AreEqual (0, testLogger.CheckAny ("Text3", MessageImportance.High), "A3");
			Assert.AreEqual (0, testLogger.CheckAny ("Text4", MessageImportance.Low), "A4");
			Assert.AreEqual (0, testLogger.CheckAny ("Text5", MessageImportance.Normal), "A5");
			Assert.AreEqual (0, testLogger.CheckAny ("Text6", MessageImportance.High), "A6");
			Assert.AreEqual (0, testLogger.CheckAny ("Text7", MessageImportance.Normal), "A7");
			Assert.AreEqual (1, testLogger.CheckAny ("Text8", MessageImportance.Normal), "A8");
		}
	}
}	

